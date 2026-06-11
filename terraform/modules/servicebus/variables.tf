# variables.tf
variable "resource_group_name" { type = string }
variable "name_prefix"          { type = string }
variable "location" {
  type    = string
  default = "australiaeast"
}
variable "sku" {
  type    = string
  default = "Standard"
}
variable "capacity" {
  type    = number
  default = 1
}
variable "vnet_id"               { type = string }
variable "integration_subnet_id" { type = string }
variable "private_dns_zone_id"   { type = string }
variable "tags" {
  type    = map(string)
  default = {}
}
