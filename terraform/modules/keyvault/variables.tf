# variables.tf
variable "resource_group_name" { type = string }
variable "name_prefix"          { type = string }
variable "location" {
  type    = string
  default = "australiaeast"
}
variable "tenant_id"   { type = string }
variable "vnet_id"     { type = string }
variable "data_subnet_id" { type = string }
variable "private_dns_zone_id" { type = string }
variable "sku_name" {
  type    = string
  default = "standard"
}
variable "soft_delete_retention_days" {
  type    = number
  default = 90
}
variable "enable_public_network_access" {
  description = "Allow public network access in dev; disable in prod"
  type        = bool
  default     = false
}
variable "tags" {
  type    = map(string)
  default = {}
}
