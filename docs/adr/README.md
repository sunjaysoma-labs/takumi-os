# Architecture Decision Records (ADRs)

This directory contains the architecture decision records for Takumi
OS. Each ADR documents a single decision: the context, the decision
itself, and the consequences.

ADRs are immutable once accepted. If a decision changes, write a
new ADR that supersedes the old one (and update the Status of the
old).

## Format

We use the [MADR](https://adr.github.io/madr/) format. Each ADR is
a markdown file named `NNNN-short-slug.md` (e.g.
`0001-reasoning-model-minimax.md`).

A new ADR should be added to this directory as part of the same PR
that introduces the decision. The PR template enforces a wiki /
code co-update.

## Index

| # | Title | Status |
|---|---|---|
| [0001](0001-reasoning-model-minimax.md) | Primary reasoning model is MiniMax-M3 | Accepted |
| [0002](0002-knowledge-graph-neo4j.md) | Knowledge graph on Neo4j; Cosmos SQL for MemoryUnits | Accepted |
| [0003](0003-monorepo-vs-polyrepo.md) | Monorepo for M0 | Accepted |
| [0004](0004-vector-store-azure-ai-search.md) | Vector store is Azure AI Search (downstream of MemoryUnit source of truth) | Accepted |
| [0005](0005-microsoft-fabric-m2.md) | Microsoft Fabric go-live is R2 | Accepted |
| [0006](0006-subagent-model-m2.7.md) | Subagent tasks use MiniMax-M2.7 | Accepted |

New ADRs are added in numeric order. The next available number is
**0007**.
