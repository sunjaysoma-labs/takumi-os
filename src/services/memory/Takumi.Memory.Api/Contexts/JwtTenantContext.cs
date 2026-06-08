// <copyright file="JwtTenantContext.cs" company="Takumi OS">
// Copyright (c) Takumi OS. Licensed under the MIT License.
// </copyright>

using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Takumi.Memory.Domain.Common;
using Takumi.Memory.Domain.Contexts;

namespace Takumi.Memory.Api.Contexts;

/// <summary>
/// <see cref="ITenantContext"/> backed by the current
/// <see cref="HttpContext.User"/>. Resolves tenant from the
/// <c>tid</c> (tenant id) claim issued by Entra ID, principal id
/// from <c>oid</c>, and display name from <c>name</c>. Falls back
/// to <c>tenantId</c> / <c>sub</c> for non-Entra tokens (e.g.
/// service-to-service test tokens).
/// </summary>
public sealed class JwtTenantContext : ITenantContext
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public JwtTenantContext(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor
            ?? throw new ArgumentNullException(nameof(httpContextAccessor));
    }

    public TenantId? CurrentTenantId
    {
        get
        {
            var user = _httpContextAccessor.HttpContext?.User;
            if (user is null || !user.Identity?.IsAuthenticated == true)
            {
                return null;
            }

            var raw = user.FindFirstValue("tid")
                ?? user.FindFirstValue("tenantId")
                ?? user.FindFirstValue("tenant_id");

            if (string.IsNullOrWhiteSpace(raw))
            {
                return null;
            }

            return Guid.TryParse(raw, out var g) ? TenantId.From(g) : null;
        }
    }

    public Guid? PrincipalId
    {
        get
        {
            var user = _httpContextAccessor.HttpContext?.User;
            if (user is null) return null;

            var raw = user.FindFirstValue("oid")
                ?? user.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? user.FindFirstValue(JwtRegisteredClaimNames.Sub);
            return Guid.TryParse(raw, out var g) ? g : null;
        }
    }

    public string? PrincipalName =>
        _httpContextAccessor.HttpContext?.User?.Identity?.Name
        ?? _httpContextAccessor.HttpContext?.User?.FindFirstValue("name");
}
