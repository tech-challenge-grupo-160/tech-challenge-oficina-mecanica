variable "project" {
  description = "Prefixo compartilhado dos recursos AWS (filas SQS)."
  type        = string
  default     = "tc-grupo160"
}

variable "environment" {
  description = "Ambiente logico. Vira o env:<ambiente> das consultas e o prefixo das filas."
  type        = string

  validation {
    condition     = contains(["dev", "hom", "prod"], var.environment)
    error_message = "environment deve ser dev, hom ou prod."
  }
}

variable "datadog_api_key" {
  description = "API key da organizacao Datadog (a mesma do Agent)."
  type        = string
  sensitive   = true
}

variable "datadog_app_key" {
  description = "Application key da organizacao Datadog, exigida para criar dashboards e monitores."
  type        = string
  sensitive   = true
}

variable "datadog_api_url" {
  description = "Endpoint da API do site Datadog (datadoghq.com, o mesmo do Agent)."
  type        = string
  default     = "https://api.datadoghq.com/"
}

variable "servico_os" {
  description = "DD_SERVICE do OS Service, que orquestra a Saga."
  type        = string
  default     = "oficina-mecanica-api"
}

variable "servicos_apm" {
  description = "DD_SERVICE dos tres microsservicos, para os paineis de latencia."
  type        = list(string)
  default     = ["oficina-mecanica-api", "oficina-mecanica-billing", "oficina-mecanica-execucao"]
}

variable "notificar" {
  description = "Destinos dos alertas, no formato do Datadog (ex.: [\"@fulano@exemplo.com\"]). Vazio, o monitor so muda de estado."
  type        = list(string)
  default     = []
}

variable "taxa_compensacao_critica" {
  description = "Percentual de sagas compensadas, na ultima hora, que dispara o alerta."
  type        = number
  default     = 30

  validation {
    condition     = var.taxa_compensacao_critica > 0 && var.taxa_compensacao_critica <= 100
    error_message = "taxa_compensacao_critica deve ficar entre 0 (exclusivo) e 100."
  }
}

variable "taxa_compensacao_alerta" {
  description = "Percentual de sagas compensadas, na ultima hora, que gera aviso (warning)."
  type        = number
  default     = 15

  validation {
    condition     = var.taxa_compensacao_alerta > 0 && var.taxa_compensacao_alerta <= 100
    error_message = "taxa_compensacao_alerta deve ficar entre 0 (exclusivo) e 100."
  }
}
