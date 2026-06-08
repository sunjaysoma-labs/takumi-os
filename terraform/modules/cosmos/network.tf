# network.tf
resource "azurerm_private_endpoint" "cosmos" {
  name                = "pe-cosmos-${var.name_prefix}"
  location            = var.location
  resource_group_name = var.resource_group_name
  subnet_id           = var.data_subnet_id
  tags                = local.common_tags

  private_service_connection {
    name                           = "psc-cosmos"
    private_connection_resource_id = azurerm_cosmosdb_account.this.id
    is_manual_connection           = false
    subresource_names              = ["Sql"]
  }

  private_dns_zone_group {
    name                 = "cosmos-dns-group"
    private_dns_zone_ids = [var.private_dns_zone_id]
  }
}

# Diagnostic settings: pipe Cosmos metrics to LAW
resource "azurerm_monitor_diagnostic_setting" "cosmos" {
  name                       = "cosmos-to-law"
  target_resource_id         = azurerm_cosmosdb_account.this.id
  log_analytics_workspace_id = var.log_analytics_workspace_id

  enabled_log {
    category_group = "allLogs"
  }

  enabled_metric {
    category = "AllMetrics"
  }
}
