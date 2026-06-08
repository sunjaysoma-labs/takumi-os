// <copyright file="MemoryApiFactory.cs" company="Takumi OS">
// Copyright (c) Takumi OS. Licensed under the MIT License.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Takumi.Memory.Domain.Common;
using Takumi.Memory.Domain.Contexts;
using Takumi.Memory.Domain.Repositories;
using Takumi.Memory.Tests.Fakes;

namespace Takumi.Memory.Tests.Api;

/// <summary>
/// <see cref="WebApplicationFactory{TEntryPoint}"/> for the Memory
/// API. Replaces the real Cosmos client + Entra ID auth with
/// in-memory doubles so we can drive the full ASP.NET pipeline in
/// a unit test without external dependencies.
/// </summary>
public sealed class MemoryApiFactory : WebApplicationFactory<Program>
{
    public InMemoryMemoryUnitRepository Repository { get; } = new();

    public TenantId TestTenant { get; } = TenantId.New();

    protected override IHost CreateHost(IHostBuilder builder)
    {
        builder.ConfigureHostConfiguration(cfg =>
        {
            cfg.AddInMemoryCollection(new Dictionary<string, string?>
            {
                // Disable real Cosmos startup ensure; we use the
                // in-memory repo.
                ["Cosmos:EnsureDatabaseOnStartup"] = "false",
                // Make the health check happy even without Cosmos.
                ["Cosmos:ConnectionString"] = "AccountEndpoint=https://localhost:8081/;AccountKey=fake",
                // Suppress AzureAd config validation in test host.
                ["AzureAd:ClientId"] = "00000000-0000-0000-0000-000000000000",
                ["AzureAd:TenantId"] = "11111111-1111-1111-1111-111111111111",
            });
        });

        builder.ConfigureServices(services =>
        {
            // Replace repository
            services.RemoveAll<IMemoryUnitRepository>();
            services.AddSingleton<IMemoryUnitRepository>(Repository);

            // Replace tenant context with our fake, populated per-request
            services.RemoveAll<ITenantContext>();
            services.AddScoped<ITenantContext, FakeTenantContext>();

            // Replace the JWT bearer auth with a test scheme that
            // synthesises a ClaimsPrincipal from a header so we can
            // simulate tenants in tests.
            services.AddAuthentication(TestAuthHandler.SchemeName)
                .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(
                    TestAuthHandler.SchemeName, _ => { });
        });

        return base.CreateHost(builder);
    }
}

/// <summary>
/// Minimal auth handler for API tests. Reads the
/// <c>X-Test-Tenant-Id</c> header and synthesises a
/// <see cref="ClaimsPrincipal"/> with the <c>tid</c>, <c>oid</c>,
/// and <c>scp</c> claims. The handler does NOT enforce scopes — the
/// controllers do that via [Authorize(Policy=...)]; this handler
/// only authenticates.
/// </summary>
public sealed class TestAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public const string SchemeName = "Test";

    public TestAuthHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder)
        : base(options, logger, encoder)
    {
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var tenantHeader = Request.Headers["X-Test-Tenant-Id"].ToString();
        if (string.IsNullOrWhiteSpace(tenantHeader))
        {
            return Task.FromResult(AuthenticateResult.NoResult());
        }

        var claims = new List<Claim>
        {
            new("tid", tenantHeader),
            new("oid", Guid.NewGuid().ToString()),
            new("name", "test-user"),
            new("scp", "memory.read memory.write"),
        };
        var identity = new ClaimsIdentity(claims, SchemeName);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, SchemeName);
        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}
