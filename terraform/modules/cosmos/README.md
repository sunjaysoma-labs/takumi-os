# Module: cosmos

Cosmos DB account (SQL API) for [[takumi-os]]. The 10 domain
containers per DIP §5 (MemoryUnits + 9 others) partition by
`tenantId` for automatic multi-tenant isolation and efficient
per-tenant queries.

Per [[takumi-os-azure-architecture]] §Data Architecture, Cosmos DB
is the source of truth for MemoryUnits. AI Search (in a later
ticket) is a downstream index; the data always lands here first.

## Inputs

| Name | Description | Type | Default |
|---|---|---|---|
| `name_prefix` | e.g. `takumi-dev` | `string` | required |
| `location` | Azure region | `string` | `australiaeast` |
| `resource_group_name` | Resource group | `string` | required |
| `vnet_id` | VNet for private endpoint | `string` | required |
| `data_subnet_id` | Subnet for private endpoint | `string` | required |
| `private_dns_zone_id` | privatelink.documents.azure.com zone ID | `string` | required |
| `log_analytics_workspace_id` | LAW for diagnostic settings | `string` | required |
| `kind` | `GlobalDocumentDB` (SQL) | `string` | `GlobalDocumentDB` |
| `consistency_level` | `BoundedStaleness` / `Session` / `Strong` / `Eventual` / `ConsistentPrefix` | `string` | `Session` |
| `offer_type` | `Standard` (provisioned) or Free tier | `string` | `Standard` |
| `automatic_failover_enabled` | Multi-region failover | `bool` | `true` |
| `local_auth_disabled` | Disable local keys; require Entra ID | `bool` | `false` (dev), `true` (prod) |
| `pitr_retention_days` | Point-in-time restore | `number` | `7` (dev), `30` (prod) |
| `memory_units_throughput` | RU/s on MemoryUnits (manual) | `number` | `1000` |
| `default_throughput` | RU/s on other containers (manual) | `number` | `400` |
| `enable_autoscale` | Use autoscale throughput (prod) | `bool` | `false` (dev), `true` (prod) |
| `enable_public_network_access` | Allow public access (dev only) | `bool` | `true` (dev), `false` (prod) |
| `ip_allowlist` | IPs allowed when public access enabled | `list(string)` | `[]` |
| `tags` | Resource tags | `map(string)` | `{}` |

## Outputs

| Name | Description |
|---|---|
| `account_id` | Cosmos account resource ID |
| `account_name` | Cosmos account name |
| `account_endpoint` | `<name>.documents.azure.com` |
| `database_name` | Database name (`takumi`) |
| `container_ids` | Map of container name to ID |
| `primary_key` | Primary key (sensitive; dev only — prod uses Entra ID) |
| `connection_strings` | List of connection strings (sensitive) |
| `data_plane_role_assignment_id` | Entra ID role assignment for app identity (R2) |

## Container schema

All containers partition by `/tenantId` for tenant isolation.
Throughput is per-container (not shared across the database) so
high-traffic containers can be autoscaled independently.

| Container | Partition | TTL (days) | Indexing policy |
|---|---|---|---|
| `MemoryUnits` | `tenantId` | 0 (forever) | `SourceSystemId`, `MemoryType`, `confidenceScore`, `createdAt` |
| `KPIs` | `tenantId` | 0 | `name`, `ownerId`, `unit` |
| `Initiatives` | `tenantId` | 0 | `ownerId`, `status`, `dueDate` |
| `Processes` | `tenantId` | 0 | `owner`, `status` |
| `Projects` | `tenantId` | 0 | `owner`, `status` |
| `Decisions` | `tenantId` | 0 | `madeBy`, `relatedProjectId` |
| `Risks` | `tenantId` | 0 | `severity`, `status`, `owner` |
| `Recommendations` | `tenantId` | 0 | `agentId`, `confidence` |
| `JobRuns` | `tenantId` | 30 | `status`, `startedAt` |
| `SourceEvents` | `tenantId` | 90 | `sourceSystemId`, `eventType`, `receivedAt` |

The `default_ttl` on `MemoryUnits` is 0 (never expire) per
[[takumi-os-quantized-memory]] — the MemoryUnit is the audit
trail; deletions require explicit operator action.

## Conventions

- **Customer-managed keys (CMK):** enabled in prod only via a
  variable; requires `customer_managed_key` block
- **Public access:** disabled in prod/test; dev allows operator
  IPs via the `ip_allowlist` variable
- **Local auth:** disabled in prod (R2); dev keeps it for tooling
- **Multi-region:** single region in M0; failover regions added
  in R2 per the DR plan
- **Diagnostic settings:** all Cosmos metrics and logs piped to LAW

## Related pages

- [[takumi-os-azure-architecture]]
- [[takumi-os-quantized-memory]]
- [[0002-knowledge-graph-neo4j]]
