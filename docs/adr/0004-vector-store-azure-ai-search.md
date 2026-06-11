# TKM-ADR-004 — Vector store is Azure AI Search

> **Note:** This is a *copy* of the wiki ADR.

## Status
Accepted, 2026-06-08.

## Decision
**Azure AI Search** is the vector store. MemoryUnits in Cosmos
are the source of truth; the AI Search index is a derived index
rebuilt incrementally.

## Operational
- Index name: `memory-units`
- Vector dimensions: 1536
- HNSW algorithm
- Per-tenant filter mandatory in every query (CI lint enforces)

## Source
Wiki: `~/wiki/adr/0004-vector-store-azure-ai-search.md`
