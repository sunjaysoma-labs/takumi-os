# locals.tf
locals {
  common_tags = merge(
    var.tags,
    {
      "managed-by" = "terraform"
      "project"    = "takumi-os"
      "module"     = "cosmos"
    },
  )

  # 10 domain containers per DIP §5. Order matters: MemoryUnits is
  # the source of truth, gets its own throughput setting.
  container_definitions = {
    MemoryUnits = {
      partition_key_paths   = ["/tenantId"]
      default_ttl           = 0
      included_paths        = ["/tenantId/?", "/SourceSystemId/?", "/MemoryType/?", "/confidenceScore/?", "/createdAt/?"]
      excluded_paths        = ["/body/?", "/context/?", "/event/?", "/decision/?", "/outcome/?"]
      autoscale_max_throughput = 4000
    }
    KPIs = {
      partition_key_paths   = ["/tenantId"]
      default_ttl           = 0
      included_paths        = ["/tenantId/?", "/name/?", "/ownerId/?", "/unit/?"]
      excluded_paths        = []
      autoscale_max_throughput = 1000
    }
    Initiatives = {
      partition_key_paths   = ["/tenantId"]
      default_ttl           = 0
      included_paths        = ["/tenantId/?", "/ownerId/?", "/status/?", "/dueDate/?"]
      excluded_paths        = []
      autoscale_max_throughput = 1000
    }
    Processes = {
      partition_key_paths   = ["/tenantId"]
      default_ttl           = 0
      included_paths        = ["/tenantId/?", "/owner/?", "/status/?"]
      excluded_paths        = []
      autoscale_max_throughput = 1000
    }
    Projects = {
      partition_key_paths   = ["/tenantId"]
      default_ttl           = 0
      included_paths        = ["/tenantId/?", "/owner/?", "/status/?"]
      excluded_paths        = []
      autoscale_max_throughput = 1000
    }
    Decisions = {
      partition_key_paths   = ["/tenantId"]
      default_ttl           = 0
      included_paths        = ["/tenantId/?", "/madeBy/?", "/relatedProjectId/?"]
      excluded_paths        = []
      autoscale_max_throughput = 1000
    }
    Risks = {
      partition_key_paths   = ["/tenantId"]
      default_ttl           = 0
      included_paths        = ["/tenantId/?", "/severity/?", "/status/?", "/owner/?"]
      excluded_paths        = []
      autoscale_max_throughput = 1000
    }
    Recommendations = {
      partition_key_paths   = ["/tenantId"]
      default_ttl           = 0
      included_paths        = ["/tenantId/?", "/agentId/?", "/confidence/?"]
      excluded_paths        = []
      autoscale_max_throughput = 1000
    }
    JobRuns = {
      partition_key_paths   = ["/tenantId"]
      default_ttl           = 2592000  # 30 days in seconds
      included_paths        = ["/tenantId/?", "/status/?", "/startedAt/?"]
      excluded_paths        = []
      autoscale_max_throughput = 1000
    }
    SourceEvents = {
      partition_key_paths   = ["/tenantId"]
      default_ttl           = 7776000  # 90 days in seconds
      included_paths        = ["/tenantId/?", "/sourceSystemId/?", "/eventType/?", "/receivedAt/?"]
      excluded_paths        = []
      autoscale_max_throughput = 1000
    }
  }
}
