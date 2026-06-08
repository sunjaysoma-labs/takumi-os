# variables.tf
variable "resource_group_name" { type = string }
variable "name_prefix"          { type = string }
variable "location" {
  type    = string
  default = "australiaeast"
}
variable "log_retention_days" {
  type    = number
  default = 30
}
variable "tags" {
  type    = map(string)
  default = {}
}

variable "alert_email_recipients" {
  description = "Operator email addresses for action group routing"
  type        = list(string)
  default     = []
}
