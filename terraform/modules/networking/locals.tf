# locals.tf
locals {
  common_tags = merge(
    var.tags,
    {
      "managed-by" = "terraform"
      "project"    = "takumi-os"
      "module"     = "networking"
    },
  )

  subnets = {
    application = {
      cidrs         = [cidrsubnet(var.address_space[0], 8, 0)]   # /20
      delegation    = "aks"
      service_endpoints = ["Microsoft.Storage", "Microsoft.KeyVault", "Microsoft.Sql"]
    }
    data = {
      cidrs         = [cidrsubnet(var.address_space[0], 8, 4)]   # /22
      delegation    = null
      service_endpoints = ["Microsoft.Storage", "Microsoft.Sql", "Microsoft.AzureCosmosDB"]
    }
    integration = {
      cidrs         = [cidrsubnet(var.address_space[0], 8, 5)]   # /24
      delegation    = "servicebus"
      service_endpoints = ["Microsoft.ServiceBus", "Microsoft.EventHub"]
    }
    management = {
      cidrs         = [cidrsubnet(var.address_space[0], 8, 6)]   # /24
      delegation    = null
      service_endpoints = ["Microsoft.Storage"]
    }
  }

  private_dns_zones = [
    "privatelink.azurecr.io",
    "privatelink.blob.core.windows.net",
    "privatelink.file.core.windows.net",
    "privatelink.queue.core.windows.net",
    "privatelink.table.core.windows.net",
    "privatelink.documents.azure.com",
    "privatelink.vaultcore.azure.net",
    "privatelink.database.windows.net",
    "privatelink.azurewebsites.net",
    "privatelink.servciebus.windows.net",
    "privatelink.search.windows.net",
  ]
}
