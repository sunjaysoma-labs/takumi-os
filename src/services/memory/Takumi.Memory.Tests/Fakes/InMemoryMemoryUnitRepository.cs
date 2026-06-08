// <copyright file="InMemoryMemoryUnitRepository.cs" company="Takumi OS">
// Copyright (c) Takumi OS. Licensed under the MIT License.
// </copyright>

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Takumi.Memory.Domain.Common;
using Takumi.Memory.Domain.Entities;
using Takumi.Memory.Domain.Repositories;

namespace Takumi.Memory.Tests.Fakes;

/// <summary>
/// In-process <see cref="IMemoryUnitRepository"/> for unit tests.
/// Enforces the same tenant-isolation invariants as the Cosmos
/// implementation, so any test that passes against this fake is
/// also a tenant-isolation test.
/// </summary>
public sealed class InMemoryMemoryUnitRepository : IMemoryUnitRepository
{
    private readonly ConcurrentDictionary<(TenantId Tenant, string Id), MemoryUnit> _byId = new();
    private readonly ConcurrentDictionary<
        (TenantId Tenant, Guid SourceSystemId, string ExternalId),
        string> _bySource = new();

    public IReadOnlyCollection<MemoryUnit> All =>
        _byId.Values.ToArray();

    public Task<MemoryUnit> CreateAsync(
        MemoryUnit unit,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(unit);

        var key = (unit.TenantId, unit.Source.SourceSystemId, unit.Source.ExternalId);
        if (!_bySource.TryAdd(key, unit.Id))
        {
            throw new DuplicateMemoryUnitException(
                unit.TenantId,
                unit.Source.SourceSystemId,
                unit.Source.ExternalId);
        }

        _byId[(unit.TenantId, unit.Id)] = unit;
        return Task.FromResult(unit);
    }

    public Task<MemoryUnit?> GetAsync(
        TenantId tenantId,
        Guid memoryId,
        CancellationToken cancellationToken = default)
    {
        var id = $"mu_{memoryId:N}";
        _byId.TryGetValue((tenantId, id), out var unit);
        return Task.FromResult<MemoryUnit?>(unit);
    }

    public Task<MemoryUnitPage> ListAsync(
        TenantId tenantId,
        MemoryUnitListOptions options,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(options);
        var limit = options.Limit <= 0 ? 50 : Math.Min(options.Limit, 100);

        var query = _byId.Values
            .Where(u => u.TenantId == tenantId)
            .Where(u => options.MemoryType is null || u.MemoryType == options.MemoryType)
            .Where(u => options.MemoryCategory is null || u.MemoryCategory == options.MemoryCategory);

        if (options.Tags is { Count: > 0 })
        {
            var required = new HashSet<string>(options.Tags, StringComparer.OrdinalIgnoreCase);
            query = query.Where(u => u.Tags.Any(t => required.Contains(t)));
        }

        query = options.Sort switch
        {
            ListSortOrder.CreatedAtAsc => query.OrderBy(u => u.CreatedAt),
            ListSortOrder.ConfidenceDesc => query.OrderByDescending(u => u.ConfidenceScore),
            ListSortOrder.UpdatedAtDesc => query.OrderByDescending(u => u.UpdatedAt),
            _ => query.OrderByDescending(u => u.CreatedAt),
        };

        var items = query.Take(limit).ToArray();
        var hasMore = items.Length == limit;
        int? total = options.IncludeTotal
            ? _byId.Values.Count(u => u.TenantId == tenantId)
            : null;

        return Task.FromResult(new MemoryUnitPage(items, null, hasMore, total));
    }

    public Task<IReadOnlyList<MemoryUnitSearchHit>> SearchAsync(
        TenantId tenantId,
        MemoryUnitSearchOptions options,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(options);
        if (string.IsNullOrWhiteSpace(options.Query))
        {
            throw new ArgumentException("Query is required.", nameof(options));
        }

        var topK = options.TopK <= 0 ? 10 : Math.Min(options.TopK, 100);
        var q = options.Query;
        var minConf = options.MinimumConfidence;
        var typeSet = options.MemoryTypes is { Count: > 0 }
            ? new HashSet<Domain.Enums.MemoryType>(options.MemoryTypes) : null;
        var tagSet = options.Tags is { Count: > 0 }
            ? new HashSet<string>(options.Tags, StringComparer.OrdinalIgnoreCase) : null;

        var hits = _byId.Values
            .Where(u => u.TenantId == tenantId)
            .Where(u => u.ConfidenceScore >= minConf)
            .Where(u => typeSet is null || typeSet.Contains(u.MemoryType))
            .Where(u => tagSet is null || u.Tags.Any(t => tagSet.Contains(t)))
            .Select(u => (Unit: u, Score: ScoreLexical(u, q)))
            .Where(t => t.Score > 0)
            .OrderByDescending(t => t.Score)
            .Take(topK)
            .Select(t => new MemoryUnitSearchHit(t.Unit, t.Score))
            .ToArray();

        return Task.FromResult<IReadOnlyList<MemoryUnitSearchHit>>(hits);
    }

    private static double ScoreLexical(MemoryUnit u, string q)
    {
        double s = 0;
        if (u.Context.Contains(q, StringComparison.OrdinalIgnoreCase)) s += 1.5;
        if (u.Event.Contains(q, StringComparison.OrdinalIgnoreCase)) s += 1.0;
        if (u.Decision is not null
            && u.Decision.Contains(q, StringComparison.OrdinalIgnoreCase)) s += 1.2;
        if (u.Outcome is not null
            && u.Outcome.Contains(q, StringComparison.OrdinalIgnoreCase)) s += 1.0;
        s += u.ConfidenceScore * 0.25;
        return s;
    }
}
