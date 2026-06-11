# namespace.tf
resource "azurerm_servicebus_namespace" "this" {
  name                = "sb-${var.name_prefix}-${substr(sha256(var.name_prefix), 0, 6)}"
  location            = var.location
  resource_group_name = var.resource_group_name
  sku                 = var.sku
  capacity            = var.sku == "Premium" ? var.capacity : 0
  tags                = local.common_tags

  # Network rules: default deny traffic outside the VNet
  network_rule_set {
    default_action                = "Deny"
    public_network_access_enabled = false
    trusted_service_access_enabled = true
  }
}

resource "azurerm_servicebus_topic" "this" {
  for_each            = local.topic_definitions
  name                = each.key
  namespace_id        = azurerm_servicebus_namespace.this.id
  max_size_in_megabytes = each.value.max_size_mb
  default_message_ttl  = each.value.default_ttl
  enable_partitioning  = true
  tags                 = local.common_tags
}

# Default audit-sink subscription on every topic. Routes dead
# letters to a sibling queue.
resource "azurerm_servicebus_subscription" "audit_sink" {
  for_each     = azurerm_servicebus_topic.this
  name         = "audit-sink"
  topic_id     = each.value.id
  max_delivery_count = 10
  default_message_ttl = "P30D"
  lock_duration       = "PT1M"

  # Enable dead-lettering on message expiration
  dead_lettering_on_message_expiration = true
}

resource "azurerm_servicebus_namespace_network_rule_set" "private_endpoint" {
  namespace_id = azurerm_servicebus_namespace.this.id
  default_action = "Deny"

  network_rules {
    subnet_id                            = var.integration_subnet_id
    ignore_missing_vnet_service_endpoint = false
  }
}

resource "azurerm_private_endpoint" "servicebus" {
  name                = "pe-sb-${var.name_prefix}"
  location            = var.location
  resource_group_name = var.resource_group_name
  subnet_id           = var.integration_subnet_id
  tags                = local.common_tags

  private_service_connection {
    name                           = "psc-sb"
    private_connection_resource_id = azurerm_servicebus_namespace.this.id
    is_manual_connection           = false
    subresource_names              = ["namespace"]
  }

  private_dns_zone_group {
    name                 = "sb-dns-group"
    private_dns_zone_ids = [var.private_dns_zone_id]
  }
}
