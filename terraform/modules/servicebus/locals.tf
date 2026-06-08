# locals.tf
locals {
  common_tags = merge(
    var.tags,
    {
      "managed-by" = "terraform"
      "project"    = "takumi-os"
      "module"     = "servicebus"
    },
  )

  # 8 domain topics per [[takumi-os-azure-architecture]] §Event
  # Driven Architecture. The list is the source of truth — adding
  # a new event means adding to this list.
  topic_definitions = {
    "memory.created" = {
      max_size_mb = 1024
      default_ttl = "P14D"  # audit trail
    }
    "decision.recorded" = {
      max_size_mb = 1024
      default_ttl = "P365D"
    }
    "process.updated" = {
      max_size_mb = 1024
      default_ttl = "P90D"
    }
    "kpi.changed" = {
      max_size_mb = 512
      default_ttl = "P30D"
    }
    "risk.detected" = {
      max_size_mb = 512
      default_ttl = "P365D"
    }
    "project.completed" = {
      max_size_mb = 512
      default_ttl = "P365D"
    }
    "agent.invoked" = {
      max_size_mb = 2048
      default_ttl = "P30D"
    }
    "recommendation.generated" = {
      max_size_mb = 1024
      default_ttl = "P365D"
    }
  }
}
