# TKM M0 Tickets

Sequenced tickets for the [[takumi-os]] MVP. Ordered per
[DIP §18](../raw/pack-archive/DEVELOPER_IMPLEMENTATION_PACK_v1.0.md#18-m0-implementation-order).

## Index

| # | ID | Title | Status |
|---|---|---|---|
| 1 | [TKM-M0-001](./TKM-M0-001.md) | Bootstrap monorepo, CI, ADR template | done |
| 2 | [TKM-M0-002](./TKM-M0-002.md) | Terraform: networking, AKS, Key Vault, App Insights, Service Bus | done |
| 3 | [TKM-M0-003](./TKM-M0-003.md) | Terraform: Cosmos account + MemoryUnits container | done |
| 4 | [TKM-M0-004](./TKM-M0-004.md) | Terraform: Azure SQL (identity + organization DBs) | done |
| 5 | TKM-M0-005 | Identity Service: schema, RBAC, Entra ID integration | todo |
| 6 | TKM-M0-006 | Organization Service: schema, basic CRUD | todo |
| 7 | [TKM-M0-007](./TKM-M0-007.md) | Memory Service: schema-free Cosmos CRUD + OpenAPI | done |
| 8 | TKM-M0-008 | Knowledge Acquisition Service: shell, Service Bus subscription | todo |
| 9 | TKM-M0-009 | Connector: Microsoft 365 (Teams + Outlook via Graph API) | todo |
| 10 | TKM-M0-010 | Quantisation: Teams meeting extractor | todo |
| 11 | TKM-M0-011 | Quantisation: email extractor | todo |
| 12 | TKM-M0-012 | Connector: Jira (REST API) | todo |
| 13 | TKM-M0-013 | Connector: HubSpot (REST API) | todo |
| 14 | TKM-M0-014 | Quantisation: Jira issue extractor | todo |
| 15 | TKM-M0-015 | Quantisation: HubSpot opportunity extractor | todo |
| 16 | TKM-M0-016 | Memory Service: Azure AI Search index + hybrid retrieval | todo |
| 17 | TKM-M0-017 | Memory Service: context assembly endpoint | todo |
| 18 | TKM-M0-018 | Agent Service: orchestration shell, Hermes integration | todo |
| 19 | TKM-M0-019 | Agent: COO Agent system prompt + RAG loop | todo |
| 20 | TKM-M0-020 | Agent: Knowledge Agent (read-only context retrieval) | todo |
| 21 | TKM-M0-021 | Frontend: Next.js app shell, Tailwind, design tokens, shadcn/ui | todo |
| 22 | TKM-M0-022 | Frontend: Login + Entra ID SSO | todo |
| 23-31 | (see DIP §18) | Frontend: 9 screens | todo |
| 32 | TKM-M0-032 | E2E: full happy path | todo |
| 33 | TKM-M0-033 | Observability: dashboards + alerts wired | todo |
| 34 | TKM-M0-034 | DR drill: backup + restore from RPO/RTO | todo |
| 35 | TKM-M0-035 | Wiki drift job + ADR-0001 through ADR-0006 | todo |

## Phases

- **Foundation:** 1-4 (done)
- **Services:** 5-8 (TKM-M0-007 done; 5, 6, 8 todo)
- **Integrations:** 9-15 (todo)
- **Retrieval:** 16-17 (todo)
- **Agents:** 18-20 (todo)
- **Frontend:** 21-31 (todo)
- **Validation:** 32 (todo)
- **Ops:** 33-34 (todo)
- **Docs:** 35 (todo)
