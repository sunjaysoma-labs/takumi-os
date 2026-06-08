// <copyright file="TenantHeaderOperationFilter.cs" company="Takumi OS">
// Copyright (c) Takumi OS. Licensed under the MIT License.
// </copyright>

using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Takumi.Memory.Api.Swagger;

/// <summary>
/// Adds the <c>X-Correlation-Id</c> request header to every
/// operation so generated clients (openapi-typescript) know to
/// thread it through.
/// </summary>
public sealed class TenantHeaderOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        operation.Parameters ??= new System.Collections.Generic.List<OpenApiParameter>();
        operation.Parameters.Add(new OpenApiParameter
        {
            Name = "X-Correlation-Id",
            In = ParameterLocation.Header,
            Required = false,
            Description = "Caller-supplied correlation id; echoed in the response header.",
            Schema = new OpenApiSchema { Type = "string", Format = "uuid" },
        });
        operation.Parameters.Add(new OpenApiParameter
        {
            Name = "Idempotency-Key",
            In = ParameterLocation.Header,
            Required = false,
            Description = "Caller-supplied idempotency key for write endpoints (M0: accepted, R1: enforced).",
            Schema = new OpenApiSchema { Type = "string", Format = "uuid" },
        });
    }
}

/// <summary>
/// Documents the <c>x-llm-model</c> response header on every
/// operation. Per DIP §9.1, every LLM-touching endpoint declares
/// the model route in this header. M0 memory endpoints don't
/// touch an LLM, but we still document the header for the
/// contract.
/// </summary>
public sealed class LlmModelHeaderOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        // OpenApiResponses is a wrapper around IDictionary<string, OpenApiResponse>
        // but typed as a concrete class in Swashbuckle 6.8.x. Construct it directly.
        operation.Responses ??= new OpenApiResponses();
        foreach (var (_, response) in operation.Responses)
        {
            response.Headers ??= new System.Collections.Generic.Dictionary<string, OpenApiHeader>();
            if (!response.Headers.ContainsKey("x-llm-model"))
            {
                response.Headers["x-llm-model"] = new OpenApiHeader
                {
                    Description = "Model route for any LLM-backed work on this endpoint.",
                    Schema = new OpenApiSchema { Type = "string" },
                };
            }
        }
    }
}
