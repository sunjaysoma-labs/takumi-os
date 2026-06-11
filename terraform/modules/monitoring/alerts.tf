# alerts.tf
# 4 baseline metric alerts (per module README).

locals {
  action_group_ids = azurerm_monitor_action_group.this[*].id
}

resource "azurerm_monitor_metric_alert" "api_latency_p95" {
  name                = "alert-${var.name_prefix}-api-latency-p95"
  resource_group_name = var.resource_group_name
  scopes              = [azurerm_application_insights.this.id]
  description         = "API p95 latency > 1s for 5m"
  severity            = 2
  frequency           = "PT1M"
  window_size         = "PT5M"
  tags                = local.common_tags

  criteria {
    metric_namespace = "Microsoft.Insights/components"
    metric_name      = "requests/duration"
    aggregation      = "Average"
    operator         = "GreaterThan"
    threshold        = 1.0  # 1 second

    dimension {
      name     = "cloud/roleName"
      operator = "Include"
      values   = ["*"]
    }
  }

  dynamic "action" {
    for_each = local.action_group_ids
    content {
      action_group_id = action.value
    }
  }
}

resource "azurerm_monitor_metric_alert" "error_rate" {
  name                = "alert-${var.name_prefix}-error-rate"
  resource_group_name = var.resource_group_name
  scopes              = [azurerm_application_insights.this.id]
  description         = "Request failure rate > 5% for 5m"
  severity            = 1
  frequency           = "PT1M"
  window_size         = "PT5M"
  tags                = local.common_tags

  criteria {
    metric_namespace = "Microsoft.Insights/components"
    metric_name      = "requests/failed"
    aggregation      = "Average"
    operator         = "GreaterThan"
    threshold        = 5.0  # percent
  }

  dynamic "action" {
    for_each = local.action_group_ids
    content {
      action_group_id = action.value
    }
  }
}

resource "azurerm_monitor_metric_alert" "saturation" {
  name                = "alert-${var.name_prefix}-saturation"
  resource_group_name = var.resource_group_name
  scopes              = [azurerm_application_insights.this.id]
  description         = "Processor time > 80% for 5m"
  severity            = 2
  frequency           = "PT1M"
  window_size         = "PT5M"
  tags                = local.common_tags

  criteria {
    metric_namespace = "Microsoft.Insights/components"
    metric_name      = "performanceCounter/percentProcessorTime"
    aggregation      = "Average"
    operator         = "GreaterThan"
    threshold        = 80
  }

  dynamic "action" {
    for_each = local.action_group_ids
    content {
      action_group_id = action.value
    }
  }
}

# memory_retrieval_p95: custom metric emitted by the Memory
# Service. Created disabled until TKM-M0-016 ships.
resource "azurerm_monitor_metric_alert" "memory_retrieval_p95" {
  name                = "alert-${var.name_prefix}-memory-retrieval-p95"
  resource_group_name = var.resource_group_name
  scopes              = [azurerm_application_insights.this.id]
  description         = "Memory retrieval p95 > 500ms (disabled until TKM-M0-016)"
  severity            = 3
  frequency           = "PT1M"
  window_size         = "PT5M"
  enabled             = false
  tags                = local.common_tags

  criteria {
    metric_namespace = "Microsoft.Insights/components"
    metric_name      = "memory_retrieval_ms"
    aggregation      = "Average"
    operator         = "GreaterThan"
    threshold        = 500
  }

  dynamic "action" {
    for_each = local.action_group_ids
    content {
      action_group_id = action.value
    }
  }
}
