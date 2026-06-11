# variables.tf
variable "resource_group_name" { type = string }
variable "name_prefix"          { type = string }
variable "location" {
  type    = string
  default = "australiaeast"
}
variable "vnet_id"             { type = string }
variable "data_subnet_id"      { type = string }
variable "private_dns_zone_id" { type = string }
variable "log_analytics_workspace_id" { type = string }
variable "kind" {
  type    = string
  default = "GlobalDocumentDB"
}
variable "consistency_level" {
  type    = string
  default = "Session"
}
variable "offer_type" {
  type    = string
  default = "Standard"
}
variable "automatic_failover_enabled" {
  type    = bool
  default = true
}
variable "local_auth_disabled" {
  type    = bool
  default = false
}
variable "pitr_retention_days" {
  type    = number
  default = 7
}
variable "memory_units_throughput" {
  type    = number
  default = 1000
}
variable "default_throughput" {
  type    = number
  default = 400
}
variable "enable_autoscale" {
  type    = bool
  default = false
}
variable "enable_public_network_access" {
  type    = bool
  default = true
}
variable "ip_allowlist" {
  type    = list(string)
  default = []
}
variable "tags" {
  type    = map(string)
  default = {}
}
