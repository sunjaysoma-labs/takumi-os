# Module: servicebus

Premium-tier Service Bus namespace with the 5 [[takumi-os]]
domain topics and matching subscriptions.

Per [[takumi-os-azure-architecture]] §Event Driven Architecture and
DIP §10, the 5 topics are:
- `memory.created` — emitted by Knowledge Acquisition Service on
  every new MemoryUnit
- `decision.recorded` — emitted by COO Agent on every recorded
  decision
- `process.updated` — emitted by Process Service on SOP changes
- `kpi.changed` — emitted by KPI Service on threshold crossings
- `risk.detected` — emitted by Risk Agent on new risks
- `project.completed` — emitted by Project Service on completion
- `agent.invoked` — emitted by Agent Service on every agent call
- `recommendation.generated` — emitted by COO / Strategic Advisor
  on every recommendation

(That's actually 8 topics; the module's topic list reflects the
full set.)

## Inputs

| Name | Description | Type | Default |
|---|---|---|---|
| `name_prefix` | e.g. `takumi-dev` | `string` | required |
| `location` | Azure region | `string` | `australiaeast` |
| `resource_group_name` | Resource group | `string` | required |
| `sku` | `Basic`, `Standard`, `Premium` | `string` | `Standard` (dev), `Premium` (prod) |
| `capacity` | Messaging units (Premium only) | `number` | `1` |
| `vnet_id` | VNet for private endpoint | `string` | required |
| `integration_subnet_id` | Subnet for private endpoint | `string` | required |
| `private_dns_zone_id` | privatelink.servicebus.windows.net zone ID | `string` | required |
| `tags` | Resource tags | `map(string)` | `{}` |

## Outputs

| Name | Description |
|---|---|
| `namespace_id` | Service Bus namespace ID |
| `namespace_name` | Service Bus namespace name |
| `namespace_fqdn` | `<name>.servicebus.windows.net` |
| `topic_ids` | Map of topic name to ID |
| `topic_names` | Map of logical name to actual topic name |
| `default_primary_connection_string` | Root SAS for dev (sensitive) |
| `default_primary_key` | Root key (sensitive) |

## Conventions

- **Topics are sized `Standard` (1MB, partitioning on)**
- **Subscriptions: each topic gets at least one default subscription
  `audit-sink`** that writes to a dead-letter queue
- **No message TTL by default** (audit trail); per-topic TTL
  configurable in R2
- **Private endpoint** mandatory in all envs
- **Local auth not disabled** in M0 (R2 disables it; requires
  Entra ID for every consumer)

## Related pages

- [[takumi-os-azure-architecture]]
- [[takumi-os-domain-model]]
