// <copyright file="Enums.cs" company="Takumi OS">
// Copyright (c) Takumi OS. Licensed under the MIT License.
// </copyright>

namespace Takumi.Memory.Domain.Enums;

/// <summary>
/// Top-level classification of a MemoryUnit per
/// [[takumi-os-quantized-memory]] §3.1 and DIP §6.1.
/// Drives RAG routing, indexing strategy, and downstream
/// agent reasoning paths.
/// </summary>
public enum MemoryType
{
    /// <summary>Long-horizon strategy, vision, OKRs.</summary>
    Strategic = 0,

    /// <summary>Day-to-day operating facts, decisions, actions.</summary>
    Operational = 1,

    /// <summary>People, roles, relationships, expertise.</summary>
    Human = 2,

    /// <summary>Measured results of past decisions and actions.</summary>
    Outcome = 3,

    /// <summary>Identified risks, mitigations, exposures.</summary>
    Risk = 4,

    /// <summary>Financial facts, budgets, variance, forecasts.</summary>
    Financial = 5,

    /// <summary>Customer accounts, segments, sentiment, churn signals.</summary>
    Customer = 6,

    /// <summary>Projects, milestones, status, blockers.</summary>
    Project = 7,

    /// <summary>Compliance, regulatory, audit-relevant events.</summary>
    Compliance = 8,
}

/// <summary>
/// Coarse grouping used for filterable UI facets and high-level
/// dashboards. One MemoryCategory can map to multiple MemoryTypes
/// (e.g. <see cref="MemoryType.Risk"/> and
/// <see cref="MemoryType.Financial"/> both fall under
/// <see cref="MemoryCategory.Operational"/>).
/// </summary>
public enum MemoryCategory
{
    Strategic = 0,
    Operational = 1,
    Human = 2,
    Outcome = 3,
}

/// <summary>
/// Provenance of a memory — who or what produced it.
/// Drives the actor field in DIP §6.1.
/// </summary>
public enum ActorType
{
    Person = 0,
    Agent = 1,
    System = 2,
}
