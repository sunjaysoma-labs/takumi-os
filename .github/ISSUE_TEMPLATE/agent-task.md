---
name: Agent task
about: Task to be picked up by an AI coding agent (Claude Code, Codex, or future Hermes CLI)
title: "[Agent] "
labels: ["agent-task", "ready-for-pickup"]
assignees: []
---

## Task

<!--
One sentence: what the agent should produce. Be specific.
Example: "Implement the MemoryService /memory-units POST endpoint
with Cosmos DB persistence and OpenAPI validation per DIP §9.2."
-->

## Ticket

**TKM-XXX-NNN** — [link to ticket file](tickets/...)

## Orientation

The coding agent MUST read, in order:

1. `~/wiki/AGENTS.md` (cross-agent contract)
2. `~/wiki/SCHEMA.md` (wiki conventions)
3. `docs/architecture/DIP.md` §19 (handoff contract for AI coding agents)
4. `docs/architecture/DIP.md` §1 (repo structure)
5. The relevant service's OpenAPI spec in `docs/api/openapi/`
6. This issue

## Decision authority

The agent MAY decide:
- Function/variable naming
- Test case selection
- Library choice *within the constraints in DIP §2*
- Implementation details not specified in the spec stack

The agent MUST escalate (PR comment `@sunjaysoma-labs/maintainers`)
on:
- Any schema deviation from DIP §5/§6/§7
- New dependencies not in DIP §2
- Cross-service API changes
- Any architecture change requiring an ADR

## Acceptance criteria

- [ ]
- [ ]
- [ ]

## Definition of Done

Per DIP §3.

## Output expectations

- Branch: `feat/<ticket-id>-<short-slug>`
- Commits: conventional format (`feat(scope):`, `fix(scope):`, etc.)
- PR description: links ticket, summarises change, includes test evidence
- Wiki update (if a [[takumi-os-*]] page changed)
- ADR (if any new architecture decision)

## Escalation

Anything in "MUST escalate" → comment on the PR and stop work on
that change until resolved.
