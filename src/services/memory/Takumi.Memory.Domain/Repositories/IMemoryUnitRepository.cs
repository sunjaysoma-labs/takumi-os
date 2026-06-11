// <copyright file="IMemoryUnitRepository.cs" company="Takumi OS">
// Copyright (c) Takumi OS. Licensed under the MIT License.
// </copyright>

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Takumi.Memory.Domain.Common;
using Takumi.Memory.Domain.Enums;

namespace Takumi.Memory.Domain.Repositories;

/// <summary>
/// Persistence contract for <see cref="Entities.MemoryUnit"/>.
/// Implementations MUST enforce tenant isolation at the query
/// boundary (Cosmos partition key = <see cref="TenantId"/>) and MUST
/// reject any call that does not pass a non-empty tenant id.
/// </summary>
public interface IMemoryUnitRepository
{
    /// <summary>
    /// Persist a new unit. Returns the materialized unit (with id,
    /// timestamps) so the caller can echo it back. Throws
    /// <see cref="DuplicateMemoryUnitException"/> if a unit with the
    /// same (tenantId, source.sourceSystemId, source.externalId)
    /// already exists — dedup is a contract, not a happy accident.
    /// </summary>
    Task<Entities.MemoryUnit> CreateAsync(
        Entities.MemoryUnit unit,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Read by tenant + memoryId. Returns null if not found. The
    /// query MUST include the tenant id in the WHERE clause even
    /// though <paramref name="memoryId"/> is unique; this is the
    /// tenant-isolation invariant.
    /// </summary>
    Task<Entities.MemoryUnit?> GetAsync(
        TenantId tenantId,
        System.Guid memoryId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Cursor-paginated list with optional filters. Implementation
    /// must honour the cursor and never return more than
    /// <see cref="MemoryUnitListOptions.Limit"/> results.
    /// </summary>
    Task<MemoryUnitPage> ListAsync(
        TenantId tenantId,
        MemoryUnitListOptions options,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Hybrid (vector + keyword) search. Returns results sorted by
    /// descending relevance score, optionally constrained by
    /// <see cref="MemoryUnitSearchOptions.MinimumConfidence"/> and
    /// <see cref="MemoryUnitSearchOptions.MemoryTypes"/>.
    /// M0 returns lexical + embedding cosine blended; R1 adds
    /// semantic ranker (DIP §8.1).
    /// </summary>
    Task<IReadOnlyList<MemoryUnitSearchHit>> SearchAsync(
        TenantId tenantId,
        MemoryUnitSearchOptions options,
        CancellationToken cancellationToken = default);
}

/// <summary>Cursor-paginated result envelope.</summary>
public sealed record MemoryUnitPage(
    IReadOnlyList<Entities.MemoryUnit> Items,
    string? NextCursor,
    bool HasMore,
    int? TotalCount);

/// <summary>A single search hit with its relevance score.</summary>
public sealed record MemoryUnitSearchHit(
    Entities.MemoryUnit Unit,
    double Score);

/// <summary>Options that drive <see cref="IMemoryUnitRepository.ListAsync"/>.</summary>
public sealed record MemoryUnitListOptions
{
    public MemoryType? MemoryType { get; init; }

    public MemoryCategory? MemoryCategory { get; init; }

    public IReadOnlyList<string>? Tags { get; init; }

    public int Limit { get; init; } = 50;

    public string? Cursor { get; init; }

    public bool IncludeTotal { get; init; }

    /// <summary>Order by createdAt descending unless overridden.</summary>
    public ListSortOrder Sort { get; init; } = ListSortOrder.CreatedAtDesc;
}

/// <summary>Options for <see cref="IMemoryUnitRepository.SearchAsync"/>.</summary>
public sealed record MemoryUnitSearchOptions
{
    public required string Query { get; init; }

    public int TopK { get; init; } = 10;

    public IReadOnlyList<MemoryType>? MemoryTypes { get; init; }

    public double MinimumConfidence { get; init; } = 0.5;

    public IReadOnlyList<string>? Tags { get; init; }
}

public enum ListSortOrder
{
    CreatedAtDesc = 0,
    CreatedAtAsc = 1,
    ConfidenceDesc = 2,
    UpdatedAtDesc = 3,
}

/// <summary>
/// Thrown by <see cref="IMemoryUnitRepository.CreateAsync"/> when
/// the (tenantId, sourceSystemId, externalId) tuple collides with
/// an existing unit. The application layer maps this to HTTP 409.
/// </summary>
public sealed class DuplicateMemoryUnitException : System.Exception
{
    public TenantId TenantId { get; }

    public System.Guid SourceSystemId { get; }

    public string ExternalId { get; }

    public DuplicateMemoryUnitException(
        TenantId tenantId,
        System.Guid sourceSystemId,
        string externalId)
        : base(
            $"A MemoryUnit for tenant {tenantId}, source {sourceSystemId}, " +
            $"externalId '{externalId}' already exists.")
    {
        TenantId = tenantId;
        SourceSystemId = sourceSystemId;
        ExternalId = externalId;
    }
}
