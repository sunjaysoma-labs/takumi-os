# Takumi OS — Memory Service

> Implements [[takumi-os-quantized-memory]] and the `MemoryUnit`
> entity in [[takumi-os-domain-model]]. See DEVELOPER_IMPLEMENTATION_PACK
> §6.1 (Cosmos schema) and §9.2 (OpenAPI worked example).

## What this service does

The Memory Service is the **write and read surface** for every
`MemoryUnit` in Takumi OS. The Knowledge Acquisition Service
(TKM-M0-008) quantises raw source events and posts the resulting
units here; the Agent Platform (TKM-M0-018) reads them back via
search and context-assembly endpoints.

This is the first .NET 9 service in the monorepo. The patterns
established here (Clean architecture, MediatR, FluentValidation,
Cosmos partition-key tenant isolation, OpenAPI 3.1) are reused by
every downstream service.

## Layout

```
src/services/memory/
├── Takumi.Memory.Domain/         # entities, value objects, repository contract
├── Takumi.Memory.Application/    # CQRS handlers, validators, DTOs
├── Takumi.Memory.Infrastructure/ # Cosmos DB implementation
├── Takumi.Memory.Api/            # ASP.NET Core 9 entrypoint
├── Takumi.Memory.Tests/          # xUnit + TestServer
├── Dockerfile                    # multi-stage .NET 9 → aspnet:9.0
└── .dockerignore
```

## Endpoints (v1)

| Method | Path | Scope | Purpose |
|---|---|---|---|
| `POST` | `/api/v1/memory-units` | `memory.write` | Create a MemoryUnit |
| `GET` | `/api/v1/memory-units` | `memory.read` | Cursor-paginated list |
| `GET` | `/api/v1/memory-units/{memoryId}` | `memory.read` | Read by id |
| `POST` | `/api/v1/memory-units/search` | `memory.read` | Hybrid (lexical) search |
| `GET` | `/healthz` | public | Liveness |
| `GET` | `/readyz` | public | Readiness (Cosmos reachable) |

OpenAPI: see `docs/api/openapi/memory.yaml` (canonical). Swagger
UI in dev at `/swagger`.

## Tenant isolation

Every read and write is **scoped to the caller's tenant id**
(resolved from the `tid` JWT claim). The Cosmos partition key
*is* the tenant id, and the repository sets
`PartitionKey = tenantId` on every single-item read and on every
container query. Cross-tenant access is therefore impossible at
the storage layer, not just the application layer.

The mandatory tenant-isolation test (DIP §4.2) lives in
`Takumi.Memory.Tests/Application/CreateMemoryUnitHandlerTests.cs`
and runs in CI as a release gate.

## Running locally

Prereqs: .NET 9 SDK, Azure Cosmos DB Emulator (or a real Cosmos
account).

```bash
# 1. Start Cosmos emulator (Docker)
docker run -d -p 8081:8081 -p 10251:10251 \
  -e AZURE_COSMOS_EMULATOR_PARTITION_COUNT=10 \
  mcr.microsoft.com/cosmosdb/linux/azure-cosmos-emulator

# 2. Run the service
cd src/services/memory/Takumi.Memory.Api
dotnet run
```

The dev `appsettings.Development.json` points at
`https://localhost:8081/` with the documented emulator key.

## Running tests

```bash
dotnet test src/services/memory/Takumi.Memory.Tests
```

Tests use the in-memory repository fake; no Cosmos needed.

## Container image

```bash
docker build -f src/services/memory/Dockerfile -t takumi/memory:1.0.0 .
```

## Kubernetes deploy

Kustomize base lives in `src/k8s/base/memory/`. Overlays in
`src/k8s/overlays/<env>/` (env/dev, env/test, env/prod) patch
`replicas`, image tag, and the workload identity bindings.

```bash
kubectl apply -k src/k8s/overlays/dev
```

## What's NOT in M0

- **Vector search** (TKM-M0-016 wires Azure AI Search once the
  index is provisioned in TKM-M0-016)
- **Idempotency-Key enforcement** (header accepted, R1)
- **Update / Delete endpoints** (M0 is write-once; corrections
  flow through a new quantisation pass)
- **Graph link materialization** (M0: best-effort links returned
  by extractor; M1: required, validated)

## Related

- [[takumi-os]] — product
- [[takumi-os-quantized-memory]] — the 8-stage pipeline
- [[takumi-os-domain-model]] — entity definitions
- [[takumi-os-azure-architecture]] — Cosmos sizing and partition
  strategy
- [[0002-knowledge-graph-neo4j]] — graph store (out of M0)
- [[FOUNDATIONAL_DESIGN_AUTHORITY_v1.0]] (raw)
- [[TECHNICAL_DESIGN_AUTHORITY_v1.0]] (raw)
- [[DEVELOPER_IMPLEMENTATION_PACK_v1.0]] (raw)
