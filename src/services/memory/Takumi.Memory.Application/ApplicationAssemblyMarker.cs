// <copyright file="ApplicationAssemblyMarker.cs" company="Takumi OS">
// Copyright (c) Takumi OS. Licensed under the MIT License.
// </copyright>

namespace Takumi.Memory.Application;

/// <summary>
/// Type-only marker used by <c>Program.cs</c> and tests to locate
/// the Application assembly (for MediatR and FluentValidation
/// registration). Lives in the Application project so the Api
/// layer doesn't have to reference the concrete handler types.
/// </summary>
public static class ApplicationAssemblyMarker
{
}
