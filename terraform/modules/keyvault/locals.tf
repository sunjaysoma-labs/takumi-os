# locals.tf
locals {
  common_tags = merge(
    var.tags,
    {
      "managed-by" = "terraform"
      "project"    = "takumi-os"
      "module"     = "keyvault"
    },
  )

  # Key Vault names are globally unique, 3-24 alphanumeric
  kv_name = "kv-${var.name_prefix}-${substr(sha256("${var.name_prefix}-${var.location}"), 0, 8)}"
}
