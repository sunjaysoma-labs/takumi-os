# TKM-ADR-002 — Knowledge graph on Neo4j; Cosmos SQL for MemoryUnits

> **Note:** This is a *copy* of the wiki ADR. The wiki is the
> source of truth.

## Status
Accepted, 2026-06-08.

## Context
The TDA §5 offers two options for the knowledge graph: Azure
Cosmos DB (Gremlin API) or Neo4j Enterprise.

## Decision
**Neo4j on AKS** is the knowledge graph store. **Cosmos DB (SQL
API)** remains the store for MemoryUnits.

## Rationale
- Primary access pattern is graph traversal; Neo4j's Cypher is the
  production-grade choice
- MemoryUnits are document-shaped, not graph-shaped; Cosmos SQL API
  with partition by `tenantId` is the right fit

## Operational
- Neo4j cluster: 3 nodes for prod, 1 for dev
- Backup: daily full + hourly incremental to Blob Storage
- Helm chart: `terraform/modules/neo4j/helm.tf` references
  `https://neo4j.com/docs/operations-manual/current/kubernetes/`

## Source
Wiki: `~/wiki/adr/0002-knowledge-graph-neo4j.md`
