// <copyright file="MemoryUnitsApiTests.cs" company="Takumi OS">
// Copyright (c) Takumi OS. Licensed under the MIT License.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using FluentAssertions;
using Takumi.Memory.Application.Contracts;
using Takumi.Memory.Domain.Enums;
using Xunit;

namespace Takumi.Memory.Tests.Api;

public sealed class MemoryUnitsApiTests : IClassFixture<MemoryApiFactory>
{
    private readonly MemoryApiFactory _factory;

    public MemoryUnitsApiTests(MemoryApiFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Create_Returns201WithEnvelope()
    {
        var client = _factory.CreateClient();
        SetTenant(client, _factory.TestTenant.Value);

        var response = await client.PostAsJsonAsync(
            "/api/v1/memory-units",
            new CreateMemoryUnitRequest
            {
                MemoryType = MemoryType.Operational,
                MemoryCategory = MemoryCategory.Operational,
                Context = "ctx",
                Event = "ev",
                ConfidenceScore = 0.8,
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
                Tags = new[] { "test" },
            });

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        response.Headers.Location.Should().NotBeNull();
        response.Headers.GetValues("X-Correlation-Id").Should().HaveCount(1);

        var body = await response.Content.ReadFromJsonAsync<ApiResponse<MemoryUnitResponse>>();
        body.Should().NotBeNull();
        body!.Success.Should().BeTrue();
        body.Data.TenantId.Should().Be(_factory.TestTenant.Value);
    }

    [Fact]
    public async Task Create_WithoutTenantHeader_Returns401()
    {
        var client = _factory.CreateClient();
        // No X-Test-Tenant-Id header — handler returns NoResult → 401
        var response = await client.PostAsJsonAsync(
            "/api/v1/memory-units",
            new CreateMemoryUnitRequest
            {
                MemoryType = MemoryType.Operational,
                MemoryCategory = MemoryCategory.Operational,
                Context = "ctx",
                Event = "ev",
                ConfidenceScore = 0.5,
                Source = new MemorySourceResponse
                {
                    SourceSystemId = Guid.NewGuid(),
                    ExternalId = "X-2",
                },
                Actor = new MemoryActorResponse
                {
                    Type = ActorType.Person,
                    Id = Guid.NewGuid(),
                    Name = "Bob",
                },
            });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Get_NonExistent_Returns404ProblemDetails()
    {
        var client = _factory.CreateClient();
        SetTenant(client, _factory.TestTenant.Value);

        var response = await client.GetAsync(
            $"/api/v1/memory-units/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        response.Content.Headers.ContentType?.MediaType
            .Should().Be("application/problem+json");
    }

    [Fact]
    public async Task Create_DuplicateSource_Returns409()
    {
        var client = _factory.CreateClient();
        SetTenant(client, _factory.TestTenant.Value);

        var sourceId = Guid.NewGuid();
        var req = new CreateMemoryUnitRequest
        {
            MemoryType = MemoryType.Operational,
            MemoryCategory = MemoryCategory.Operational,
            Context = "ctx",
            Event = "ev",
            ConfidenceScore = 0.5,
            Source = new MemorySourceResponse
            {
                SourceSystemId = sourceId,
                ExternalId = "DUP-1",
            },
            Actor = new MemoryActorResponse
            {
                Type = ActorType.Person,
                Id = Guid.NewGuid(),
                Name = "Carol",
            },
        };

        var first = await client.PostAsJsonAsync("/api/v1/memory-units", req);
        first.StatusCode.Should().Be(HttpStatusCode.Created);

        var second = await client.PostAsJsonAsync("/api/v1/memory-units", req);
        second.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task TenantIsolation_TenantBSeesNothingOfTenantA()
    {
        var client = _factory.CreateClient();

        // Tenant A writes
        var tenantA = Guid.NewGuid();
        SetTenant(client, tenantA);
        var aResponse = await client.PostAsJsonAsync(
            "/api/v1/memory-units",
            new CreateMemoryUnitRequest
            {
                MemoryType = MemoryType.Risk,
                MemoryCategory = MemoryCategory.Operational,
                Context = "tenant A only",
                Event = "tenant A event",
                ConfidenceScore = 0.7,
                Source = new MemorySourceResponse
                {
                    SourceSystemId = Guid.NewGuid(),
                    ExternalId = "A-1",
                },
                Actor = new MemoryActorResponse
                {
                    Type = ActorType.Person,
                    Id = Guid.NewGuid(),
                    Name = "Alice",
                },
            });
        aResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        // Tenant B reads
        var tenantB = Guid.NewGuid();
        SetTenant(client, tenantB);
        var listResponse = await client.GetAsync("/api/v1/memory-units");
        listResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await listResponse.Content
            .ReadFromJsonAsync<ApiResponse<MemoryUnitListResponse>>();
        body!.Data.Items.Should().BeEmpty();
    }

    private static void SetTenant(HttpClient client, Guid tenant)
    {
        client.DefaultRequestHeaders.Remove("X-Test-Tenant-Id");
        client.DefaultRequestHeaders.Add("X-Test-Tenant-Id", tenant.ToString());
    }
}
