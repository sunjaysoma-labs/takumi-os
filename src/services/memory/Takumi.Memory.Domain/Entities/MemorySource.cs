// <copyright file="MemorySource.cs" company="Takumi OS">
// Copyright (c) Takumi OS. Licensed under the MIT License.
// </copyright>

using System;

namespace Takumi.Memory.Domain.Entities;

/// <summary>
/// Provenance metadata for a <see cref="MemoryUnit"/>. Captures which
/// upstream system produced the event and an external id for
/// traceability and deduplication.
/// </summary>
public sealed record MemorySource(
    Guid SourceSystemId,
    string ExternalId,
    string? Url);
