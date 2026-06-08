// <copyright file="MemoryUnitDocumentMapper.cs" company="Takumi OS">
// Copyright (c) Takumi OS. Licensed under the MIT License.
// </copyright>

using System;
using System.Linq;
using Takumi.Memory.Domain.Common;
using Takumi.Memory.Domain.Entities;
using Takumi.Memory.Domain.Enums;

namespace Takumi.Memory.Infrastructure.Persistence;

/// <summary>
/// Maps between the storage-shaped <see cref="MemoryUnitDocument"/>
/// and the domain <see cref="MemoryUnit"/>. Two-way conversion
/// keeps the domain entity pure (no Cosmos attributes, no JSON
/// concerns) and lets us evolve the wire format independently of
/// the public DTO contract.
/// </summary>
internal static class MemoryUnitDocumentMapper
{
    public static MemoryUnitDocument ToDocument(MemoryUnit unit) => new()
    {
        Id = unit.Id,
        TenantId = unit.TenantId.Value.ToString(),
        MemoryId = unit.MemoryId,
        MemoryType = unit.MemoryType,
        MemoryCategory = unit.MemoryCategory,
        Source = new SourceDocument
        {
            SourceSystemId = unit.Source.SourceSystemId,
            ExternalId = unit.Source.ExternalId,
            Url = unit.Source.Url,
        },
        Context = unit.Context,
        Actor = new ActorDocument
        {
            Type = unit.Actor.Type,
            Id = unit.Actor.Id,
            Name = unit.Actor.Name,
        },
        Event = unit.Event,
        Decision = unit.Decision,
        Outcome = unit.Outcome,
        ConfidenceScore = unit.ConfidenceScore,
        Embedding = unit.Embedding.Count == 0 ? null : unit.Embedding.ToArray(),
        GraphLinks = unit.GraphLinks
            .Select(l => new LinkDocument
            {
                NodeType = l.NodeType,
                NodeId = l.NodeId,
                EdgeType = l.EdgeType,
            })
            .ToList(),
        Tags = unit.Tags.ToList(),
        CreatedAt = unit.CreatedAt,
        UpdatedAt = unit.UpdatedAt,
    };

    /// <summary>
    /// Hydrate the domain entity from storage. Returns null when
    /// the document is null. TenantId parsing failures bubble up
    /// as <see cref="FormatException"/>; the repository converts
    /// these to 500s with structured logging.
    /// </summary>
    public static MemoryUnit? ToDomain(MemoryUnitDocument? doc)
    {
        if (doc is null)
        {
            return null;
        }

        var tenantId = TenantId.From(doc.TenantId);
        var memoryId = doc.MemoryId == Guid.Empty
            ? throw new InvalidOperationException(
                $"MemoryUnit document {doc.Id} has empty MemoryId.")
            : doc.MemoryId;

        return MemoryUnit.Rehydrate(
            id: doc.Id,
            tenantId: tenantId,
            memoryId: memoryId,
            memoryType: doc.MemoryType,
            memoryCategory: doc.MemoryCategory,
            source: new MemorySource(
                doc.Source.SourceSystemId,
                doc.Source.ExternalId,
                doc.Source.Url),
            context: doc.Context,
            actor: new MemoryActor(
                doc.Actor.Type,
                doc.Actor.Id,
                doc.Actor.Name),
            @event: doc.Event,
            decision: doc.Decision,
            outcome: doc.Outcome,
            confidenceScore: doc.ConfidenceScore,
            embedding: doc.Embedding ?? Array.Empty<float>(),
            graphLinks: doc.GraphLinks
                .Select(l => new GraphLink(l.NodeType, l.NodeId, l.EdgeType))
                .ToArray(),
            tags: doc.Tags.ToArray(),
            createdAt: doc.CreatedAt,
            updatedAt: doc.UpdatedAt);
    }
}
