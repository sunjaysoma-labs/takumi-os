# Module: monitoring

Log Analytics workspace + Application Insights + a baseline set of
metric alerts. All Takumi OS services (9 .NET microservices + 8
Python agent workers) send telemetry here.

Per [[takumi-os-azure-architecture]] §Observability, the 4 metric
alerts at minimum are: API latency, error rate, saturation,
memory_retrieval_p95. Additional alerts (kanban_dispatcher_stuck,
neo4j_query_latency, servicebus_deadletter_growth) are added in
R2 when those resources exist.

## Inputs

| Name | Description | Type | Default |
|---|---|---|---|
| `name_prefix` | e.g. `takumi-dev` | `string` | required |
| `location` | Azure region | `string` | `australiaeast` |
| `resource_group_name` | Resource group | `string` | required |
| `log_retention_days` | 30-730 | `number` | `30` (dev), `90` (prod) |
| `tags` | Resource tags | `map(string)` | `{}` |

## Outputs

| Name | Description |
|---|---|
| `log_analytics_workspace_id` | LAW resource ID |
| `log_analytics_workspace_key` | LAW primary key (sensitive) |
| `application_insights_id` | App Insights ID |
| `application_insights_connection_string` | App Insights connection string (sensitive) |
| `application_insights_instrumentation_key` | App Insights instrumentation key (sensitive) |

## Baseline alerts (4)

| Name | Metric | Window | Threshold |
|---|---|---|---|
| `api_latency_p95` | `requests/duration` | 5m | p95 > 1s |
| `error_rate` | `requests/failed` | 5m | rate > 5% |
| `saturation` | `performanceCounter/% Processor Time` | 5m | > 80% |
| `memory_retrieval_p95` | `customMetrics/memory_retrieval_ms` | 5m | p95 > 500ms |

`memory_retrieval_p95` references a custom metric emitted by the
Memory Service (TKM-M0-016). Until that service exists, the alert
is created but disabled.

## Related pages

- [[takumi-os-azure-architecture]]
