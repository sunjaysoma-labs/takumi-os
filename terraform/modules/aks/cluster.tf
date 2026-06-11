# cluster.tf
# AKS cluster with 4 node pools per module README.
# System pool has mode=System (AKS uses it for control plane
# pods); the other three are user pools with mode=User.

resource "azurerm_kubernetes_cluster" "this" {
  name                = "aks-${var.name_prefix}"
  location            = var.location
  resource_group_name = var.resource_group_name
  kubernetes_version  = var.kubernetes_version
  tags                = local.common_tags

  dns_prefix = "aks-${var.name_prefix}"

  default_node_pool {
    name                = "system"
    vm_size             = var.system_pool_vm_size
    min_count           = var.system_pool_min
    max_count           = var.system_pool_max
    auto_scaling_enabled = true
    os_disk_type        = "Ephemeral"
    vnet_subnet_id      = var.application_subnet_id
    upgrade_settings {
      max_surge = "10%"
    }
    zones = ["1", "2", "3"]
  }

  identity {
    type = "SystemAssigned"
  }

  oidc_issuer_enabled       = var.oidc_issuer_enabled
  workload_identity_enabled = var.workload_identity_enabled

  network_profile {
    network_plugin    = "azure"
    network_plugin_mode = "overlay"
    network_policy    = "calico"
    outbound_type     = "loadBalancer"
    load_balancer_sku = "standard"
  }

  # Private cluster in prod only
  private_cluster_enabled = var.private_cluster_enabled

  # RBAC via Entra ID
  azure_active_directory_role_based_access_control {
    managed            = true
    azure_rbac_enabled = true
    admin_group_object_ids = var.aks_admin_group_object_ids
  }

  # Key Vault CSI driver
  key_vault_secrets_provider {
    secret_rotation_enabled  = true
    secret_rotation_interval = "15m"
  }

  # OMS / Container Insights
  oms_agent {
    log_analytics_workspace_id = var.log_analytics_workspace_id
  }

  # Microsoft Defender for Containers
  microsoft_defender {
    log_analytics_workspace_id = var.log_analytics_workspace_id
  }
}

# User node pools (core, agent, data)
resource "azurerm_kubernetes_cluster_node_pool" "core" {
  name                  = "core-pool"
  kubernetes_cluster_id = azurerm_kubernetes_cluster.this.id
  vm_size               = var.core_pool_vm_size
  min_count             = var.core_pool_min
  max_count             = var.core_pool_max
  auto_scaling_enabled  = true
  os_disk_type          = "Ephemeral"
  vnet_subnet_id        = var.application_subnet_id
  node_labels = {
    "takumi.os/pool" = "core"
  }
  upgrade_settings {
    max_surge = "10%"
  }
  zones = ["1", "2", "3"]
}

resource "azurerm_kubernetes_cluster_node_pool" "agent" {
  name                  = "agent-pool"
  kubernetes_cluster_id = azurerm_kubernetes_cluster.this.id
  vm_size               = var.agent_pool_vm_size
  min_count             = var.agent_pool_min
  max_count             = var.agent_pool_max
  auto_scaling_enabled  = true
  os_disk_type          = "Ephemeral"
  vnet_subnet_id        = var.application_subnet_id
  node_labels = {
    "takumi.os/pool" = "agent"
  }
  upgrade_settings {
    max_surge = "10%"
  }
  zones = ["1", "2", "3"]
}

resource "azurerm_kubernetes_cluster_node_pool" "data" {
  name                  = "data-pool"
  kubernetes_cluster_id = azurerm_kubernetes_cluster.this.id
  vm_size               = var.data_pool_vm_size
  min_count             = var.data_pool_min
  max_count             = var.data_pool_max
  auto_scaling_enabled  = true
  os_disk_type          = "Ephemeral"
  vnet_subnet_id        = var.application_subnet_id
  node_labels = {
    "takumi.os/pool" = "data"
  }
  upgrade_settings {
    max_surge = "10%"
  }
  zones = ["1", "2", "3"]
}
