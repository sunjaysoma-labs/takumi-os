# outputs.tf
output "vnet_id" {
  description = "Virtual Network ID"
  value       = azurerm_virtual_network.this.id
}

output "vnet_name" {
  description = "Virtual Network name"
  value       = azurerm_virtual_network.this.name
}

output "application_subnet_id" {
  description = "Application subnet ID (AKS, App Services)"
  value       = azurerm_subnet.application.id
}

output "data_subnet_id" {
  description = "Data subnet ID (Cosmos, SQL, Storage)"
  value       = azurerm_subnet.data.id
}

output "integration_subnet_id" {
  description = "Integration subnet ID (Service Bus, Event Hubs)"
  value       = azurerm_subnet.integration.id
}

output "management_subnet_id" {
  description = "Management subnet ID (Bastion, jumpbox)"
  value       = azurerm_subnet.management.id
}

output "private_dns_zone_ids" {
  description = "Map of private DNS zone name to ID"
  value       = { for k, z in azurerm_private_dns_zone.this : k => z.id }
}

output "nsg_ids" {
  description = "Map of NSG purpose to ID"
  value = {
    application = azurerm_network_security_group.application.id
    data        = azurerm_network_security_group.data.id
    integration = azurerm_network_security_group.integration.id
    management  = azurerm_network_security_group.management.id
  }
}
