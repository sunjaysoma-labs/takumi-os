// <copyright file="Program.cs" company="Takumi OS">
// Copyright (c) Takumi OS. Licensed under the MIT License.
// </copyright>

using System;
using System.Collections.Generic;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Identity.Web;
using Microsoft.OpenApi.Models;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;
using Takumi.Memory.Api.Contexts;
using Takumi.Memory.Api.Middleware;
using Takumi.Memory.Application;
using Takumi.Memory.Domain.Contexts;
using Takumi.Memory.Infrastructure.DependencyInjection;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// ----- Logging (Serilog, DIP §2.2) -----
builder.Host.UseSerilog((ctx, services, lc) => lc
    .ReadFrom.Configuration(ctx.Configuration)
    .ReadFrom.Services(services)
    .Enrich.FromLogContext()
    .Enrich.WithMachineName()
    .Enrich.WithThreadId()
    .Enrich.WithProperty("service", "memory")
    .WriteTo.Console());

// ----- Configuration -----
var configuration = builder.Configuration;

// ----- MediatR (DIP §2.2) -----
builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(typeof(ApplicationAssemblyMarker).Assembly);
});

// ----- FluentValidation (DIP §2.2) -----
builder.Services.AddValidatorsFromAssembly(typeof(ApplicationAssemblyMarker).Assembly);

// ----- Auth (Entra ID, DIP §2.2 + TKM-M0-009 in M0 plan) -----
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApi(configuration.GetSection("AzureAd"));

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("memory.read", p => p.RequireAuthenticatedUser()
        .RequireClaim("scp", "memory.read"));
    options.AddPolicy("memory.write", p => p.RequireAuthenticatedUser()
        .RequireClaim("scp", "memory.write"));
});

// ----- Tenant context (request-scoped) -----
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ITenantContext, JwtTenantContext>();

// ----- Cosmos (Infrastructure) -----
builder.Services.AddTakumiCosmos(configuration);

// ----- Observability (OpenTelemetry → App Insights / OTLP) -----
var serviceName = "takumi-memory";
builder.Services
    .AddOpenTelemetry()
    .ConfigureResource(r => r.AddService(serviceName))
    .WithTracing(t =>
    {
        t.AddAspNetCoreInstrumentation()
            .AddHttpClientInstrumentation()
            .AddSource("Takumi.Memory");

        var appInsightsConn = configuration["ApplicationInsights:ConnectionString"];
        if (!string.IsNullOrWhiteSpace(appInsightsConn))
        {
            t.AddAzureMonitorTraceExporter(o => o.ConnectionString = appInsightsConn);
        }

        var otlpEndpoint = configuration["OpenTelemetry:Otlp:Endpoint"];
        if (!string.IsNullOrWhiteSpace(otlpEndpoint))
        {
            t.AddOtlpExporter(o => o.Endpoint = new Uri(otlpEndpoint));
        }
    });

// ----- Health checks (DIP §4 — readiness + liveness) -----
var cosmosConnStr = configuration["Cosmos:ConnectionString"]
    ?? configuration["Cosmos:AccountEndpoint"]
    ?? "https://localhost:8081/";
builder.Services
    .AddHealthChecks()
    .AddAzureCosmosDB(
        connectionString: cosmosConnStr,
        name: "cosmos",
        failureStatus: HealthStatus.Unhealthy,
        tags: new[] { "ready" });

// ----- ProblemDetails (RFC 7807) -----
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

// ----- OpenAPI / Swagger (DIP §9.1) -----
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Takumi Memory API",
        Version = "1.0.0",
        Description =
            "MemoryUnit CRUD, cursor list, and hybrid search. " +
            "Implements [[takumi-os-quantized-memory]] and the MemoryUnit entity in " +
            "[[takumi-os-domain-model]]. See DEVELOPER_IMPLEMENTATION_PACK §6.1, §9.2.",
    });
    c.OperationFilter<Takumi.Memory.Api.Swagger.TenantHeaderOperationFilter>();
    c.OperationFilter<Takumi.Memory.Api.Swagger.LlmModelHeaderOperationFilter>();
    c.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.OAuth2,
        Flows = new OpenApiOAuthFlows
        {
            AuthorizationCode = new OpenApiOAuthFlow
            {
                AuthorizationUrl = new Uri(
                    configuration["AzureAd:AuthorizationUrl"]
                    ?? "https://login.microsoftonline.com/common/oauth2/v2.0/authorize"),
                TokenUrl = new Uri(
                    configuration["AzureAd:TokenUrl"]
                    ?? "https://login.microsoftonline.com/common/oauth2/v2.0/token"),
                Scopes = new Dictionary<string, string>
                {
                    ["api://takumi-memory/memory.read"] = "Read MemoryUnits",
                    ["api://takumi-memory/memory.write"] = "Write MemoryUnits",
                },
            },
        },
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        [new OpenApiSecurityScheme
        {
            Reference = new OpenApiReference
            {
                Type = ReferenceType.SecurityScheme,
                Id = "oauth2",
            },
        }] = new[] { "api://takumi-memory/memory.read", "api://takumi-memory/memory.write" },
    });
});

// ----- MVC -----
builder.Services
    .AddControllers()
    .ConfigureApiBehaviorOptions(options =>
    {
        // Use our global exception handler / ProblemDetails even for
        // automatic 400 responses on model-binding failures.
        options.SuppressMapClientErrors = false;
    })
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy =
            System.Text.Json.JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.DefaultIgnoreCondition =
            System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
    });

WebApplication app = builder.Build();

// ----- Pipeline -----
app.UseMiddleware<CorrelationIdMiddleware>();
app.UseExceptionHandler();
app.UseSerilogRequestLogging();
app.UseAuthentication();
app.UseAuthorization();
app.UseSwagger();
app.UseSwaggerUI();
app.MapControllers();
app.MapHealthChecks("/healthz", new HealthCheckOptions
{
    Predicate = _ => false, // liveness: process is up
});
app.MapHealthChecks("/readyz", new HealthCheckOptions
{
    Predicate = c => c.Tags.Contains("ready"),
    ResponseWriter = async (ctx, report) =>
    {
        ctx.Response.ContentType = "application/json";
        await ctx.Response.WriteAsJsonAsync(new
        {
            status = report.Status.ToString(),
            checks = report.Entries,
        });
    },
});

// ----- Ensure Cosmos DB exists on dev startup (idempotent) -----
await app.Services.EnsureCosmosDatabaseAsync();

await app.RunAsync();

// Exposed for WebApplicationFactory in tests.
public partial class Program;
