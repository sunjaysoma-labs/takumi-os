// <copyright file="MemoryUnitDocument.cs" company="Takumi OS">
// Copyright (c) Takumi OS. Licensed under the MIT License.
// </copyright>

using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Takumi.Memory.Domain.Enums;

namespace Takumi.Memory.Infrastructure.Persistence;

/// <summary>
/// Cosmos-friendly representation of a <c>MemoryUnit</c>. The
/// partition key for the container is <see cref="TenantId"/> (as a
/// string for Cosmos); the document id is <see cref="Id"/>
/// (e.g. <c>mu_xxxxxxxx</c>). Cosmos serializes to JSON using
/// Newtonsoft by default; field casing is configured to camelCase
/// at the client level so this class uses PascalCase for code
/// clarity but writes/reads camelCase on the wire.
/// </summary>
internal sealed class MemoryUnitDocument
{
    [JsonProperty("id")]
    public string Id { get; set; } = string.Empty;

    [JsonProperty("tenantId")]
    public string TenantId { get; set; } = string.Empty;

    [JsonProperty("memoryId")]
    public Guid MemoryId { get; set; }

    [JsonProperty("memoryType")]
    public MemoryType MemoryType { get; set; }

    [JsonProperty("memoryCategory")]
    public MemoryCategory MemoryCategory { get; set; }

    [JsonProperty("source")]
    public SourceDocument Source { get; set; } = new();

    [JsonProperty("context")]
    public string Context { get; set; } = string.Empty;

    [JsonProperty("actor")]
    public ActorDocument Actor { get; set; } = new();

    [JsonProperty("event")]
    public string Event { get; set; } = string.Empty;

    [JsonProperty("decision")]
    public string? Decision { get; set; }

    [JsonProperty("outcome")]
    public string? Outcome { get; set; }

    [JsonProperty("confidenceScore")]
    public double ConfidenceScore { get; set; }

    [JsonProperty("embedding")]
    public float[]? Embedding { get; set; }

    [JsonProperty("graphLinks")]
    public List<LinkDocument> GraphLinks { get; set; } = new();

    [JsonProperty("tags")]
    public List<string> Tags { get; set; } = new();

    [JsonProperty("createdAt")]
    public DateTimeOffset CreatedAt { get; set; }

    [JsonProperty("updatedAt")]
    public DateTimeOffset UpdatedAt { get; set; }

    /// <summary>
    /// ETag concurrency token populated by Cosmos on read; written
    /// back on update. We do not include it in the public domain
    /// model — it's an infrastructure concern.
    /// </summary>
    [JsonProperty("_etag")]
    public string? ETag { get; set; }
}

internal sealed class SourceDocument
{
    [JsonProperty("sourceSystemId")]
    public Guid SourceSystemId { get; set; }

    [JsonProperty("externalId")]
    public string ExternalId { get; set; } = string.Empty;

    [JsonProperty("url")]
    public string? Url { get; set; }
}

internal sealed class ActorDocument
{
    [JsonProperty("type")]
    public ActorType Type { get; set; }

    [JsonProperty("id")]
    public Guid Id { get; set; }

    [JsonProperty("name")]
    public string Name { get; set; } = string.Empty;
}

internal sealed class LinkDocument
{
    [JsonProperty("nodeType")]
    public string NodeType { get; set; } = string.Empty;

    [JsonProperty("nodeId")]
    public Guid NodeId { get; set; }

    [JsonProperty("edgeType")]
    public string EdgeType { get; set; } = string.Empty;
}
