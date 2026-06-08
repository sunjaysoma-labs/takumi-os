// <copyright file="GraphLink.cs" company="Takumi OS">
// Copyright (c) Takumi OS. Licensed under the MIT License.
// </copyright>

using System;

namespace Takumi.Memory.Domain.Entities;

/// <summary>
/// Outbound edge from a <see cref="MemoryUnit"/> into the knowledge
/// graph. Persisted inline with the unit for fast traversal handoff
/// to the Agent Platform; the authoritative edge lives in Neo4j
/// (see [[0002-knowledge-graph-neo4j]]).
/// </summary>
public sealed record GraphLink(
    string NodeType,
    Guid NodeId,
    string EdgeType);
