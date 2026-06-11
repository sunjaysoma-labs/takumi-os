// <copyright file="CosmosOptions.cs" company="Takumi OS">
// Copyright (c) Takumi OS. Licensed under the MIT License.
// </copyright>

namespace Takumi.Memory.Infrastructure.Options;

/// <summary>
/// Bound from configuration section <c>Cosmos</c>. M0 dev uses
/// account key auth via <see cref="ConnectionString"/>; prod uses
/// AAD / managed identity via <see cref="AccountEndpoint"/>. The
/// Terraform module (TKM-M0-002) emits these as Key Vault secrets
/// in real envs; <c>appsettings.Development.json</c> keeps a local
/// dev copy.
/// </summary>
public sealed class CosmosOptions
{
    public const string SectionName = "Cosmos";

    /// <summary>Full connection string (dev only). Mutually exclusive
    /// with <see cref="AccountEndpoint"/> + managed identity.</summary>
    public string? ConnectionString { get; set; }

    /// <summary>Account endpoint URL (prod). Used together with
    /// <see cref="DatabaseName"/> when running under a managed
    /// identity (RBAC data plane).</summary>
    public string? AccountEndpoint { get; set; }

    public string DatabaseName { get; set; } = "takumi";

    public string MemoryUnitsContainer { get; set; } = "MemoryUnits";

    /// <summary>When true, the repository auto-creates the database
    /// and container on startup (M0 dev convenience). Must be false
    /// in test/prod — infrastructure is owned by Terraform
    /// (TKM-M0-003).</summary>
    public bool EnsureDatabaseOnStartup { get; set; } = true;
}
