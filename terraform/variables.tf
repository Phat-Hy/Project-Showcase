variable "environment" {
  type        = string
  description = "The target environment (dev, staging, prod)"
  default     = "dev"
}

variable "location" {
  type        = string
  description = "The Azure region to deploy resources"
  default     = "eastasia"
}

variable "project_name" {
  type        = string
  description = "The naming prefix for all resources"
  default     = "gara"
}
