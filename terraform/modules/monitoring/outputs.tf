# outputs.tf
output "log_analytics_workspace_id" {
  value = azurerm_log_analytics_workspace.this.id
}

output "log_analytics_workspace_name" {
  value = azurerm_log_analytics_workspace.this.name
}

output "log_analytics_workspace_key" {
  value     = azurerm_log_analytics_workspace.this.primary_shared_key
  sensitive = true
}

output "application_insights_id" {
  value = azurerm_application_insights.this.id
}

output "application_insights_name" {
  value = azurerm_application_insights.this.name
}

output "application_insights_connection_string" {
  value     = azurerm_application_insights.this.connection_string
  sensitive = true
}

output "application_insights_instrumentation_key" {
  value     = azurerm_application_insights.this.instrumentation_key
  sensitive = true
}

output "action_group_id" {
  value     = try(azurerm_monitor_action_group.this[0].id, null)
  sensitive = false
}
