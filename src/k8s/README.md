# Kubernetes manifests

Kustomize-managed AKS deploys for every Takumi OS service.
Per-service bases live in `base/<service>/`; per-environment
overlays in `overlays/<env>/`.

## M0 services

| Service | Base | Notes |
|---|---|---|
| Memory | `base/memory/` | First .NET service; this is the template for the rest |

## Overlays (M0)

- `overlays/dev/` — single replica, public image pull secret,
  workload identity pointed at dev tenant
- `overlays/test/` — same shape, separate cosmos account
- `overlays/prod/` — multi-region, private registry, dedicated
  Cosmos (placeholder until R3)

## Conventions

- Every Deployment has `runAsNonRoot: true`, `readOnlyRootFilesystem: true`,
  drops ALL capabilities (DIP §15)
- Service accounts use `azure.workload.identity/*` annotations
  for Cosmos RBAC (no connection strings in prod)
- HPA at 70% CPU / 80% memory; min 2, max 10
- Pod topology spread across zones + nodes
- Prometheus scrape via PodMonitor (DIP §17)
