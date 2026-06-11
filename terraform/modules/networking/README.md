# Module: networking

VNet, 4 subnets (Application, Data, Integration, Management), NSGs,
private DNS zone for blob/core/sql/keyvault, and private endpoint
subnet delegation per [[takumi-os-azure-architecture]].

Per the design authority, dev/test use a single VNet per
environment; prod uses hub-spoke (hub for shared services, spoke
for AKS). M0 implements the single-VNet pattern; hub-spoke is
deferred to R2.

## Inputs

| Name | Description | Type | Default |
|---|---|---|---|
| `name_prefix` | e.g. `takumi-dev` | `string` | required |
| `location` | Azure region | `string` | `australiaeast` |
| `address_space` | VNet CIDR | `list(string)` | `["10.20.0.0/16"]` (dev), `["10.30.0.0/16"]` (test), `["10.40.0.0/16"]` (prod) |
| `tags` | Resource tags | `map(string)` | `{}` |

## Outputs

| Name | Description |
|---|---|
| `vnet_id` | VNet resource ID |
| `vnet_name` | VNet name |
| `application_subnet_id` | AKS / app services subnet ID |
| `data_subnet_id` | Data services subnet ID (Cosmos, SQL, Storage) |
| `integration_subnet_id` | Integration / Service Bus / private endpoints subnet ID |
| `management_subnet_id` | Bastion / jumpbox / management subnet ID |
| `private_dns_zone_ids` | Private DNS zone IDs for blob, sql, vault, etc. |

## Subnet allocation (10.20.0.0/16 in dev)

| Subnet | CIDR | Delegation | NSG |
|---|---|---|---|
| application | 10.20.0.0/20 | `Microsoft.ContainerService/managedClusters` (AKS) | `nsg-application` |
| data | 10.20.16.0/22 | `Microsoft.DBforPostgreSQL/flexibleServers` (future) | `nsg-data` |
| integration | 10.20.20.0/24 | `Microsoft.ServiceBus/namespaces` | `nsg-integration` |
| management | 10.20.21.0/24 | none | `nsg-management` |

## NSG rules (default)

- application: allow 443 from internet (AKS ingress in dev/test);
  deny all else
- data: deny all from internet; allow 1433, 6380, 443 from
  application subnet only
- integration: allow 443, 5671-5672 from application subnet; deny
  internet
- management: allow 22 (SSH) from a single operator IP (passed via
  variable); deny all else

## Related pages

- [[takumi-os-azure-architecture]]
- [[0003-monorepo-vs-polyrepo]]
