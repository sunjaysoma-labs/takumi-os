// <copyright file="CorrelationIdMiddleware.cs" company="Takumi OS">
// Copyright (c) Takumi OS. Licensed under the MIT License.
// </copyright>

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Takumi.Memory.Api.Middleware;

/// <summary>
/// Reads (or generates) a correlation id per request, echoes it in
/// the <c>X-Correlation-Id</c> response header, and stashes it on
/// <see cref="HttpContext.Items"/> for downstream consumers (the
/// controller, log enrichers, the OTel trace).
/// </summary>
public sealed class CorrelationIdMiddleware
{
    public const string HeaderName = "X-Correlation-Id";
    public const string ItemKey = "CorrelationId";

    private readonly RequestDelegate _next;
    private readonly ILogger<CorrelationIdMiddleware> _logger;

    public CorrelationIdMiddleware(
        RequestDelegate next,
        ILogger<CorrelationIdMiddleware> logger)
    {
        _next = next ?? throw new ArgumentNullException(nameof(next));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var raw = context.Request.Headers[HeaderName].ToString();
        var correlationId = Guid.TryParse(raw, out var parsed) ? parsed : Guid.NewGuid();

        context.Items[ItemKey] = correlationId;
        context.Response.OnStarting(() =>
        {
            context.Response.Headers[HeaderName] = correlationId.ToString();
            return Task.CompletedTask;
        });

        using (_logger.BeginScope(new System.Collections.Generic.Dictionary<string, object>
        {
            ["CorrelationId"] = correlationId,
        }))
        {
            await _next(context).ConfigureAwait(false);
        }
    }
}
