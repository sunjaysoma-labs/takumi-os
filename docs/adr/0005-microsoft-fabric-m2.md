# TKM-ADR-005 — Microsoft Fabric go-live is R2

> **Note:** This is a *copy* of the wiki ADR.

## Status
Accepted, 2026-06-08.

## Decision
**Microsoft Fabric is provisioned in M0** via Terraform but
**not actively used until R2**. M0 KPIs read directly from
Azure SQL.

## Operational
- `terraform/modules/fabric/` module exists
- `enable_fabric = false` in `env/dev/main.tf` and `env/prod/main.tf`
- Flipped to `true` in R2

## Source
Wiki: `~/wiki/adr/0005-microsoft-fabric-m2.md`
