# Module: aks

Single AKS cluster with 3 node pools per DIP §19 Kubernetes
architecture. Per [[takumi-os-azure-architecture]] §Kubernetes,
the cluster runs in the `application` subnet of the networking
module and integrates with the monitoring module via OMS agent.

Per [[0002-knowledge-graph-neo4j]], Neo4j is the knowledge graph
store and runs in a dedicated stateful set on the `data-pool` node
pool.

## Inputs

| Name | Description | Type | Default |
|---|---|---|---|
| `name_prefix` | e.g. `takumi-dev` | `string` | required |
| `location` | Azure region | `string` | `australiaeast` |
| `resource_group_name` | Resource group | `string` | required |
| `application_subnet_id` | Subnet for AKS nodes | `string` | required |
| `kubernetes_version` | AKS version | `string` | latest (when null) |
| `oidc_issuer_enabled` | Enable OIDC issuer for workload identity | `bool` | `true` |
| `workload_identity_enabled` | Enable Entra ID workload identity | `bool` | `true` |
| `key_vault_id` | Key Vault ID for CSI driver | `string` | required |
| `log_analytics_workspace_id` | LAW for OMS agent | `string` | required |
| `tags` | Resource tags | `map(string)` | `{}` |

## Node pools

Per DIP §19:

| Pool | Purpose | Min | Max | VM SKU (dev) | VM SKU (prod) |
|---|---|---|---|---|---|
| `system` | CoreDNS, kured, metrics-server | 1 | 3 | Standard_B2s | Standard_DS2_v2 |
| `core-pool` | Microservices (9 .NET services) | 2 | 10 | Standard_D2s_v5 | Standard_D4s_v5 |
| `agent-pool` | Python agent workers (8) | 2 | 10 | Standard_D2s_v5 | Standard_D4s_v5 |
| `data-pool` | Neo4j stateful set, future stateful | 0 | 4 | Standard_E2s_v5 | Standard_E4s_v5 |

`data-pool` uses 0 as the initial count (no Neo4j in M0; the
stateful set is created in R2 per [[0002-knowledge-graph-neo4j]]).

## Outputs

| Name | Description |
|---|---|
| `cluster_id` | AKS cluster ID |
| `cluster_name` | AKS cluster name |
| `cluster_fqdn` | AKS API server FQDN |
| `oidc_issuer_url` | OIDC issuer URL (for workload identity) |
| `kubelet_identity_object_id` | Kubelet managed identity (for ACR pull, Key Vault) |
| `node_resource_group` | AKS-managed resource group name |

## Conventions

- **Azure CNI Overlay** for IP efficiency
- **OIDC + workload identity** enabled (per [[takumi-os-azure-architecture]])
- **Local accounts disabled** in prod; dev keeps them for kubectl
  debugging (R2 disables)
- **Key Vault CSI driver** enabled by default
- **OMS agent** for Container Insights
- **No public API server** in prod; private cluster mode

## Related pages

- [[takumi-os-azure-architecture]]
- [[0002-knowledge-graph-neo4j]]
