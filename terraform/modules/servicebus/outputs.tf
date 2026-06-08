# outputs.tf
output "namespace_id" {
  value = azurerm_servicebus_namespace.this.id
}

output "namespace_name" {
  value = azurerm_servicebus_namespace.this.name
}

output "namespace_fqdn" {
  value = "${azurerm_servicebus_namespace.this.name}.servicebus.windows.net"
}

output "topic_ids" {
  value = { for k, t in azurerm_servicebus_topic.this : k => t.id }
}

output "topic_names" {
  value = { for k, t in azurerm_servicebus_topic.this : k => t.name }
}

output "subscription_ids" {
  value = { for k, s in azurerm_servicebus_subscription.audit_sink : k => s.id }
}

# Root SAS — used by dev tooling. Apps in prod use Entra ID +
# per-app SAS scoped to topics. The root key should be rotated
# whenever a developer leaves the team (R2: rotate via automation).
output "default_primary_connection_string" {
  value     = azurerm_servicebus_namespace.this.default_primary_connection_string
  sensitive = true
}

output "default_primary_key" {
  value     = azurerm_servicebus_namespace.this.default_primary_key
  sensitive = true
}
