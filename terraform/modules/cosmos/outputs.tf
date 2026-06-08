# outputs.tf
output "account_id" {
  value = azurerm_cosmosdb_account.this.id
}

output "account_name" {
  value = azurerm_cosmosdb_account.this.name
}

output "account_endpoint" {
  value = azurerm_cosmosdb_account.this.endpoint
}

output "database_name" {
  value = azurerm_cosmosdb_sql_database.this.name
}

output "container_ids" {
  value = { for k, c in azurerm_cosmosdb_sql_container.this : k => c.id }
}

output "container_names" {
  value = { for k, c in azurerm_cosmosdb_sql_container.this : k => c.name }
}

# Connection info: sensitive. Dev uses primary key; prod uses
# Entra ID (R2: stored in Key Vault by app-bootstrap).
output "primary_key" {
  value     = azurerm_cosmosdb_account.this.primary_key
  sensitive = true
}

output "connection_strings" {
  value     = azurerm_cosmosdb_account.this.connection_strings
  sensitive = true
}
