// <copyright file="FakeTenantContext.cs" company="Takumi OS">
// Copyright (c) Takumi OS. Licensed under the MIT License.
// </copyright>

using System;
using Takumi.Memory.Domain.Common;
using Takumi.Memory.Domain.Contexts;

namespace Takumi.Memory.Tests.Fakes;

/// <summary>
/// Hand-rolled <see cref="ITenantContext"/> for unit tests. Set
/// <see cref="CurrentTenantId"/> to the value you want the handler
/// to act as; leave it null to simulate an unauthenticated
/// request.
/// </summary>
public sealed class FakeTenantContext : ITenantContext
{
    public TenantId? CurrentTenantId { get; set; }

    public Guid? PrincipalId { get; set; }

    public string? PrincipalName { get; set; }
}
