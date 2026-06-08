# dns.tf
# Private DNS zones for all the data services that will attach to
# this VNet. Zones are created empty; private endpoints (created in
# the service modules) register A records here.

resource "azurerm_private_dns_zone" "this" {
  for_each            = toset(local.private_dns_zones)
  name                = each.value
  resource_group_name = var.resource_group_name
  tags                = local.common_tags
}

resource "azurerm_private_dns_zone_virtual_network_link" "this" {
  for_each              = azurerm_private_dns_zone.this
  name                  = "vnetlink-${var.name_prefix}-${replace(each.key, ".", "-")}"
  resource_group_name   = var.resource_group_name
  private_dns_zone_name = each.value
  virtual_network_id    = azurerm_virtual_network.this.id
  registration_enabled  = false
  tags                  = local.common_tags
}
