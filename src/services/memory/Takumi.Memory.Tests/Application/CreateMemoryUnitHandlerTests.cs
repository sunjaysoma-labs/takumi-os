// <copyright file="CreateMemoryUnitHandlerTests.cs" company="Takumi OS">
// Copyright (c) Takumi OS. Licensed under the MIT License.
// </copyright>

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using NSubstitute;
using Takumi.Memory.Application.Contracts;
using Takumi.Memory.Application.MemoryUnits;
using Takumi.Memory.Domain.Common;
using Takumi.Memory.Domain.Enums;
using Takumi.Memory.Domain.Repositories;
using Takumi.Memory.Tests.Fakes;
using Xunit;

namespace Takumi.Memory.Tests.Application;

public sealed class CreateMemoryUnitHandlerTests
{
    [Fact]
    public async Task Handle_RequiresTenant()
    {
        var repo = new InMemoryMemoryUnitRepository();
        var tenant = new FakeTenantContext(); // no CurrentTenantId
        var sut = new CreateMemoryUnitCommandHandler(repo, tenant);

        var act = () => sut.Handle(
            new CreateMemoryUnitCommand(NewValidRequest()),
            default);

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact]
    public async Task Handle_PersistsAndEchoes()
    {
        var repo = new InMemoryMemoryUnitRepository();
        var tenant = new FakeTenantContext
        {
            CurrentTenantId = TenantId.New(),
        };
        var sut = new CreateMemoryUnitCommandHandler(repo, tenant);

        var result = await sut.Handle(
            new CreateMemoryUnitCommand(NewValidRequest()),
            default);

        result.Id.Should().StartWith("mu_");
        result.TenantId.Should().Be(tenant.CurrentTenantId!.Value.Value);
        result.MemoryId.Should().NotBe(Guid.Empty);
        result.MemoryType.Should().Be(MemoryType.Operational);
        repo.All.Should().HaveCount(1);
    }

    [Fact]
    public async Task Handle_TenantIsolation_TenantBSeesNothingOfTenantA()
    {
        // DIP §4.2 mandatory tenant isolation test
        var repo = new InMemoryMemoryUnitRepository();
        var tenantA = new FakeTenantContext { CurrentTenantId = TenantId.New() };
        var tenantB = new FakeTenantContext { CurrentTenantId = TenantId.New() };
        var handler = new CreateMemoryUnitCommandHandler(repo, tenantA);

        await handler.Handle(new CreateMemoryUnitCommand(NewValidRequest()), default);

        // Re-bind handler to tenantB and try to read tenant A's data
        var listHandler = new ListMemoryUnitsQueryHandler(repo, tenantB);
        var page = await listHandler.Handle(new ListMemoryUnitsQuery(), default);

        page.Items.Should().BeEmpty();
        page.HasMore.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_DuplicateSource_Throws409Equivalent()
    {
        var repo = new InMemoryMemoryUnitRepository();
        var tenant = new FakeTenantContext { CurrentTenantId = TenantId.New() };
        var handler = new CreateMemoryUnitCommandHandler(repo, tenant);

        var req = NewValidRequest();
        await handler.Handle(new CreateMemoryUnitCommand(req), default);

        var act = () => handler.Handle(new CreateMemoryUnitCommand(req), default);
        await act.Should().ThrowAsync<DuplicateMemoryUnitException>();
    }

    private static CreateMemoryUnitRequest NewValidRequest() => new()
    {
        MemoryType = MemoryType.Operational,
        MemoryCategory = MemoryCategory.Operational,
        Context = "Q2 capacity review",
        Event = "Capacity constraint identified for Q2 onboarding",
        Decision = "Reallocate 2 FTE from Internal IT to Client Services",
        ConfidenceScore = 0.87,
        Source = new MemorySourceResponse
        {
            SourceSystemId = Guid.NewGuid(),
            ExternalId = "JIRA-1234",
            Url = "https://acme.atlassian.net/browse/JIRA-1234",
        },
        Actor = new MemoryActorResponse
        {
            Type = ActorType.Person,
            Id = Guid.NewGuid(),
            Name = "Alice Smith",
        },
        Tags = new[] { "capacity", "Q2-2026" },
    };
}
