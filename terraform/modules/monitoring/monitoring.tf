# monitoring.tf
resource "azurerm_log_analytics_workspace" "this" {
  name                = "log-${var.name_prefix}"
  location            = var.location
  resource_group_name = var.resource_group_name
  sku                 = "PerGB2018"
  retention_in_days   = var.log_retention_days
  tags                = local.common_tags
}

resource "azurerm_application_insights" "this" {
  name                = "appi-${var.name_prefix}"
  location            = var.location
  resource_group_name = var.resource_group_name
  workspace_id        = azurerm_log_analytics_workspace.this.id
  application_type    = "web"
  retention_in_days   = 90
  tags                = local.common_tags

  # Disable local auth in prod; use Entra ID for ingestion
  local_authentication_disabled = false
}

# Diagnostic settings: pipe App Insights and other resources' logs
# to the LAW. The other resources are wired in their own modules.
resource "azurerm_monitor_diagnostic_setting" "appi_to_law" {
  name                       = "appi-to-law"
  target_resource_id         = azurerm_application_insights.this.id
  log_analytics_workspace_id = azurerm_log_analytics_workspace.this.id

  enabled_log {
    category_group = "allLogs"
  }

  enabled_metric {
    category = "AllMetrics"
  }
}

# Action group for alert routing. Email recipients are configured
# per env via tfvars.
resource "azurerm_monitor_action_group" "this" {
  count               = length(var.alert_email_recipients) > 0 ? 1 : 0
  name                = "ag-${var.name_prefix}-default"
  resource_group_name = var.resource_group_name
  short_name          = "takumi-${var.name_prefix}"
  tags                = local.common_tags

  email_receiver {
    name          = "default"
    email_address = var.alert_email_recipients[0]
  }
}
