// <copyright file="ApiResponse.cs" company="Takumi OS">
// Copyright (c) Takumi OS. Licensed under the MIT License.
// </copyright>

using System;
using System.Text.Json.Serialization;

namespace Takumi.Memory.Application.Contracts;

/// <summary>
/// Standard success envelope for every API response, per DIP §9.1.
/// Keeps a single, predictable shape for clients to deserialize
/// generically (every generated TS client and the FE ApiClient
/// depend on this exact field ordering).
/// </summary>
/// <typeparam name="T">The payload type.</typeparam>
public sealed record ApiResponse<T>(
    [property: JsonPropertyName("success")] bool Success,
    [property: JsonPropertyName("data")] T Data,
    [property: JsonPropertyName("correlationId")] Guid CorrelationId)
{
    public static ApiResponse<T> Ok(T data, Guid correlationId) =>
        new(true, data, correlationId);

    /// <summary>
    /// Generic factory that infers <typeparamref name="T"/> from
    /// the data argument, so call sites can write
    /// <c>ApiResponse.Ok(myDto, corrId)</c> without repeating the
    /// type parameter. Internally delegates to the constructor
    /// route so the wire shape stays identical.
    /// </summary>
    public static ApiResponse<T> Ok<T>(T data, Guid correlationId) =>
        new(true, data, correlationId);
}
