// <copyright file="MemoryUnitCreateTests.cs" company="Takumi OS">
// Copyright (c) Takumi OS. Licensed under the MIT License.
// </copyright>

using System;
using FluentAssertions;
using Takumi.Memory.Domain.Common;
using Takumi.Memory.Domain.Entities;
using Takumi.Memory.Domain.Enums;
using Xunit;

namespace Takumi.Memory.Tests.Domain;

public sealed class MemoryUnitCreateTests
{
    [Fact]
    public void Create_SetsIdAsMuPrefixOfMemoryId()
    {
        var unit = MemoryUnit.Create(
            tenantId: TenantId.New(),
            memoryType: MemoryType.Operational,
            memoryCategory: MemoryCategory.Operational,
            source: NewSource(),
            context: "ctx",
            actor: NewActor(),
            @event: "ev",
            decision: null,
            outcome: null,
            confidenceScore: 0.5);

        unit.Id.Should().StartWith("mu_");
        unit.Id.Should().HaveLength("mu_".Length + 32);
        unit.MemoryId.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public void Create_RejectsEmptyTenantId()
    {
        var act = () => MemoryUnit.Create(
            tenantId: default,
            memoryType: MemoryType.Operational,
            memoryCategory: MemoryCategory.Operational,
            source: NewSource(),
            context: "ctx",
            actor: NewActor(),
            @event: "ev",
            decision: null,
            outcome: null,
            confidenceScore: 0.5);

        act.Should().Throw<ArgumentException>()
            .WithMessage("*TenantId*required*");
    }

    [Fact]
    public void Create_RejectsBlankContext()
    {
        var act = () => MemoryUnit.Create(
            tenantId: TenantId.New(),
            memoryType: MemoryType.Operational,
            memoryCategory: MemoryCategory.Operational,
            source: NewSource(),
            context: "  ",
            actor: NewActor(),
            @event: "ev",
            decision: null,
            outcome: null,
            confidenceScore: 0.5);

        act.Should().Throw<ArgumentException>()
            .WithMessage("*Context*required*");
    }

    [Theory]
    [InlineData(-0.1)]
    [InlineData(1.1)]
    [InlineData(double.NaN)]
    public void Create_RejectsOutOfRangeConfidence(double bad)
    {
        var act = () => MemoryUnit.Create(
            tenantId: TenantId.New(),
            memoryType: MemoryType.Operational,
            memoryCategory: MemoryCategory.Operational,
            source: NewSource(),
            context: "ctx",
            actor: NewActor(),
            @event: "ev",
            decision: null,
            outcome: null,
            confidenceScore: bad);

        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void Create_RejectsNullSource()
    {
        var act = () => MemoryUnit.Create(
            tenantId: TenantId.New(),
            memoryType: MemoryType.Operational,
            memoryCategory: MemoryCategory.Operational,
            source: null!,
            context: "ctx",
            actor: NewActor(),
            @event: "ev",
            decision: null,
            outcome: null,
            confidenceScore: 0.5);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Create_AcceptsZeroTags()
    {
        var unit = MemoryUnit.Create(
            tenantId: TenantId.New(),
            memoryType: MemoryType.Operational,
            memoryCategory: MemoryCategory.Operational,
            source: NewSource(),
            context: "ctx",
            actor: NewActor(),
            @event: "ev",
            decision: null,
            outcome: null,
            confidenceScore: 0.5,
            tags: null);

        unit.Tags.Should().BeEmpty();
    }

    private static MemorySource NewSource() =>
        new(Guid.NewGuid(), "JIRA-1", "https://example/jira/JIRA-1");

    private static MemoryActor NewActor() =>
        new(ActorType.Person, Guid.NewGuid(), "Alice");
}
