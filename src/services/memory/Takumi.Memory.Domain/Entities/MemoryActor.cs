// <copyright file="MemoryActor.cs" company="Takumi OS">
// Copyright (c) Takumi OS. Licensed under the MIT License.
// </copyright>

using System;
using Takumi.Memory.Domain.Enums;

namespace Takumi.Memory.Domain.Entities;

/// <summary>
/// Who or what produced or is referenced by a memory.
/// Maps to the <c>actor</c> field in DIP §6.1.
/// </summary>
public sealed record MemoryActor(
    ActorType Type,
    Guid Id,
    string Name);
