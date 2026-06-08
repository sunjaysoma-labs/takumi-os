# Takumi OS — tickets

Tickets are sequenced per [DIP §18](../../raw/pack-archive/DEVELOPER_IMPLEMENTATION_PACK_v1.0.md#18-m0-implementation-order).

- [`M0/`](./M0/) — MVP (Company Brain + COO Agent + 10 screens + 3 integrations)
- R2/, R3/, R4/, R5/ — to be created as milestones approach

## Convention

Per [[sunjaysoma-labs]]:
- File name: `TKM-<MILESTONE>-<NNN>.md` (zero-padded NNN)
- Frontmatter: id, title, milestone, status, priority, estimate,
  dependencies, assignee, fr, nfr, acceptance_criteria
- Body: Context, Approach, Definition of Done (DIP §3)
- Wiki is the source of truth for ticket content; tickets/M0/ is
  the in-repo copy for code-review proximity
- CI wiki-drift job verifies the two stay in sync
