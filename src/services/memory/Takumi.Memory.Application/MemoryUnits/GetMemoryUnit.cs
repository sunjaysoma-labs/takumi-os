// <copyright file="GetMemoryUnit.cs" company="Takumi OS">
// Copyright (c) Takumi OS. Licensed under the MIT License.
// </copyright>

using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Takumi.Memory.Application.Contracts;
using Takumi.Memory.Application.Mappings;
using Takumi.Memory.Domain.Contexts;
using Takumi.Memory.Domain.Repositories;

namespace Takumi.Memory.Application.MemoryUnits;

public sealed record GetMemoryUnitQuery(Guid MemoryId) : IRequest<MemoryUnitResponse?>;

public sealed class GetMemoryUnitQueryHandler
    : IRequestHandler<GetMemoryUnitQuery, MemoryUnitResponse?>
{
    private readonly IMemoryUnitRepository _repository;
    private readonly ITenantContext _tenantContext;

    public GetMemoryUnitQueryHandler(
        IMemoryUnitRepository repository,
        ITenantContext tenantContext)
    {
        _repository = repository;
        _tenantContext = tenantContext;
    }

    public async Task<MemoryUnitResponse?> Handle(
        GetMemoryUnitQuery query,
        CancellationToken cancellationToken)
    {
        var tenantId = _tenantContext.CurrentTenantId
            ?? throw new UnauthorizedAccessException(
                "GetMemoryUnit requires an authenticated tenant.");

        if (query.MemoryId == Guid.Empty)
        {
            throw new ArgumentException("memoryId is required.", nameof(query));
        }

        var unit = await _repository
            .GetAsync(tenantId, query.MemoryId, cancellationToken)
            .ConfigureAwait(false);

        return unit is null ? null : MemoryUnitMapper.ToResponse(unit);
    }
}
