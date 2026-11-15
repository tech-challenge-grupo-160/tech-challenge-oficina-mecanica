terraform {
  required_version = ">= 1.9.0"

  required_providers {
    datadog = {
      source  = "DataDog/datadog"
      version = "~> 3.50"
    }
  }

  # State proprio (chave <ambiente>/datadog-os.tfstate no bucket compartilhado):
  # um erro no Datadog nao trava o apply da mensageria, e vice-versa.
  backend "s3" {}
}

# As duas chaves entram por TF_VAR_datadog_api_key e TF_VAR_datadog_app_key e
# nunca sao versionadas. A Application Key e exigida pela API de dashboards e
# monitores; a API key sozinha so envia dados.
provider "datadog" {
  api_key = var.datadog_api_key
  app_key = var.datadog_app_key
  api_url = var.datadog_api_url
}
