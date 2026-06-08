// <copyright file="MemoryUnitMapper.cs" company="Takumi OS">
// Copyright (c) Takumi OS. Licensed under the MIT License.
// </copyright>

using System.Collections.Generic;
using System.Linq;
using Takumi.Memory.Application.Contracts;
using Takumi.Memory.Domain.Entities;

namespace Takumi.Memory.Application.Mappings;

/// <summary>
/// Hand-rolled DTO ↔ entity mapping. Static and explicit so the
/// transformations are obvious in PR review and so we don't pull
/// in AutoMapper for a single service (per DIP §2.2 library
/// constraints).
/// </summary>
public static class MemoryUnitMapper
{
    public static MemoryUnitResponse ToResponse(MemoryUnit unit) => new()
    {
        Id = unit.Id,
        TenantId = unit.TenantId.Value,
        MemoryId = unit.MemoryId,
        MemoryType = unit.MemoryType,
        MemoryCategory = unit.MemoryCategory,
        Source = new MemorySourceResponse
        {
            SourceSystemId = unit.Source.SourceSystemId,
            ExternalId = unit.Source.ExternalId,
            Url = unit.Source.Url,
        },
        Context = unit.Context,
        Actor = new MemoryActorResponse
        {
            Type = unit.Actor.Type,
            Id = unit.Actor.Id,
            Name = unit.Actor.Name,
        },
        Event = unit.Event,
        Decision = unit.Decision,
        Outcome = unit.Outcome,
        ConfidenceScore = unit.ConfidenceScore,
        Tags = unit.Tags.ToArray(),
        GraphLinks = unit.GraphLinks
            .Select(l => new GraphLinkResponse
            {
                NodeType = l.NodeType,
                NodeId = l.NodeId,
                EdgeType = l.EdgeType,
            })
            .ToArray(),
        CreatedAt = unit.CreatedAt,
        UpdatedAt = unit.UpdatedAt,
    };

    public static MemoryUnitSearchHitResponse ToSearchHit(
        (MemoryUnit Unit, double Score) hit) => new()
    {
        Unit = ToResponse(hit.Unit),
        Score = hit.Score,
    };
}
