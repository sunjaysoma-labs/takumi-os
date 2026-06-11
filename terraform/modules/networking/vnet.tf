# vnet.tf
resource "azurerm_virtual_network" "this" {
  name                = "vnet-${var.name_prefix}"
  location            = var.location
  resource_group_name = var.resource_group_name
  address_space       = var.address_space
  tags                = local.common_tags
}

resource "azurerm_subnet" "application" {
  name                 = "snet-application"
  resource_group_name  = var.resource_group_name
  virtual_network_name = azurerm_virtual_network.this.name
  address_prefixes     = local.subnets.application.cidrs
  service_endpoints    = local.subnets.application.service_endpoints

  delegation {
    name = "aks-delegation"
    service_delegation {
      name = "Microsoft.ContainerService/managedClusters"
      actions = [
        "Microsoft.Network/virtualNetworks/subnets/join/action",
      ]
    }
  }
}

resource "azurerm_subnet" "data" {
  name                 = "snet-data"
  resource_group_name  = var.resource_group_name
  virtual_network_name = azurerm_virtual_network.this.name
  address_prefixes     = local.subnets.data.cidrs
  service_endpoints    = local.subnets.data.service_endpoints
}

resource "azurerm_subnet" "integration" {
  name                 = "snet-integration"
  resource_group_name  = var.resource_group_name
  virtual_network_name = azurerm_virtual_network.this.name
  address_prefixes     = local.subnets.integration.cidrs
  service_endpoints    = local.subnets.integration.service_endpoints

  delegation {
    name = "servicebus-delegation"
    service_delegation {
      name = "Microsoft.ServiceBus/namespaces"
      actions = [
        "Microsoft.Network/virtualNetworks/subnets/join/action",
      ]
    }
  }
}

resource "azurerm_subnet" "management" {
  name                 = "snet-management"
  resource_group_name  = var.resource_group_name
  virtual_network_name = azurerm_virtual_network.this.name
  address_prefixes     = local.subnets.management.cidrs
  service_endpoints    = local.subnets.management.service_endpoints
}
