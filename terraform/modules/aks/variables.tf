# variables.tf
variable "resource_group_name" { type = string }
variable "name_prefix"          { type = string }
variable "location" {
  type    = string
  default = "australiaeast"
}
variable "application_subnet_id"  { type = string }
variable "kubernetes_version" {
  type    = string
  default = null
}
variable "oidc_issuer_enabled" {
  type    = bool
  default = true
}
variable "workload_identity_enabled" {
  type    = bool
  default = true
}
variable "key_vault_id"            { type = string }
variable "log_analytics_workspace_id" { type = string }
variable "private_cluster_enabled" {
  type    = bool
  default = false
}
variable "tags" {
  type    = map(string)
  default = {}
}

variable "aks_admin_group_object_ids" {
  description = "Entra ID group object IDs that get cluster-admin (R2: tighten to a single ops group)"
  type        = list(string)
  default     = []  # dev/test: empty allows AKS to use local admin (then disabled); prod: required
}

# Node pool sizing per module README. Defaults are dev-sized;
# prod overrides via tfvars.
variable "system_pool_vm_size" {
  type    = string
  default = "Standard_B2s"
}
variable "system_pool_min" {
  type    = number
  default = 1
}
variable "system_pool_max" {
  type    = number
  default = 3
}
variable "core_pool_vm_size" {
  type    = string
  default = "Standard_D2s_v5"
}
variable "core_pool_min" {
  type    = number
  default = 2
}
variable "core_pool_max" {
  type    = number
  default = 10
}
variable "agent_pool_vm_size" {
  type    = string
  default = "Standard_D2s_v5"
}
variable "agent_pool_min" {
  type    = number
  default = 2
}
variable "agent_pool_max" {
  type    = number
  default = 10
}
variable "data_pool_vm_size" {
  type    = string
  default = "Standard_E2s_v5"
}
variable "data_pool_min" {
  type    = number
  default = 0
}
variable "data_pool_max" {
  type    = number
  default = 4
}
