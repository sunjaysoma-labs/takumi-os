# Takumi OS

> AI-native Executive Operating System. Fractional executive services
> at enterprise quality and SMB affordability.

This is the monorepo for the Takumi OS platform. Everything lives
here: 9 .NET 9 microservices, 8 Python agent workers, 1 Next.js 15
frontend, Terraform modules, Kustomize manifests, OpenAPI specs, and
the documentation stack.

## Documentation

The authoritative engineering specification stack lives in
`docs/architecture/`:

- [`FDA.md`](docs/architecture/FDA.md) — Foundational Design Authority v1.0
- [`TDA.md`](docs/architecture/TDA.md) — Technical Design Authority v1.0
- [`SAP.md`](docs/architecture/SAP.md) — Solution Architecture Pack v1.0
- [`INTEGRATION.md`](docs/architecture/INTEGRATION.md) — Integration & Knowledge Acquisition
- [`UI.md`](docs/architecture/UI.md) — Front-End Design System & UI Architecture
- [`DIP.md`](docs/architecture/DIP.md) — Developer Implementation Pack v1.0

Architecture decision records (ADRs) live in `docs/adr/`.

The cross-agent wiki (read by every agent in the fleet, including
Claude Code, Codex, and Hermes) lives at `~/wiki` and is the source
of truth for [[takumi-os]]. See [[takumi-os]] for the entry point.

## Repository structure

```
takumi-os/
├── docs/
│   ├── architecture/    # FDA, TDA, SAP, Integration, UI, DIP
│   ├── adr/             # Architecture Decision Records
│   ├── runbooks/        # Incident response, recovery procedures
│   └── api/             # OpenAPI specs, event JSON Schemas
├── terraform/           # All infrastructure-as-code
├── k8s/                 # Kustomize base + overlays
├── src/
│   ├── services/        # .NET 9 microservices (9)
│   ├── workers/         # Python agent workers (8) + connectors
│   ├── frontend/        # Next.js 15 app
│   └── shared/          # Cross-service contracts, events, types
├── tests/
│   ├── e2e/             # Playwright + pytest
│   ├── load/            # k6 scripts
│   ├── security/        # OWASP ZAP, Nuclei
│   └── chaos/           # Litmus (R2+)
├── scripts/             # Local dev scripts
├── tickets/             # M0-Mn tickets (TKM-M0-NNN)
├── .github/
│   ├── workflows/       # CI/CD
│   ├── CODEOWNERS
│   ├── PULL_REQUEST_TEMPLATE.md
│   └── ISSUE_TEMPLATE/
├── README.md
├── LICENSE
└── <dotfiles>
```

## Quick start (M0 — local dev)

```bash
# Prereqs: .NET 9, Node 22, Python 3.12, Docker, kubectl, kustomize,
# terraform, az CLI, gh CLI
make help           # show all make targets
make dev-up         # bring up local stack (docker-compose, M0 scope)
make test           # run all unit + integration tests
make lint           # run all linters
```

## Milestones

| Milestone | Scope | Status |
|---|---|---|
| **M0 — MVP** | 9 services, 2 internal agents, 10 screens, 3 integrations (M365, Jira, HubSpot) | In progress |
| **R2 — Knowledge Graph + Benchmarking** | All 15 KG nodes, all 8 MemoryTypes, AI Search, Microsoft Fabric go-live | Backlog |
| **R3 — Executive Agent Team** | All 8 internal agents, 4 user-facing Assistants, AI Governance | Backlog |
| **R4 — Autonomous Operations** | Self-optimising workflows, predictive ops, ISO 27001 audit | Backlog |
| **R5 — Cross-Tenant Intelligence** | Pattern store, benchmarking, industry insights | Backlog |

Tickets for M0 are in `tickets/M0/`. Sequenced per DIP §18. Start
with TKM-M0-001.

## Reasoning model

This codebase is built around a two-model split:

- **Primary reasoning:** **MiniMax-M3** (executive reasoning, agent
  responses, recommendation explanations, cross-tenant pattern
  synthesis). See [`docs/adr/0001-reasoning-model-minimax.md`](docs/adr/0001-reasoning-model-minimax.md).
- **Subagent tasks:** **MiniMax-M2.7** (memory extraction,
  classification, entity resolution, re-ranking, context packing).
  See [`docs/adr/0006-subagent-model-m2.7.md`](docs/adr/0006-subagent-model-m2.7.md).

Routing is centralised in `src/workers/shared/llm_router.py`.

## Licence

Proprietary. © sunjaysoma-labs. All rights reserved.
