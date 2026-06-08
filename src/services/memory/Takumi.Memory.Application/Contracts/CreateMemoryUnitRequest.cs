// <copyright file="CreateMemoryUnitRequest.cs" company="Takumi OS">
// Copyright (c) Takumi OS. Licensed under the MIT License.
// </copyright>

using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Takumi.Memory.Domain.Enums;

namespace Takumi.Memory.Application.Contracts;

/// <summary>
/// Request body for <c>POST /api/v1/memory-units</c>.
/// Mirrors DIP §6.1 with everything except the server-managed
/// fields (id, memoryId, createdAt, updatedAt, embedding, graphLinks).
/// </summary>
public sealed record CreateMemoryUnitRequest
{
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
}

/// <summary>
/// Request body for <c>POST /api/v1/memory-units/search</c>.
/// </summary>
public sealed record SearchMemoryUnitsRequest
{
    [JsonPropertyName("query")]
    public string Query { get; init; } = string.Empty;

    [JsonPropertyName("topK")]
    public int TopK { get; init; } = 10;

    [JsonPropertyName("memoryTypes")]
    public IReadOnlyList<MemoryType>? MemoryTypes { get; init; }

    [JsonPropertyName("minConfidence")]
    public double MinConfidence { get; init; } = 0.5;

    [JsonPropertyName("tags")]
    public IReadOnlyList<string>? Tags { get; init; }
}
