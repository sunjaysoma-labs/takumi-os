# Architecture

This directory contains the authoritative engineering specification
stack for Takumi OS, in layered order:

| Layer | File | Purpose |
|---|---|---|
| **FDA** | `FDA.md` | Foundational Design Authority — what + why |
| **TDA** | `TDA.md` | Technical Design Authority — engineering architecture |
| **SAP** | `SAP.md` | Solution Architecture Pack — C4 + DDD + schemas |
| **Integration** | `INTEGRATION.md` | Connector framework + knowledge acquisition |
| **UI** | `UI.md` | Front-end design system + IA |
| **DIP** | `DIP.md` | Developer Implementation Pack — repo, schemas, manifests, M0 ticket order |

The full source markdown for each document lives at
`~/wiki/raw/pack-archive/`. The files in this directory are symlinks
(or copies) of those sources, kept in-repo for code-review
proximity.

The cross-agent wiki at `~/wiki` is the **source of truth** —
[[takumi-os]] is the entry point. The wiki drift job
(`.github/workflows/weekly-wiki-drift.yml`) verifies that the wiki
and the code stay in sync.
