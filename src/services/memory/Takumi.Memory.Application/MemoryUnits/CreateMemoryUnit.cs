// <copyright file="CreateMemoryUnit.cs" company="Takumi OS">
// Copyright (c) Takumi OS. Licensed under the MIT License.
// </copyright>

using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using Takumi.Memory.Application.Contracts;
using Takumi.Memory.Application.Mappings;
using Takumi.Memory.Domain.Contexts;
using Takumi.Memory.Domain.Entities;
using Takumi.Memory.Domain.Repositories;

namespace Takumi.Memory.Application.MemoryUnits;

/// <summary>
/// Command: write a new <see cref="MemoryUnit"/> for the current
/// tenant. The handler resolves the tenant from
/// <see cref="ITenantContext"/>, builds the domain entity, and
/// persists it. Returns the materialized unit (with id and
/// timestamps) so the caller can echo it back.
/// </summary>
public sealed record CreateMemoryUnitCommand(CreateMemoryUnitRequest Request)
    : IRequest<MemoryUnitResponse>;

public sealed class CreateMemoryUnitCommandValidator
    : AbstractValidator<CreateMemoryUnitCommand>
{
    public CreateMemoryUnitCommandValidator()
    {
        RuleFor(x => x.Request.MemoryType).IsInEnum();
        RuleFor(x => x.Request.MemoryCategory).IsInEnum();
        RuleFor(x => x.Request.Context).NotEmpty().MaximumLength(2000);
        RuleFor(x => x.Request.Event).NotEmpty().MaximumLength(2000);
        RuleFor(x => x.Request.Decision).MaximumLength(2000);
        RuleFor(x => x.Request.Outcome).MaximumLength(2000);
        RuleFor(x => x.Request.ConfidenceScore)
            .InclusiveBetween(0.0, 1.0)
            .WithMessage("confidenceScore must be in [0, 1]");
        RuleFor(x => x.Request.Source.SourceSystemId)
            .NotEqual(Guid.Empty).WithMessage("source.sourceSystemId is required");
        RuleFor(x => x.Request.Source.ExternalId)
            .NotEmpty().MaximumLength(512);
        RuleFor(x => x.Request.Source.Url)
            .MaximumLength(2048);
        RuleFor(x => x.Request.Actor.Type).IsInEnum();
        RuleFor(x => x.Request.Actor.Id)
            .NotEqual(Guid.Empty).WithMessage("actor.id is required");
        RuleFor(x => x.Request.Actor.Name)
            .NotEmpty().MaximumLength(200);
        RuleForEach(x => x.Request.Tags)
            .NotEmpty().MaximumLength(64);
    }
}

public sealed class CreateMemoryUnitCommandHandler
    : IRequestHandler<CreateMemoryUnitCommand, MemoryUnitResponse>
{
    private readonly IMemoryUnitRepository _repository;
    private readonly ITenantContext _tenantContext;

    public CreateMemoryUnitCommandHandler(
        IMemoryUnitRepository repository,
        ITenantContext tenantContext)
    {
        _repository = repository;
        _tenantContext = tenantContext;
    }

    public async Task<MemoryUnitResponse> Handle(
        CreateMemoryUnitCommand command,
        CancellationToken cancellationToken)
    {
        var tenantId = _tenantContext.CurrentTenantId
            ?? throw new UnauthorizedAccessException(
                "CreateMemoryUnit requires an authenticated tenant.");

        var request = command.Request;

        var unit = MemoryUnit.Create(
            tenantId: tenantId,
            memoryType: request.MemoryType,
            memoryCategory: request.MemoryCategory,
            source: new MemorySource(
                request.Source.SourceSystemId,
                request.Source.ExternalId,
                request.Source.Url),
            context: request.Context,
            actor: new MemoryActor(
                request.Actor.Type,
                request.Actor.Id,
                request.Actor.Name),
            @event: request.Event,
            decision: request.Decision,
            outcome: request.Outcome,
            confidenceScore: request.ConfidenceScore,
            tags: request.Tags);

        var persisted = await _repository.CreateAsync(unit, cancellationToken)
            .ConfigureAwait(false);

        return MemoryUnitMapper.ToResponse(persisted);
    }
}
