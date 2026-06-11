# TKM-ADR-006 — Subagent model is MiniMax-M2.7

> **Note:** This is a *copy* of the wiki ADR.

## Status
Accepted, 2026-06-08.

## Decision
**Subagent tasks in [[takumi-os]] use MiniMax-M2.7.** Routing is
done by the Hermes orchestration layer per task classification.

## What counts as a "subagent task"

Subagent tasks are workloads that meet **all** of:
- **Narrow scope** — well-bounded by an explicit prompt template
- **Low context** — fits comfortably in M2.7's context window (< 32k tokens)
- **High volume** — runs in a loop or stream
- **Deterministic enough** — minor variation between runs is OK
- **No executive reasoning** — doesn't need multi-hop graph traversal

## Task routing (canonical)

| Task | Model |
|---|---|
| Executive reasoning (agent responses) | MiniMax-M3 |
| Memory extraction (Teams/email/Jira/HubSpot → MemoryUnit) | MiniMax-M2.7 |
| MemoryType classification | MiniMax-M2.7 |
| Entity resolution | MiniMax-M2.7 |
| Confidence calibration | MiniMax-M2.7 |
| Cross-encoder re-ranking | MiniMax-M2.7 |
| Context assembly / packing | MiniMax-M2.7 |
| Recommendation explanation (CoT) | MiniMax-M3 |
| Goldens evaluation | MiniMax-M3 |
| Cross-tenant pattern synthesis (R5) | MiniMax-M3 |

## Configuration

The single source of truth for routing is
`src/workers/shared/llm_router.py`. Agents call:

```python
from shared.llm_router import llm_router, ModelRoute

response = llm_router.complete(
    task_type="memory_extraction",  # one of the values above
    messages=[...],
)
```

## Source
Wiki: `~/wiki/adr/0006-subagent-model-m2.7.md`
