// <copyright file="TenantId.cs" company="Takumi OS">
// Copyright (c) Takumi OS. Licensed under the MIT License.
// </copyright>

using System;

namespace Takumi.Memory.Domain.Common;

/// <summary>
/// Strongly-typed wrapper around a tenant identifier (GUID).
/// Enforces non-empty tenant identity at the domain boundary so
/// accidental cross-tenant reads/writes fail at compile time or at
/// the first validation pass, not deep in the data layer.
/// </summary>
public readonly record struct TenantId
{
    public Guid Value { get; }

    public TenantId(Guid value)
    {
        if (value == Guid.Empty)
        {
            throw new ArgumentException("TenantId cannot be empty.", nameof(value));
        }

        Value = value;
    }

    public static TenantId New() => new(Guid.NewGuid());

    public static TenantId From(Guid value) => new(value);

    public static TenantId From(string value)
    {
        if (!Guid.TryParse(value, out var parsed))
        {
            throw new FormatException($"'{value}' is not a valid TenantId.");
        }

        return new TenantId(parsed);
    }

    public override string ToString() => Value.ToString();
}
