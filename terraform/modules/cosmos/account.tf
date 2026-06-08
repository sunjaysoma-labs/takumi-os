# account.tf
resource "azurerm_cosmosdb_account" "this" {
  name                          = "cosmos-${var.name_prefix}-${substr(sha256(var.name_prefix), 0, 6)}"
  location                      = var.location
  resource_group_name           = var.resource_group_name
  offer_type                    = var.offer_type
  kind                          = var.kind
  automatic_failover_enabled    = var.automatic_failover_enabled
  local_authentication_disabled = var.local_auth_disabled
  public_network_access_enabled = var.enable_public_network_access
  tags                          = local.common_tags

  consistency_policy {
    consistency_level       = var.consistency_level
    max_interval_in_seconds = 5
    max_staleness_prefix    = 100
  }

  geo_location {
    location          = var.location
    failover_priority = 0
  }

  backup {
    type                = "Continuous"
    retention_in_hours  = var.pitr_retention_days * 24
  }

  dynamic "ip_range_filter" {
    for_each = length(var.ip_allowlist) > 0 ? [1] : []
    content {
      ip_range_filter = join(",", var.ip_allowlist)
    }
  }
}

resource "azurerm_cosmosdb_sql_database" "this" {
  name                = "takumi"
  resource_group_name = var.resource_group_name
  account_name        = azurerm_cosmosdb_account.this.name
}
