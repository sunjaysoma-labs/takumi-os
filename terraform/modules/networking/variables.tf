# variables.tf
variable "resource_group_name" {
  description = "Resource group name (passed from env config)"
  type        = string
}

variable "name_prefix" {
  description = "Resource name prefix, e.g. takumi-dev"
  type        = string
}

variable "location" {
  description = "Azure region"
  type        = string
  default     = "australiaeast"
}

variable "address_space" {
  description = "VNet address space"
  type        = list(string)
  default     = ["10.20.0.0/16"]
}

variable "tags" {
  description = "Resource tags"
  type        = map(string)
  default     = {}
}

variable "ssh_source_ips" {
  description = "Operator IPs allowed to SSH into the management subnet (CIDR list)"
  type        = list(string)
  default     = []  # dev defaults to empty (no SSH); prod sets to a single operator IP
}

variable "allow_https_internet" {
  description = "Whether to allow 443 inbound from Internet to the application subnet (dev/test only)"
  type        = bool
  default     = false
}
