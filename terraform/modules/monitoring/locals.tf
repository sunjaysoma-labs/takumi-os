# locals.tf
locals {
  common_tags = merge(
    var.tags,
    {
      "managed-by" = "terraform"
      "project"    = "takumi-os"
      "module"     = "monitoring"
    },
  )
}
