// <copyright file="ValidatorTests.cs" company="Takumi OS">
// Copyright (c) Takumi OS. Licensed under the MIT License.
// </copyright>

using System;
using FluentAssertions;
using FluentValidation.TestHelper;
using Takumi.Memory.Application.Contracts;
using Takumi.Memory.Application.MemoryUnits;
using Takumi.Memory.Domain.Enums;
using Xunit;

namespace Takumi.Memory.Tests.Application;

public sealed class ValidatorTests
{
    private readonly CreateMemoryUnitCommandValidator _create = new();
    private readonly ListMemoryUnitsQueryValidator _list = new();
    private readonly SearchMemoryUnitsQueryValidator _search = new();

    [Fact]
    public void Create_AcceptsValidRequest()
    {
        var req = NewValidCreate();
        _create.TestValidate(new CreateMemoryUnitCommand(req)).IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData(-0.1)]
    [InlineData(1.1)]
    public void Create_RejectsOutOfRangeConfidence(double bad)
    {
        var req = NewValidCreate() with { ConfidenceScore = bad };
        _create.TestValidate(new CreateMemoryUnitCommand(req))
            .ShouldHaveValidationErrorFor(x => x.Request.ConfidenceScore);
    }

    [Fact]
    public void Create_RejectsBlankContext()
    {
        var req = NewValidCreate() with { Context = "" };
        _create.TestValidate(new CreateMemoryUnitCommand(req))
            .ShouldHaveValidationErrorFor(x => x.Request.Context);
    }

    [Fact]
    public void Create_RejectsEmptySourceSystemId()
    {
        var req = NewValidCreate() with
        {
            Source = new MemorySourceResponse
            {
                SourceSystemId = Guid.Empty,
                ExternalId = "X-1",
            },
        };
        _create.TestValidate(new CreateMemoryUnitCommand(req))
            .ShouldHaveValidationErrorFor(x => x.Request.Source.SourceSystemId);
    }

    [Fact]
    public void List_RejectsLimitOver100()
    {
        _list.TestValidate(new ListMemoryUnitsQuery(Limit: 500))
            .ShouldHaveValidationErrorFor(x => x.Limit);
    }

    [Fact]
    public void Search_RejectsBlankQuery()
    {
        _search.TestValidate(new SearchMemoryUnitsQuery(new SearchMemoryUnitsRequest
        {
            Query = "",
        })).ShouldHaveValidationErrorFor(x => x.Request.Query);
    }

    private static CreateMemoryUnitRequest NewValidCreate() => new()
    {
        MemoryType = MemoryType.Operational,
        MemoryCategory = MemoryCategory.Operational,
        Context = "ctx",
        Event = "ev",
        ConfidenceScore = 0.5,
        Source = new MemorySourceResponse
        {
            SourceSystemId = Guid.NewGuid(),
            ExternalId = "X-1",
        },
        Actor = new MemoryActorResponse
        {
            Type = ActorType.Person,
            Id = Guid.NewGuid(),
            Name = "Alice",
        },
        Tags = new[] { "ok" },
    };
}
