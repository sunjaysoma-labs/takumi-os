# nsg.tf
# Network Security Groups for each subnet. Default-deny inbound;
# allow only the documented traffic patterns.

resource "azurerm_network_security_group" "application" {
  name                = "nsg-${var.name_prefix}-application"
  location            = var.location
  resource_group_name = var.resource_group_name
  tags                = local.common_tags
}

resource "azurerm_network_security_group" "data" {
  name                = "nsg-${var.name_prefix}-data"
  location            = var.location
  resource_group_name = var.resource_group_name
  tags                = local.common_tags
}

resource "azurerm_network_security_group" "integration" {
  name                = "nsg-${var.name_prefix}-integration"
  location            = var.location
  resource_group_name = var.resource_group_name
  tags                = local.common_tags
}

resource "azurerm_network_security_group" "management" {
  name                = "nsg-${var.name_prefix}-management"
  location            = var.location
  resource_group_name = var.resource_group_name
  tags                = local.common_tags
}

# NSG rules: keep these explicit and auditable

resource "azurerm_network_security_rule" "application_https_in" {
  count                       = var.allow_https_internet ? 1 : 0
  name                        = "allow-https-internet"
  priority                    = 100
  direction                   = "Inbound"
  access                      = "Allow"
  protocol                    = "Tcp"
  source_port_range           = "*"
  destination_port_range      = "443"
  source_address_prefix       = "Internet"
  destination_address_prefix  = "*"
  resource_group_name         = var.resource_group_name
  network_security_group_name = azurerm_network_security_group.application.name
}

resource "azurerm_network_security_rule" "data_block_internet" {
  name                        = "deny-internet-inbound"
  priority                    = 100
  direction                   = "Inbound"
  access                      = "Deny"
  protocol                    = "*"
  source_port_range           = "*"
  destination_port_range      = "*"
  source_address_prefix       = "Internet"
  destination_address_prefix  = "*"
  resource_group_name         = var.resource_group_name
  network_security_group_name = azurerm_network_security_group.data.name
}

resource "azurerm_network_security_rule" "management_ssh" {
  count                       = length(var.ssh_source_ips) > 0 ? length(var.ssh_source_ips) : 0
  name                        = "allow-ssh-${count.index}"
  priority                    = 100 + count.index
  direction                   = "Inbound"
  access                      = "Allow"
  protocol                    = "Tcp"
  source_port_range           = "*"
  destination_port_range      = "22"
  source_address_prefix       = var.ssh_source_ips[count.index]
  destination_address_prefix  = "*"
  resource_group_name         = var.resource_group_name
  network_security_group_name = azurerm_network_security_group.management.name
}

# Subnet -> NSG associations

resource "azurerm_subnet_network_security_group_association" "application" {
  subnet_id                 = azurerm_subnet.application.id
  network_security_group_id = azurerm_network_security_group.application.id
}

resource "azurerm_subnet_network_security_group_association" "data" {
  subnet_id                 = azurerm_subnet.data.id
  network_security_group_id = azurerm_network_security_group.data.id
}

resource "azurerm_subnet_network_security_group_association" "integration" {
  subnet_id                 = azurerm_subnet.integration.id
  network_security_group_id = azurerm_network_security_group.integration.id
}

resource "azurerm_subnet_network_security_group_association" "management" {
  subnet_id                 = azurerm_subnet.management.id
  network_security_group_id = azurerm_network_security_group.management.id
}
