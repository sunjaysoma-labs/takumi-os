// <copyright file="ListMemoryUnits.cs" company="Takumi OS">
// Copyright (c) Takumi OS. Licensed under the MIT License.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using Takumi.Memory.Application.Contracts;
using Takumi.Memory.Application.Mappings;
using Takumi.Memory.Domain.Contexts;
using Takumi.Memory.Domain.Enums;
using Takumi.Memory.Domain.Repositories;

namespace Takumi.Memory.Application.MemoryUnits;

public sealed record ListMemoryUnitsQuery(
    MemoryType? MemoryType = null,
    MemoryCategory? MemoryCategory = null,
    IReadOnlyList<string>? Tags = null,
    int Limit = 50,
    string? Cursor = null,
    bool IncludeTotal = false,
    ListSortOrder Sort = ListSortOrder.CreatedAtDesc)
    : IRequest<MemoryUnitListResponse>;

public sealed class ListMemoryUnitsQueryValidator
    : AbstractValidator<ListMemoryUnitsQuery>
{
    public ListMemoryUnitsQueryValidator()
    {
        RuleFor(x => x.Limit).InclusiveBetween(1, 100);
        RuleFor(x => x.Cursor).MaximumLength(4096);
        RuleForEach(x => x.Tags!.EmptyIfNull()).NotEmpty().MaximumLength(64);
    }
}

public sealed class ListMemoryUnitsQueryHandler
    : IRequestHandler<ListMemoryUnitsQuery, MemoryUnitListResponse>
{
    private readonly IMemoryUnitRepository _repository;
    private readonly ITenantContext _tenantContext;

    public ListMemoryUnitsQueryHandler(
        IMemoryUnitRepository repository,
        ITenantContext tenantContext)
    {
        _repository = repository;
        _tenantContext = tenantContext;
    }

    public async Task<MemoryUnitListResponse> Handle(
        ListMemoryUnitsQuery query,
        CancellationToken cancellationToken)
    {
        var tenantId = _tenantContext.CurrentTenantId
            ?? throw new UnauthorizedAccessException(
                "ListMemoryUnits requires an authenticated tenant.");

        var options = new MemoryUnitListOptions
        {
            MemoryType = query.MemoryType,
            MemoryCategory = query.MemoryCategory,
            Tags = query.Tags,
            Limit = query.Limit <= 0 ? 50 : Math.Min(query.Limit, 100),
            Cursor = query.Cursor,
            IncludeTotal = query.IncludeTotal,
            Sort = query.Sort,
        };

        var page = await _repository.ListAsync(tenantId, options, cancellationToken)
            .ConfigureAwait(false);

        return new MemoryUnitListResponse
        {
            Items = page.Items.Select(MemoryUnitMapper.ToResponse).ToArray(),
            NextCursor = page.NextCursor,
            HasMore = page.HasMore,
            TotalCount = page.TotalCount,
        };
    }
}

internal static class EnumerableNullExtensions
{
    public static IEnumerable<T> EmptyIfNull<T>(this IEnumerable<T>? source) =>
        source ?? Array.Empty<T>();
}
