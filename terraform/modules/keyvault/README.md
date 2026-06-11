# Module: keyvault

Azure Key Vault with RBAC authorization, soft delete, purge
protection, and private endpoint integration.

Per [[takumi-os-azure-architecture]] §Secrets, all application
secrets live in Key Vault. The AKS cluster (in the `aks` module)
mounts secrets via the Azure Key Vault Provider for Secrets Store
CSI Driver.

## Inputs

| Name | Description | Type | Default |
|---|---|---|---|
| `name_prefix` | e.g. `takumi-dev` | `string` | required |
| `location` | Azure region | `string` | `australiaeast` |
| `resource_group_name` | Resource group | `string` | required |
| `tenant_id` | Entra ID tenant ID | `string` | required |
| `vnet_id` | VNet for private endpoint | `string` | required |
| `data_subnet_id` | Subnet for private endpoint | `string` | required |
| `private_dns_zone_id` | privatelink.vaultcore.azure.net zone ID | `string` | required |
| `sku_name` | `standard` or `premium` | `string` | `standard` (dev), `premium` (prod) |
| `soft_delete_retention_days` | 7-90 | `number` | `90` |
| `tags` | Resource tags | `map(string)` | `{}` |

## Outputs

| Name | Description |
|---|---|
| `key_vault_id` | Key Vault ID |
| `key_vault_uri` | Data plane URI (`https://*.vault.azure.net/`) |
| `key_vault_name` | Key Vault name |

## Conventions

- **RBAC authorization enabled** (not legacy access policies)
- **Soft delete + purge protection enabled** in all envs
- **Private endpoint** mandatory in all envs (even dev)
- **Public network access disabled** in prod; dev allows
  `<operator_ips>` via a parameter
- **Initial secret:** none — secrets are created by application
  Helm charts during deployment (TKM-M0-005+)

## Related pages

- [[takumi-os-azure-architecture]]
