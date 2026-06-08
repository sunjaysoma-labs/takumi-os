// <copyright file="MemoryUnit.cs" company="Takumi OS">
// Copyright (c) Takumi OS. Licensed under the MIT License.
// </copyright>

using System;
using System.Collections.Generic;
using Takumi.Memory.Domain.Common;
using Takumi.Memory.Domain.Enums;

namespace Takumi.Memory.Domain.Entities;

/// <summary>
/// Atomic memory primitive (the "Knowledge Unit" defined by
/// [[takumi-os-quantized-memory]] in the project wiki). Persisted
/// to Azure Cosmos DB per DIP §6.1.
/// </summary>
/// <remarks>
/// <para>
/// The MemoryUnit is intentionally schema-light in the storage sense
/// (Cosmos is schemaless) but strongly typed in code: every field has
/// a domain meaning, and the application layer is responsible for
/// validation. Confidence is recorded on the document so downstream
/// RAG and agents can reason about trust.
/// </para>
/// <para>
/// Invariants:
/// <list type="bullet">
/// <item><see cref="TenantId"/> is required and matches the storage
/// partition key (enforced by the repository).</item>
/// <item><see cref="MemoryType"/> is required.</item>
/// <item><see cref="Source"/> identifies the upstream system and
/// external id; combined with <see cref="TenantId"/> it forms the
/// natural dedup key.</item>
/// <item><see cref="ConfidenceScore"/> is in [0, 1].</item>
/// </list>
/// </para>
/// </remarks>
public sealed class MemoryUnit
{
    /// <summary>
    /// Cosmos document id. Format: <c>mu_&lt;guid&gt;</c> per DIP §6.1.
    /// </summary>
    public string Id { get; private set; }

    /// <summary>Tenant partition key. Must match every other field that is
    /// tenant-scoped.</summary>
    public TenantId TenantId { get; private set; }

    /// <summary>Stable business identifier (GUID). Distinct from <see cref="Id"/>
    /// in case a MemoryUnit is migrated across containers or stores.</summary>
    public Guid MemoryId { get; private set; }

    public MemoryType MemoryType { get; private set; }

    public MemoryCategory MemoryCategory { get; private set; }

    public MemorySource Source { get; private set; }

    public string Context { get; private set; }

    public MemoryActor Actor { get; private set; }

    public string Event { get; private set; }

    public string? Decision { get; private set; }

    public string? Outcome { get; private set; }

    /// <summary>Calibrated confidence in [0, 1]. See the
    /// <c>takumi-os-quantized-memory</c> wiki page for how this is
    /// computed.</summary>
    public double ConfidenceScore { get; private set; }

    /// <summary>Vector embedding produced by the quantisation stage.
    /// Stored inline for the M0 dev path; will move to dedicated vector
    /// storage in R1.</summary>
    public IReadOnlyList<float> Embedding { get; private set; }

    /// <summary>Edges into the knowledge graph. Empty when the unit has
    /// not been linked yet (M0: links are best-effort, M1: required).</summary>
    public IReadOnlyList<GraphLink> GraphLinks { get; private set; }

    public IReadOnlyList<string> Tags { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public DateTimeOffset UpdatedAt { get; private set; }

    /// <summary>
    /// Reconstruct a MemoryUnit from a persistence layer (e.g. when
    /// reading from Cosmos). Skips the invariant checks of
    /// <see cref="Create"/> on the assumption the storage layer
    /// already validated the data; the persistence mapper is
    /// responsible for shape correctness.
    /// </summary>
    public static MemoryUnit Rehydrate(
        string id,
        TenantId tenantId,
        Guid memoryId,
        MemoryType memoryType,
        MemoryCategory memoryCategory,
        MemorySource source,
        string context,
        MemoryActor actor,
        string @event,
        string? decision,
        string? outcome,
        double confidenceScore,
        IReadOnlyList<float> embedding,
        IReadOnlyList<GraphLink> graphLinks,
        IReadOnlyList<string> tags,
        DateTimeOffset createdAt,
        DateTimeOffset updatedAt)
    {
        return new MemoryUnit(
            id,
            tenantId,
            memoryId,
            memoryType,
            memoryCategory,
            source,
            context,
            actor,
            @event,
            decision,
            outcome,
            confidenceScore,
            embedding,
            graphLinks,
            tags,
            createdAt,
            updatedAt);
    }

    // Cosmos requires a parameterless constructor for materialization.
    private MemoryUnit()
    {
        Id = string.Empty;
        TenantId = default;
        MemoryId = Guid.Empty;
        MemoryType = default;
        MemoryCategory = default;
        Source = null!;
        Context = string.Empty;
        Actor = null!;
        Event = string.Empty;
        Decision = null;
        Outcome = null;
        ConfidenceScore = 0.0;
        Embedding = Array.Empty<float>();
        GraphLinks = Array.Empty<GraphLink>();
        Tags = Array.Empty<string>();
        CreatedAt = default;
        UpdatedAt = default;
    }

    private MemoryUnit(
        string id,
        TenantId tenantId,
        Guid memoryId,
        MemoryType memoryType,
        MemoryCategory memoryCategory,
        MemorySource source,
        string context,
        MemoryActor actor,
        string @event,
        string? decision,
        string? outcome,
        double confidenceScore,
        IReadOnlyList<float> embedding,
        IReadOnlyList<GraphLink> graphLinks,
        IReadOnlyList<string> tags,
        DateTimeOffset createdAt,
        DateTimeOffset updatedAt)
    {
        Id = id;
        TenantId = tenantId;
        MemoryId = memoryId;
        MemoryType = memoryType;
        MemoryCategory = memoryCategory;
        Source = source;
        Context = context;
        Actor = actor;
        Event = @event;
        Decision = decision;
        Outcome = outcome;
        ConfidenceScore = confidenceScore;
        Embedding = embedding;
        GraphLinks = graphLinks;
        Tags = tags;
        CreatedAt = createdAt;
        UpdatedAt = updatedAt;
    }

    /// <summary>
    /// Construct a new MemoryUnit for persistence. Performs the
    /// domain-level invariant checks; the application layer is
    /// expected to have already validated user input.
    /// </summary>
    public static MemoryUnit Create(
        TenantId tenantId,
        MemoryType memoryType,
        MemoryCategory memoryCategory,
        MemorySource source,
        string context,
        MemoryActor actor,
        string @event,
        string? decision,
        string? outcome,
        double confidenceScore,
        IReadOnlyList<float>? embedding = null,
        IReadOnlyList<GraphLink>? graphLinks = null,
        IReadOnlyList<string>? tags = null,
        DateTimeOffset? now = null)
    {
        if (tenantId.Value == Guid.Empty)
        {
            throw new ArgumentException("TenantId is required.", nameof(tenantId));
        }

        ArgumentNullException.ThrowIfNull(source);

        ArgumentNullException.ThrowIfNull(actor);

        if (string.IsNullOrWhiteSpace(context))
        {
            throw new ArgumentException("Context is required.", nameof(context));
        }

        if (string.IsNullOrWhiteSpace(@event))
        {
            throw new ArgumentException("Event is required.", nameof(@event));
        }

        if (confidenceScore < 0.0 || confidenceScore > 1.0 || double.IsNaN(confidenceScore))
        {
            throw new ArgumentOutOfRangeException(
                nameof(confidenceScore),
                "ConfidenceScore must be in [0, 1].");
        }

        var timestamp = now ?? DateTimeOffset.UtcNow;
        var memoryId = Guid.NewGuid();
        var id = $"mu_{memoryId:N}";

        return new MemoryUnit(
            id,
            tenantId,
            memoryId,
            memoryType,
            memoryCategory,
            source,
            context,
            actor,
            @event,
            decision,
            outcome,
            confidenceScore,
            embedding ?? Array.Empty<float>(),
            graphLinks ?? Array.Empty<GraphLink>(),
            tags ?? Array.Empty<string>(),
            timestamp,
            timestamp);
    }
}
