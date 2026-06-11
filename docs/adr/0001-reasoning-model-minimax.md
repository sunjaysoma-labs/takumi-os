# TKM-ADR-001 — Primary reasoning model is MiniMax-M3

> **Note:** This is a *copy* of the wiki ADR `~/wiki/adr/0001-reasoning-model-minimax.md`.
> Kept in-repo for code-review proximity. The wiki is the source of
> truth. The weekly wiki-drift job verifies both copies are in sync.

## Status
Accepted, 2026-06-08.

## Context
[[takumi-os]]'s Executive Reasoning Engine needs a primary LLM
that handles long-context, enterprise-knowledge workloads. The
[FDA Chapter 5](../architecture/FDA.md) nominates "MiniMax M3" as
primary, with OpenAI/Anthropic as secondary. The DIP makes this
concrete: all agent system prompts in `src/workers/agents/*/prompts/`
route to MiniMax-M3 by default.

## Decision
**MiniMax-M3 is the primary reasoning model for [[takumi-os]]'s
Executive Reasoning Engine.** Already configured and operational.
Secondary models: OpenAI GPT, Anthropic Claude, routed per task
complexity by the Hermes orchestration layer.

## Consequences

**Positive:**
- Massive context window — full tenant memory graph fits in a
  single reasoning pass for executive questions
- Strong long-term memory utilisation — fits the MemoryUnit
  pattern naturally
- Single-vendor reasoning reduces operational complexity
- Already configured in the user's environment (no new contracts)

**Negative:**
- Vendor concentration risk — a MiniMax outage is a Takumi OS
  outage for reasoning
- Mitigated by secondary model fallback (Claude / GPT) for
  non-critical paths

**Operational:**
- API key in `Takumi.ApiKeys__MiniMax` env var (per AKS Secret)
- Cost monitoring: per-tenant token usage in `AgentRuns.tokenUsage`
- Rate limiting: 1000 req/min per tenant; 100 req/s per agent
- Evaluation: `minimax-evals` runs against goldens on every PR

## Source

Wiki: `~/wiki/adr/0001-reasoning-model-minimax.md`
