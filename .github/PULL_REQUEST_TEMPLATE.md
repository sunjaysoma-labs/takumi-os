<!--
Thanks for your contribution to Takumi OS!

Please fill out the sections below. This template is enforced by
PR template validation in CI.
-->

## Summary

<!-- One-paragraph description of what this PR does and why. -->

## Ticket

<!-- Link to the ticket this PR addresses, e.g. TKM-M0-005 -->

**Ticket:** [TKM-XXX-NNN](tickets/...)

## Type of change

- [ ] Bug fix (non-breaking change that fixes an issue)
- [ ] New feature (non-breaking change that adds functionality)
- [ ] Breaking change (fix or feature that would cause existing functionality to change)
- [ ] Documentation update
- [ ] Infrastructure / DevOps
- [ ] Refactor (no functional change)

## Functional / non-functional requirements

<!--
Link the FRs and NFRs from the FDA / TDA / SAP / DIP that this
PR addresses. If none, write "None".
-->

- **FRs:** [list or "None"]
- **NFRs:** [list or "None"]

## How has this been tested?

<!-- Describe the tests you ran. -->

- [ ] Unit tests pass
- [ ] Integration tests pass
- [ ] E2E tests pass (if user-facing)
- [ ] Tenant isolation test passes
- [ ] Manual smoke test
- [ ] Linters clean (dotnet format / ruff / eslint / prettier / tflint)
- [ ] Security scan clean (Snyk / gitleaks)

## Definition of Done (DIP §3)

- [ ] Code complete
- [ ] Tests pass (unit + integration + at least one E2E if user-facing)
- [ ] Linters clean
- [ ] No new high/critical security findings
- [ ] Observability: metrics, logs, traces emitted
- [ ] OpenAPI / JSON Schemas updated if API changed
- [ ] README / docs updated if behaviour changed
- [ ] ADR added if any new architecture decision
- [ ] Schema migrations applied + tested
- [ ] UI accessibility (axe-core clean, keyboard nav) — if user-facing
- [ ] Tenant isolation enforced
- [ ] Wiki updated — if a [[takumi-os-*]] concept changed
- [ ] Demo to product owner (Loom / live walkthrough) — if user-facing

## Screenshots / recordings (UI changes only)

<!-- Paste screenshots, screen recordings, or Loom links. -->

## Wiki updates

<!--
List any wiki pages this PR requires updating. The CI wiki-drift
job will fail the build if docs and code drift apart.
-->

- [ ] No wiki updates needed
- [ ] Updated `~/wiki/concepts/...`
- [ ] Updated `~/wiki/adr/...`
- [ ] Updated `~/wiki/raw/pack-archive/...`

## Risks and rollback

<!-- What could go wrong? How do we roll back? -->

## Reviewers

<!-- Tag the right team(s) per CODEOWNERS. The bot will auto-assign. -->

/cc @sunjaysoma-labs/team-XXX
