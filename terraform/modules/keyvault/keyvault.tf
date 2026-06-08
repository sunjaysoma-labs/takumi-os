# keyvault.tf
resource "azurerm_key_vault" "this" {
  name                = local.kv_name
  location            = var.location
  resource_group_name = var.resource_group_name
  tenant_id           = var.tenant_id
  sku_name            = var.sku_name
  tags                = local.common_tags

  enable_rbac_authorization     = true
  soft_delete_retention_days    = var.soft_delete_retention_days
  purge_protection_enabled      = true
  public_network_access_enabled = var.enable_public_network_access

  # Network ACLs: default deny; allow AKS subnet to pull secrets
  network_acls {
    bypass         = "AzureServices"
    default_action = "Deny"
  }
}

# Private endpoint so the VNet can reach the vault without traversing
# the public internet
resource "azurerm_private_endpoint" "vault" {
  name                = "pe-${local.kv_name}"
  location            = var.location
  resource_group_name = var.resource_group_name
  subnet_id           = var.data_subnet_id
  tags                = local.common_tags

  private_service_connection {
    name                           = "psc-vault"
    private_connection_resource_id = azurerm_key_vault.this.id
    is_manual_connection           = false
    subresource_names              = ["vault"]
  }

  private_dns_zone_group {
    name                 = "vault-dns-group"
    private_dns_zone_ids = [var.private_dns_zone_id]
  }
}
