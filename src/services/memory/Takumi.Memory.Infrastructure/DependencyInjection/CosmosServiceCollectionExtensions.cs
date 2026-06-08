// <copyright file="CosmosServiceCollectionExtensions.cs" company="Takumi OS">
// Copyright (c) Takumi OS. Licensed under the MIT License.
// </copyright>

using System;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Takumi.Memory.Domain.Repositories;
using Takumi.Memory.Infrastructure.Options;
using Takumi.Memory.Infrastructure.Persistence;

namespace Takumi.Memory.Infrastructure.DependencyInjection;

/// <summary>
/// DI helper for wiring the Memory Service's Cosmos dependencies
/// without coupling callers to the concrete types in
/// <see cref="Persistence"/>.
/// </summary>
public static class CosmosServiceCollectionExtensions
{
    /// <summary>
    /// Register <see cref="CosmosClient"/>,
    /// <see cref="IMemoryUnitRepository"/>, and the related options.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The configuration root (or
    /// section) containing the <c>Cosmos</c> section.</param>
    public static IServiceCollection AddTakumiCosmos(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        if (services is null) throw new ArgumentNullException(nameof(services));
        if (configuration is null) throw new ArgumentNullException(nameof(configuration));

        services
            .AddOptions<CosmosOptions>()
            .Bind(configuration.GetSection(CosmosOptions.SectionName))
            .ValidateDataAnnotations();

        services.AddSingleton<CosmosClient>(sp =>
        {
            var options = sp.GetRequiredService<IOptions<CosmosOptions>>().Value;
            var logger = sp.GetRequiredService<ILogger<CosmosClient>>();

            if (!string.IsNullOrWhiteSpace(options.ConnectionString))
            {
                logger.LogInformation(
                    "Cosmos: using connection string (dev/local). Account endpoint: {Endpoint}",
                    MaskAccountEndpoint(options.ConnectionString));
                return new CosmosClient(options.ConnectionString, new CosmosClientOptions
                {
                    ApplicationName = "Takumi.Memory",
                    Serializer = new CosmosJsonDotNetSerializer(
                        new Newtonsoft.Json.JsonSerializerSettings
                        {
                            ContractResolver = new Newtonsoft.Json.Serialization.DefaultContractResolver
                            {
                                NamingStrategy = new Newtonsoft.Json.Serialization.CamelCaseNamingStrategy(),
                            },
                        }),
                });
            }

            if (!string.IsNullOrWhiteSpace(options.AccountEndpoint))
            {
                logger.LogInformation(
                    "Cosmos: using AAD (managed identity) at {Endpoint}",
                    options.AccountEndpoint);
                return new CosmosClient(
                    options.AccountEndpoint,
                    new Azure.Identity.DefaultAzureCredential(),
                    new CosmosClientOptions
                    {
                        ApplicationName = "Takumi.Memory",
                        Serializer = new CosmosJsonDotNetSerializer(
                            new Newtonsoft.Json.JsonSerializerSettings
                            {
                                ContractResolver = new Newtonsoft.Json.Serialization.DefaultContractResolver
                                {
                                    NamingStrategy = new Newtonsoft.Json.Serialization.CamelCaseNamingStrategy(),
                                },
                            }),
                    });
            }

            throw new InvalidOperationException(
                "Cosmos configuration is missing. Provide either Cosmos:ConnectionString " +
                "(dev) or Cosmos:AccountEndpoint (prod, with managed identity).");
        });

        services.AddSingleton<IMemoryUnitRepository, CosmosMemoryUnitRepository>();

        return services;
    }

    /// <summary>
    /// Optionally create the database and container on startup.
    /// Call from <c>Program.cs</c> when
    /// <see cref="CosmosOptions.EnsureDatabaseOnStartup"/> is true
    /// (M0 dev convenience). Idempotent: safe to call repeatedly.
    /// </summary>
    public static async Task EnsureCosmosDatabaseAsync(
        this IServiceProvider serviceProvider)
    {
        var options = serviceProvider
            .GetRequiredService<IOptions<CosmosOptions>>().Value;
        if (!options.EnsureDatabaseOnStartup)
        {
            return;
        }

        var client = serviceProvider.GetRequiredService<CosmosClient>();
        var logger = serviceProvider
            .GetRequiredService<ILoggerFactory>()
            .CreateLogger("CosmosBootstrap");

        var dbResponse = await client
            .CreateDatabaseIfNotExistsAsync(options.DatabaseName)
            .ConfigureAwait(false);
        logger.LogInformation(
            "Cosmos database '{Db}' status: {Status}",
            options.DatabaseName,
            dbResponse.StatusCode);

        var containerResponse = await client
            .GetDatabase(options.DatabaseName)
            .CreateContainerIfNotExistsAsync(new ContainerProperties
            {
                Id = options.MemoryUnitsContainer,
                PartitionKeyPath = "/tenantId",
                IndexingPolicy = new IndexingPolicy
                {
                    // Default indexing with composite indexes that
                    // match DIP §6.2:
                    CompositeIndexes =
                    {
                        new CompositeIndex
                        {
                            Indexes =
                            {
                                new CompositePath { Path = "/tenantId", Order = CompositePathOrder.Ascending },
                                new CompositePath { Path = "/memoryType", Order = CompositePathOrder.Ascending },
                                new CompositePath { Path = "/createdAt", Order = CompositePathOrder.Descending },
                            },
                        },
                        new CompositeIndex
                        {
                            Indexes =
                            {
                                new CompositePath { Path = "/tenantId", Order = CompositePathOrder.Ascending },
                                new CompositePath { Path = "/source/sourceSystemId", Order = CompositePathOrder.Ascending },
                                new CompositePath { Path = "/source/externalId", Order = CompositePathOrder.Ascending },
                            },
                        },
                    },
                },
            })
            .ConfigureAwait(false);
        logger.LogInformation(
            "Cosmos container '{Container}' status: {Status}",
            options.MemoryUnitsContainer,
            containerResponse.StatusCode);
    }

    private static string? MaskAccountEndpoint(string connectionString)
    {
        // Returns the AccountEndpoint portion of a Cosmos connection
        // string with the account key redacted. Never logs the key.
        foreach (var part in connectionString.Split(';', StringSplitOptions.RemoveEmptyEntries))
        {
            var eq = part.IndexOf('=');
            if (eq <= 0) continue;
            var key = part[..eq];
            if (string.Equals(key, "AccountEndpoint", StringComparison.OrdinalIgnoreCase))
            {
                return part[(eq + 1)..];
            }
        }
        return null;
    }
}
