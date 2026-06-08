// <copyright file="CosmosMemoryUnitRepository.cs" company="Takumi OS">
// Copyright (c) Takumi OS. Licensed under the MIT License.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Takumi.Memory.Domain.Common;
using Takumi.Memory.Domain.Enums;
using Takumi.Memory.Domain.Repositories;
using Takumi.Memory.Infrastructure.Options;
using Takumi.Memory.Infrastructure.Persistence;

namespace Takumi.Memory.Infrastructure.Persistence;

/// <summary>
/// Azure Cosmos DB (SQL API) implementation of
/// <see cref="IMemoryUnitRepository"/>.
///
/// <para>
/// Tenant isolation is enforced by:
/// <list type="number">
/// <item>Always scoping reads/writes to
/// <c>Container.GetItemLinqQueryable&lt;MemoryUnitDocument&gt;()
/// Where(d =&gt; d.TenantId == tenantId.Value.ToString())</c></item>
/// <item>Setting <c>PartitionKey</c> on every single-item read to
/// the tenant id, so the call is partition-scoped and the
/// read-token never crosses tenants.</item>
/// <item>Insert with a pre-flight existence check on
/// <c>(tenantId, source.sourceSystemId, source.externalId)</c> per
/// DIP §6.2 dedup index, throwing
/// <see cref="DuplicateMemoryUnitException"/> on collision.</item>
/// </list>
/// </para>
/// </summary>
public sealed class CosmosMemoryUnitRepository : IMemoryUnitRepository
{
    private readonly CosmosClient _client;
    private readonly CosmosOptions _options;
    private readonly ILogger<CosmosMemoryUnitRepository> _logger;
    private Container? _container;

    public CosmosMemoryUnitRepository(
        CosmosClient client,
        IOptions<CosmosOptions> options,
        ILogger<CosmosMemoryUnitRepository> logger)
    {
        _client = client ?? throw new ArgumentNullException(nameof(client));
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    private Container Container =>
        _container ??= _client.GetContainer(_options.DatabaseName, _options.MemoryUnitsContainer);

    public async Task<Domain.Entities.MemoryUnit> CreateAsync(
        Domain.Entities.MemoryUnit unit,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(unit);

        var tenantIdValue = unit.TenantId.Value.ToString();
        var partitionKey = new PartitionKey(tenantIdValue);

        // Pre-flight dedup check (matches DIP §6.2 composite index on
        // (tenantId, source.sourceSystemId, source.externalId)).
        var collisionQuery = Container
            .GetItemLinqQueryable<MemoryUnitDocument>(requestOptions: new QueryRequestOptions
            {
                PartitionKey = partitionKey,
            })
            .Where(d =>
                d.TenantId == tenantIdValue
                && d.Source.SourceSystemId == unit.Source.SourceSystemId
                && d.Source.ExternalId == unit.Source.ExternalId)
            .Take(1);

        using var feed = collisionQuery.ToFeedIterator();
        if (feed.HasMoreResults)
        {
            var first = await feed.ReadNextAsync(cancellationToken).ConfigureAwait(false);
            if (first.Count > 0)
            {
                throw new DuplicateMemoryUnitException(
                    unit.TenantId,
                    unit.Source.SourceSystemId,
                    unit.Source.ExternalId);
            }
        }

        var doc = MemoryUnitDocumentMapper.ToDocument(unit);
        var response = await Container
            .CreateItemAsync(doc, partitionKey, cancellationToken: cancellationToken)
            .ConfigureAwait(false);

        _logger.LogInformation(
            "Created MemoryUnit {MemoryId} for tenant {TenantId} (ru={Ru})",
            unit.MemoryId,
            unit.TenantId,
            response.RequestCharge);

        var hydrated = MemoryUnitDocumentMapper.ToDomain(response.Resource);
        return hydrated ?? throw new InvalidOperationException(
            "Cosmos returned a null resource after create.");
    }

    public async Task<Domain.Entities.MemoryUnit?> GetAsync(
        TenantId tenantId,
        Guid memoryId,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(tenantId);
        if (memoryId == Guid.Empty)
        {
            throw new ArgumentException("memoryId is required.", nameof(memoryId));
        }

        // Read by composite id: id = mu_<memoryIdN>, partition = tenantId.
        // Scoping by partition is the tenant-isolation guarantee.
        var id = $"mu_{memoryId:N}";
        var partitionKey = new PartitionKey(tenantId.Value.ToString());

        try
        {
            var response = await Container
                .ReadItemAsync<MemoryUnitDocument>(id, partitionKey, cancellationToken: cancellationToken)
                .ConfigureAwait(false);
            return MemoryUnitDocumentMapper.ToDomain(response.Resource);
        }
        catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }
    }

    public async Task<MemoryUnitPage> ListAsync(
        TenantId tenantId,
        MemoryUnitListOptions options,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(tenantId);
        ArgumentNullException.ThrowIfNull(options);

        var partitionKey = new PartitionKey(tenantId.Value.ToString());
        var limit = options.Limit <= 0 ? 50 : Math.Min(options.Limit, 100);

        var queryable = Container
            .GetItemLinqQueryable<MemoryUnitDocument>(requestOptions: new QueryRequestOptions
            {
                PartitionKey = partitionKey,
                MaxItemCount = limit,
            })
            .Where(d => d.TenantId == tenantId.Value.ToString());

        if (options.MemoryType is { } mt)
        {
            var mtValue = mt;
            queryable = queryable.Where(d => d.MemoryType == mtValue);
        }

        if (options.MemoryCategory is { } mc)
        {
            var mcValue = mc;
            queryable = queryable.Where(d => d.MemoryCategory == mcValue);
        }

        queryable = options.Sort switch
        {
            ListSortOrder.CreatedAtAsc => queryable.OrderBy(d => d.CreatedAt),
            ListSortOrder.ConfidenceDesc => queryable.OrderByDescending(d => d.ConfidenceScore),
            ListSortOrder.UpdatedAtDesc => queryable.OrderByDescending(d => d.UpdatedAt),
            _ => queryable.OrderByDescending(d => d.CreatedAt),
        };

        var iterator = queryable.ToFeedIterator();
        var items = new List<Domain.Entities.MemoryUnit>(capacity: limit);

        while (iterator.HasMoreResults && items.Count < limit)
        {
            var next = await iterator.ReadNextAsync(cancellationToken).ConfigureAwait(false);
            foreach (var doc in next)
            {
                var domain = MemoryUnitDocumentMapper.ToDomain(doc);
                if (domain is not null)
                {
                    items.Add(domain);
                    if (items.Count >= limit)
                    {
                        break;
                    }
                }
            }
        }

        // Cursor is opaque for M0: the next page starts at the last
        // item's createdAt. R1 will replace with a proper continuation
        // token to avoid skipping items written concurrently.
        var hasMore = items.Count == limit;
        string? nextCursor = hasMore && items.Count > 0
            ? items[^1].CreatedAt.UtcTicks.ToString()
            : null;

        int? total = null;
        if (options.IncludeTotal)
        {
            // Cheap count: separate cross-partition query, scoped to tenant.
            // Acceptable for M0 admin endpoints; cache in R1.
            var countQuery = Container
                .GetItemLinqQueryable<MemoryUnitDocument>(requestOptions: new QueryRequestOptions
                {
                    PartitionKey = partitionKey,
                })
                .Where(d => d.TenantId == tenantId.Value.ToString());
            var count = 0;
            var countIterator = countQuery.Select(d => 1).ToFeedIterator();
            while (countIterator.HasMoreResults)
            {
                var page = await countIterator.ReadNextAsync(cancellationToken).ConfigureAwait(false);
                count += page.Count;
            }
            total = count;
        }

        return new MemoryUnitPage(items, nextCursor, hasMore, total);
    }

    public async Task<IReadOnlyList<MemoryUnitSearchHit>> SearchAsync(
        TenantId tenantId,
        MemoryUnitSearchOptions options,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(tenantId);
        ArgumentNullException.ThrowIfNull(options);
        if (string.IsNullOrWhiteSpace(options.Query))
        {
            throw new ArgumentException("Query is required.", nameof(options));
        }

        // M0 hybrid: keyword match across Context/Event/Decision/Outcome
        // (case-insensitive contains), ranked by lexical hit count +
        // confidence. Vector search is wired in TKM-M0-016 once the
        // AI Search index is provisioned.
        var partitionKey = new PartitionKey(tenantId.Value.ToString());
        var topK = options.TopK <= 0 ? 10 : Math.Min(options.TopK, 100);
        var queryText = options.Query.Trim();
        var minConfidence = options.MinimumConfidence;
        var memoryTypes = options.MemoryTypes is { Count: > 0 }
            ? new HashSet<MemoryType>(options.MemoryTypes)
            : null;
        var tags = options.Tags is { Count: > 0 }
            ? new HashSet<string>(options.Tags, StringComparer.OrdinalIgnoreCase)
            : null;

        var queryable = Container
            .GetItemLinqQueryable<MemoryUnitDocument>(requestOptions: new QueryRequestOptions
            {
                PartitionKey = partitionKey,
                MaxItemCount = topK * 4, // over-fetch; rank + filter
            })
            .Where(d => d.TenantId == tenantId.Value.ToString())
            .Where(d =>
                d.Context.Contains(queryText)
                || d.Event.Contains(queryText)
                || (d.Decision != null && d.Decision.Contains(queryText))
                || (d.Outcome != null && d.Outcome.Contains(queryText)))
            .Where(d => d.ConfidenceScore >= minConfidence);

        if (memoryTypes is not null)
        {
            queryable = queryable.Where(d => memoryTypes.Contains(d.MemoryType));
        }

        var iterator = queryable.ToFeedIterator();
        var raw = new List<MemoryUnitDocument>();
        while (iterator.HasMoreResults)
        {
            var page = await iterator.ReadNextAsync(cancellationToken).ConfigureAwait(false);
            raw.AddRange(page);
            if (raw.Count >= topK * 4)
            {
                break;
            }
        }

        var ranked = raw
            .Select(d => (Doc: d, Score: ScoreHit(d, queryText, tags)))
            .Where(t => t.Score > 0.0)
            .OrderByDescending(t => t.Score)
            .Take(topK)
            .ToList();

        var hits = new List<MemoryUnitSearchHit>(ranked.Count);
        foreach (var (doc, score) in ranked)
        {
            var unit = MemoryUnitDocumentMapper.ToDomain(doc);
            if (unit is not null)
            {
                hits.Add(new MemoryUnitSearchHit(unit, score));
            }
        }
        return hits;
    }

    private static double ScoreHit(
        MemoryUnitDocument doc,
        string query,
        HashSet<string>? tagFilter)
    {
        if (tagFilter is { Count: > 0 }
            && !doc.Tags.Any(t => tagFilter.Contains(t)))
        {
            return 0.0;
        }

        // Cheap lexical scoring: 1 point per substring hit across the
        // four searchable fields, weighted by field (Context = 1.5,
        // Event = 1.0, Decision = 1.2, Outcome = 1.0), plus a small
        // confidence lift. R1 replaces with semantic + lexical blend.
        double score = 0.0;
        if (doc.Context.Contains(query, StringComparison.OrdinalIgnoreCase)) score += 1.5;
        if (doc.Event.Contains(query, StringComparison.OrdinalIgnoreCase)) score += 1.0;
        if (doc.Decision is not null
            && doc.Decision.Contains(query, StringComparison.OrdinalIgnoreCase)) score += 1.2;
        if (doc.Outcome is not null
            && doc.Outcome.Contains(query, StringComparison.OrdinalIgnoreCase)) score += 1.0;
        score += doc.ConfidenceScore * 0.25;
        return score;
    }
}
