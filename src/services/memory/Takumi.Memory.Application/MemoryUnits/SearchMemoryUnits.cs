// <copyright file="SearchMemoryUnits.cs" company="Takumi OS">
// Copyright (c) Takumi OS. Licensed under the MIT License.
// </copyright>

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using Takumi.Memory.Application.Contracts;
using Takumi.Memory.Application.Mappings;
using Takumi.Memory.Domain.Contexts;
using Takumi.Memory.Domain.Repositories;

namespace Takumi.Memory.Application.MemoryUnits;

public sealed record SearchMemoryUnitsQuery(SearchMemoryUnitsRequest Request)
    : IRequest<IReadOnlyList<MemoryUnitSearchHitResponse>>;

public sealed class SearchMemoryUnitsQueryValidator
    : AbstractValidator<SearchMemoryUnitsQuery>
{
    public SearchMemoryUnitsQueryValidator()
    {
        RuleFor(x => x.Request.Query)
            .NotEmpty()
            .MaximumLength(2000)
            .WithMessage("query is required and must be <= 2000 chars");
        RuleFor(x => x.Request.TopK)
            .InclusiveBetween(1, 100);
        RuleFor(x => x.Request.MinConfidence)
            .InclusiveBetween(0.0, 1.0);
    }
}

public sealed class SearchMemoryUnitsQueryHandler
    : IRequestHandler<
        SearchMemoryUnitsQuery,
        IReadOnlyList<MemoryUnitSearchHitResponse>>
{
    private readonly IMemoryUnitRepository _repository;
    private readonly ITenantContext _tenantContext;

    public SearchMemoryUnitsQueryHandler(
        IMemoryUnitRepository repository,
        ITenantContext tenantContext)
    {
        _repository = repository;
        _tenantContext = tenantContext;
    }

    public async Task<IReadOnlyList<MemoryUnitSearchHitResponse>> Handle(
        SearchMemoryUnitsQuery query,
        CancellationToken cancellationToken)
    {
        var tenantId = _tenantContext.CurrentTenantId
            ?? throw new UnauthorizedAccessException(
                "SearchMemoryUnits requires an authenticated tenant.");

        var options = new MemoryUnitSearchOptions
        {
            Query = query.Request.Query,
            TopK = query.Request.TopK <= 0 ? 10 : System.Math.Min(query.Request.TopK, 100),
            MemoryTypes = query.Request.MemoryTypes,
            MinimumConfidence = query.Request.MinConfidence,
            Tags = query.Request.Tags,
        };

        var hits = await _repository.SearchAsync(tenantId, options, cancellationToken)
            .ConfigureAwait(false);

        return hits
            .Select(h => MemoryUnitMapper.ToSearchHit((h.Unit, h.Score)))
            .ToArray();
    }
}
