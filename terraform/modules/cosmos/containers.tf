# containers.tf
# 10 containers per DIP §5. Each partitions by /tenantId.
# Throughput is per-container, autoscale or manual via variables.

resource "azurerm_cosmosdb_sql_container" "this" {
  for_each = local.container_definitions

  name                = each.key
  resource_group_name = var.resource_group_name
  account_name        = azurerm_cosmosdb_account.this.name
  database_name       = azurerm_cosmosdb_sql_database.this.name
  partition_key_paths = each.value.partition_key_paths
  default_ttl         = each.value.default_ttl

  indexing_policy {
    indexing_mode = "consistent"

    included_path {
      path = "/_etag/?"
    }

    dynamic "included_path" {
      for_each = each.value.included_paths
      content {
        path = included_path.value
      }
    }

    dynamic "excluded_path" {
      for_each = each.value.excluded_paths
      content {
        path = excluded_path.value
      }
    }
  }

  # Autoscale vs manual throughput
  dynamic "autoscale_settings" {
    for_each = var.enable_autoscale ? [1] : []
    content {
      max_throughput = each.value.autoscale_max_throughput
    }
  }

  # Manual throughput only when autoscale is disabled
  dynamic "throughput" {
    for_each = var.enable_autoscale ? [] : [1]
    content {
      throughput = each.key == "MemoryUnits" ? var.memory_units_throughput : var.default_throughput
    }
  }
}
