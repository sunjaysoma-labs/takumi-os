// <copyright file="MemoryUnitsController.cs" company="Takumi OS">
// Copyright (c) Takumi OS. Licensed under the MIT License.
// </copyright>

using System;
using System.Collections.Generic;
using System.Threading;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using Takumi.Memory.Application.Contracts;
using Takumi.Memory.Application.MemoryUnits;
using Takumi.Memory.Domain.Enums;
using Takumi.Memory.Domain.Repositories;

namespace Takumi.Memory.Api.Controllers;

/// <summary>
/// HTTP surface for the Memory Service. All endpoints live under
/// <c>/api/v1/memory-units</c> per DIP §9.1. Every write requires
/// the <c>memory.write</c> scope; every read requires
/// <c>memory.read</c>. Authentication is delegated to Entra ID via
/// <c>Microsoft.Identity.Web</c> in <c>Program.cs</c>.
/// </summary>
[ApiController]
[Route("api/v1/memory-units")]
[Authorize]
[Produces("application/json")]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
public sealed class MemoryUnitsController : ControllerBase
{
    private readonly IMediator _mediator;

    public MemoryUnitsController(IMediator mediator)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
    }

    /// <summary>Write a new MemoryUnit.</summary>
    [HttpPost]
    [Authorize(Policy = "memory.write")]
    [SwaggerOperation(
        OperationId = "createMemoryUnit",
        Summary = "Write a MemoryUnit")]
    [ProducesResponseType(typeof(ApiResponse<MemoryUnitResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ApiResponse<MemoryUnitResponse>>> Create(
        [FromBody] CreateMemoryUnitRequest body,
        [FromHeader(Name = "Idempotency-Key")] string? idempotencyKey,
        CancellationToken cancellationToken)
    {
        // M0: idempotency key is accepted but not yet stored; R1 will
        // add a Cosmos container for idempotency records (TKM-M0-007
        // is the CRUD-only ticket per DIP §18, TKM-M0-016 wires
        // search + AI Search index).
        _ = idempotencyKey;

        var result = await _mediator
            .Send(new CreateMemoryUnitCommand(body), cancellationToken)
            .ConfigureAwait(false);

        return CreatedAtAction(
            nameof(GetById),
            new { memoryId = result.MemoryId },
            ApiResponse.Ok(result, GetCorrelationId()));
    }

    /// <summary>List MemoryUnits (cursor-paginated).</summary>
    [HttpGet]
    [Authorize(Policy = "memory.read")]
    [SwaggerOperation(
        OperationId = "listMemoryUnits",
        Summary = "List MemoryUnits (cursor pagination)")]
    [ProducesResponseType(typeof(ApiResponse<MemoryUnitListResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<MemoryUnitListResponse>>> List(
        [FromQuery] MemoryType? memoryType,
        [FromQuery] MemoryCategory? memoryCategory,
        [FromQuery(Name = "tags")] string[]? tags,
        [FromQuery] int limit = 50,
        [FromQuery] string? cursor = null,
        [FromQuery] bool includeTotal = false,
        [FromQuery] ListSortOrder sort = ListSortOrder.CreatedAtDesc,
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator
            .Send(new ListMemoryUnitsQuery(
                MemoryType: memoryType,
                MemoryCategory: memoryCategory,
                Tags: tags,
                Limit: limit,
                Cursor: cursor,
                IncludeTotal: includeTotal,
                Sort: sort), cancellationToken)
            .ConfigureAwait(false);

        return Ok(ApiResponse.Ok(result, GetCorrelationId()));
    }

    /// <summary>Get a MemoryUnit by id (tenant-scoped).</summary>
    [HttpGet("{memoryId:guid}")]
    [Authorize(Policy = "memory.read")]
    [SwaggerOperation(
        OperationId = "getMemoryUnit",
        Summary = "Get a MemoryUnit by id")]
    [ProducesResponseType(typeof(ApiResponse<MemoryUnitResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<MemoryUnitResponse>>> GetById(
        [FromRoute] Guid memoryId,
        CancellationToken cancellationToken)
    {
        var result = await _mediator
            .Send(new GetMemoryUnitQuery(memoryId), cancellationToken)
            .ConfigureAwait(false);

        if (result is null)
        {
            return NotFound(ProblemDetailsFor(
                StatusCodes.Status404NotFound,
                "memory-unit-not-found",
                $"No MemoryUnit found for id '{memoryId}' in the current tenant."));
        }

        return Ok(ApiResponse.Ok(result, GetCorrelationId()));
    }

    /// <summary>Hybrid (vector + keyword) search across MemoryUnits.</summary>
    [HttpPost("search")]
    [Authorize(Policy = "memory.read")]
    [SwaggerOperation(
        OperationId = "searchMemoryUnits",
        Summary = "Hybrid search across MemoryUnits")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<MemoryUnitSearchHitResponse>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<MemoryUnitSearchHitResponse>>>> Search(
        [FromBody] SearchMemoryUnitsRequest body,
        CancellationToken cancellationToken)
    {
        var result = await _mediator
            .Send(new SearchMemoryUnitsQuery(body), cancellationToken)
            .ConfigureAwait(false);

        // Touch the field to keep the compiler happy; we always set
        // x-llm-model even when no LLM was hit so consumers can rely
        // on the header being present (DIP §9.1).
        Response.Headers["x-llm-model"] = "n/a";
        return Ok(ApiResponse.Ok(result, GetCorrelationId()));
    }

    private Guid GetCorrelationId()
    {
        if (HttpContext.Items.TryGetValue("CorrelationId", out var v) && v is Guid g)
        {
            return g;
        }
        return Guid.NewGuid();
    }

    private ProblemDetails ProblemDetailsFor(int status, string type, string detail) =>
        new()
        {
            Status = status,
            Type = $"https://api.takumi.ai/errors/{type}",
            Title = ReasonPhrases.GetReasonPhrase(status) ?? "Error",
            Detail = detail,
            Instance = HttpContext.Request.Path,
        };
}

/// <summary>
/// Local RFC 7231 reason-phrase lookup (avoid pulling in
/// <c>Microsoft.AspNetCore.WebUtilities</c> for one helper).
/// </summary>
internal static class ReasonPhrases
{
    public static string? GetReasonPhrase(int statusCode) => statusCode switch
    {
        200 => "OK",
        201 => "Created",
        400 => "Bad Request",
        401 => "Unauthorized",
        403 => "Forbidden",
        404 => "Not Found",
        409 => "Conflict",
        500 => "Internal Server Error",
        _ => null,
    };
}
