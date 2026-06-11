// <copyright file="GlobalExceptionHandler.cs" company="Takumi OS">
// Copyright (c) Takumi OS. Licensed under the MIT License.
// </copyright>

using System;
using System.Linq;
using System.Threading.Tasks;
using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Takumi.Memory.Domain.Repositories;

namespace Takumi.Memory.Api.Middleware;

/// <summary>
/// Global exception handler that converts known exception types
/// to RFC 7807 <see cref="ProblemDetails"/> responses per DIP §9.1
/// and lets unknown exceptions fall through to the framework's
/// default developer page (in development) or a 500 (in
/// production).
/// </summary>
public sealed class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var (status, type, title, detail) = exception switch
        {
            ValidationException ve => (
                StatusCodes.Status400BadRequest,
                "validation-failed",
                "Validation failed",
                string.Join("; ", ve.Errors.Select(e => $"{e.PropertyName}: {e.ErrorMessage}"))),

            DuplicateMemoryUnitException dup => (
                StatusCodes.Status409Conflict,
                "memory-unit-duplicate",
                "Duplicate MemoryUnit",
                dup.Message),

            UnauthorizedAccessException => (
                StatusCodes.Status401Unauthorized,
                "tenant-required",
                "Tenant context missing",
                "The request did not include a valid tenant id."),

            ArgumentException ae => (
                StatusCodes.Status400BadRequest,
                "bad-request",
                "Bad request",
                ae.Message),

            _ => default,
        };

        if (status == 0)
        {
            // Unknown exception: let the framework default handler
            // produce a 500. We still log here for correlation.
            _logger.LogError(exception, "Unhandled exception in Memory Service");
            return false;
        }

        _logger.LogWarning(
            exception,
            "Mapped {Type} to {Status} ProblemDetails",
            exception.GetType().Name,
            status);

        var problem = new ProblemDetails
        {
            Status = status,
            Type = $"https://api.takumi.ai/errors/{type}",
            Title = title,
            Detail = detail,
            Instance = httpContext.Request.Path,
        };

        httpContext.Response.StatusCode = status;
        httpContext.Response.ContentType = "application/problem+json";
        await httpContext.Response.WriteAsJsonAsync(problem, cancellationToken)
            .ConfigureAwait(false);
        return true;
    }
}
