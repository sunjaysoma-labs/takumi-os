# Takumi OS — Infrastructure as Code

Terraform modules for the [[takumi-os]] Azure landing zone.

Per [[0003-monorepo-vs-polyrepo]], all modules live in this single
repo under `terraform/`. Per [[takumi-os-azure-architecture]]
§Resource group structure, there are three environment configs.

## Module graph

```
modules/
├── networking/    VNet, 4 subnets, NSGs, private endpoints
├── aks/           AKS cluster, 3 node pools, OIDC issuer
├── keyvault/      Key Vault, RBAC, secrets, CSI driver wiring
├── monitoring/    Log Analytics, App Insights, alerts
└── servicebus/    Premium namespace, 5 topics, 5 subscriptions
```

Dependency order (apply in this order):
```
networking  →  keyvault  →  monitoring  →  servicebus  →  aks
```

`aks` depends on `networking` (subnet), `keyvault` (CSI), and
`monitoring` (oms-agent). `servicebus` is independent of AKS
(consumed by worker pods but provisioned first).

## Environments

```
env/
├── dev/    (M0 development, public IP allowlist, no DR)
├── test/   (CI test, IP allowlist, basic DR drill)
└── prod/   (Entra ID only, full DR, geo-redundant)
```

Each `env/` directory has:
- `main.tf` — wires modules together
- `providers.tf` — provider config + backend
- `variables.tf` — env-specific vars
- `terraform.tfvars.example` — committed example values
- `terraform.tfvars` — **gitignored**, real values
- `outputs.tf` — common outputs

## Conventions

- **Naming:** `<resource-type>-<env>-<purpose>` (e.g.
  `vnet-takumi-prod`, `aks-takumi-prod-core`)
- **Tags (all resources):** `environment`, `managed-by=terraform`,
  `project=takumi-os`, `cost-center=takumi-os`, `owner`,
  `data-classification`
- **Region:** `australiaeast` (per [[takumi-os-azure-architecture]]
  §Reference Deployment; can override per env)
- **No public endpoints in prod:** all data services use private
  endpoints; only AKS API server is publicly reachable in dev/test
- **Dev uses Cosmos SQL + Azure SQL free / dev tiers; prod uses
  provisioned throughput**

## How to use

```bash
# Dev — full apply
cd terraform/env/dev
terraform init
terraform plan -out=tfplan
terraform apply tfplan

# Test — full apply
cd terraform/env/test
terraform init
terraform plan -out=tfplan
terraform apply tfplan

# Prod — requires a human review (no auto-apply)
cd terraform/env/prod
terraform init
terraform plan -out=tfplan
# Open PR, get review, then:
terraform apply tfplan
```

## CI

The `ci-iac.yml` workflow runs:
- `terraform fmt -check -recursive`
- `tflint --recursive`
- `tfsec` (in place of checkov for speed; full checkov in
  security-scan.yml)
- `terraform validate` per env
- `kubeconform` against Kustomize builds (separate job)

## Related pages

- [[takumi-os-azure-architecture]]
- [[0002-knowledge-graph-neo4j]]
- [[0003-monorepo-vs-polyrepo]]
- [[0005-microsoft-fabric-m2]]
