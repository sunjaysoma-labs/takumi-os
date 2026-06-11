// <copyright file="ITenantContext.cs" company="Takumi OS">
// Copyright (c) Takumi OS. Licensed under the MIT License.
// </copyright>

using System;
using Takumi.Memory.Domain.Common;

namespace Takumi.Memory.Domain.Contexts;

/// <summary>
/// Request-scoped source of the current tenant id. Implementations
/// live in the API layer (resolve from JWT, header, or
/// <c>SESSION_CONTEXT</c> for service-to-service hops). The
/// application layer consumes this interface so it remains
/// transport-agnostic and unit-testable.
/// </summary>
public interface ITenantContext
{
    /// <summary>
    /// The current tenant id, or <c>null</c> if the caller has not
    /// authenticated. The application layer MUST treat null as an
    /// authorization failure.
    /// </summary>
    TenantId? CurrentTenantId { get; }

    /// <summary>The principal id (Entra oid) of the caller, when
    /// available. Used for audit columns and for the
    /// <see cref="Enums.ActorType.Person"/> actor on writes.</summary>
    Guid? PrincipalId { get; }

    /// <summary>Display name of the caller (best-effort from
    /// <c>name</c> claim).</summary>
    string? PrincipalName { get; }
}
