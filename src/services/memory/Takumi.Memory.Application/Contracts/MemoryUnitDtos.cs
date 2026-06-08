// <copyright file="MemoryUnitDtos.cs" company="Takumi OS">
// Copyright (c) Takumi OS. Licensed under the MIT License.
// </copyright>

using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Takumi.Memory.Domain.Enums;

namespace Takumi.Memory.Application.Contracts;

/// <summary>
/// Wire-format representation of a <c>MemoryUnit</c>. Decoupled from
/// the domain entity so we can evolve storage layout (Cosmos) without
/// breaking the API contract. All fields are present even when null
/// so the generated TypeScript types stay stable.
/// </summary>
public sealed record MemoryUnitResponse
{
    [JsonPropertyName("id")]
    public string Id { get; init; } = string.Empty;

    [JsonPropertyName("tenantId")]
    public Guid TenantId { get; init; }

    [JsonPropertyName("memoryId")]
    public Guid MemoryId { get; init; }

    [JsonPropertyName("memoryType")]
    public MemoryType MemoryType { get; init; }

    [JsonPropertyName("memoryCategory")]
    public MemoryCategory MemoryCategory { get; init; }

    [JsonPropertyName("source")]
    public MemorySourceResponse Source { get; init; } = new();

    [JsonPropertyName("context")]
    public string Context { get; init; } = string.Empty;

    [JsonPropertyName("actor")]
    public MemoryActorResponse Actor { get; init; } = new();

    [JsonPropertyName("event")]
    public string Event { get; init; } = string.Empty;

    [JsonPropertyName("decision")]
    public string? Decision { get; init; }

    [JsonPropertyName("outcome")]
    public string? Outcome { get; init; }

    [JsonPropertyName("confidenceScore")]
    public double ConfidenceScore { get; init; }

    [JsonPropertyName("tags")]
    public IReadOnlyList<string> Tags { get; init; } = Array.Empty<string>();

    [JsonPropertyName("graphLinks")]
    public IReadOnlyList<GraphLinkResponse> GraphLinks { get; init; } = Array.Empty<GraphLinkResponse>();

    [JsonPropertyName("createdAt")]
    public DateTimeOffset CreatedAt { get; init; }

    [JsonPropertyName("updatedAt")]
    public DateTimeOffset UpdatedAt { get; init; }
}

public sealed record MemorySourceResponse
{
    [JsonPropertyName("sourceSystemId")]
    public Guid SourceSystemId { get; init; }

    [JsonPropertyName("externalId")]
    public string ExternalId { get; init; } = string.Empty;

    [JsonPropertyName("url")]
    public string? Url { get; init; }
}

public sealed record MemoryActorResponse
{
    [JsonPropertyName("type")]
    public ActorType Type { get; init; }

    [JsonPropertyName("id")]
    public Guid Id { get; init; }

    [JsonPropertyName("name")]
    public string Name { get; init; } = string.Empty;
}

public sealed record GraphLinkResponse
{
    [JsonPropertyName("nodeType")]
    public string NodeType { get; init; } = string.Empty;

    [JsonPropertyName("nodeId")]
    public Guid NodeId { get; init; }

    [JsonPropertyName("edgeType")]
    public string EdgeType { get; init; } = string.Empty;
}

/// <summary>
/// Single hit in a search response. Adds a relevance score
/// alongside the unit so the UI can rank and badge low-confidence
/// results.
/// </summary>
public sealed record MemoryUnitSearchHitResponse
{
    [JsonPropertyName("unit")]
    public MemoryUnitResponse Unit { get; init; } = new();

    [JsonPropertyName("score")]
    public double Score { get; init; }
}

public sealed record MemoryUnitListResponse
{
    [JsonPropertyName("items")]
    public IReadOnlyList<MemoryUnitResponse> Items { get; init; } = Array.Empty<MemoryUnitResponse>();

    [JsonPropertyName("nextCursor")]
    public string? NextCursor { get; init; }

    [JsonPropertyName("hasMore")]
    public bool HasMore { get; init; }

    [JsonPropertyName("totalCount")]
    public int? TotalCount { get; init; }
}
