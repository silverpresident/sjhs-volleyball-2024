# Core Infrastructure Variables
variable "project_name" {
  description = "The name of the project"
  type        = string
  default     = "stjago-vb"
}

variable "environment" {
  description = "The environment (dev, staging, prod)"
  type        = string
  default     = "dev"
  validation {
    condition     = contains(["dev", "staging", "prod"], var.environment)
    error_message = "Environment must be one of: dev, staging, prod"
  }
}

variable "location" {
  description = "The Azure region to deploy to"
  type        = string
  default     = "eastus"
}

# SQL Database Variables
variable "sql_admin_login" {
  description = "The admin username for SQL Server"
  type        = string
  sensitive   = true
}

variable "sql_admin_password" {
  description = "The admin password for SQL Server"
  type        = string
  sensitive   = true
}

variable "sql_database_sku" {
  description = "The SKU for the SQL Database"
  type        = string
  default     = "Basic"
}

variable "sql_database_max_size_gb" {
  description = "The maximum size of the SQL Database in GB"
  type        = number
  default     = 2
}

# App Service Variables
variable "app_service_sku" {
  description = "The SKU for the App Service Plan"
  type        = string
  default     = "B1"
}

# SignalR Variables
variable "signalr_sku" {
  description = "The SKU for the SignalR Service"
  type = object({
    name     = string
    capacity = number
  })
  default = {
    name     = "Free_F1"
    capacity = 1
  }
}

# Key Vault Variables
variable "key_vault_sku" {
  description = "The SKU for the Key Vault"
  type        = string
  default     = "standard"
}

# Application Insights Variables
variable "enable_application_insights" {
  description = "Whether to enable Application Insights"
  type        = bool
  default     = true
}

variable "app_insights_retention_days" {
  description = "Number of days to retain Application Insights data"
  type        = number
  default     = 90
}

# Authentication Variables
variable "google_client_id" {
  description = "Google OAuth client ID"
  type        = string
  sensitive   = true
  default     = ""
}

variable "google_client_secret" {
  description = "Google OAuth client secret"
  type        = string
  sensitive   = true
  default     = ""
}

variable "microsoft_client_id" {
  description = "Microsoft OAuth client ID"
  type        = string
  sensitive   = true
  default     = ""
}

variable "microsoft_client_secret" {
  description = "Microsoft OAuth client secret"
  type        = string
  sensitive   = true
  default     = ""
}

# Email Service Variables
variable "sendgrid_api_key" {
  description = "SendGrid API key for email notifications"
  type        = string
  sensitive   = true
  default     = ""
}

# Storage Variables
variable "storage_connection_string" {
  description = "Azure Storage connection string"
  type        = string
  sensitive   = true
  default     = ""
}

# CORS Variables
variable "allowed_origins" {
  description = "List of allowed origins for CORS"
  type        = list(string)
  default     = ["*"]
}

# Tagging Variables
variable "tags" {
  description = "Tags to apply to all resources"
  type        = map(string)
  default = {
    ManagedBy = "Terraform"
    Project   = "ST JAGO VOLLEYBALL RALLY"
  }
}

# Backup Variables
variable "enable_backup" {
  description = "Whether to enable backup for supported resources"
  type        = bool
  default     = true
}

variable "backup_retention_days" {
  description = "Number of days to retain backups"
  type        = number
  default     = 30
}

# Monitoring Variables
variable "alert_email_addresses" {
  description = "List of email addresses to receive monitoring alerts"
  type        = list(string)
  default     = []
}

variable "enable_monitoring" {
  description = "Whether to enable monitoring and alerting"
  type        = bool
  default     = true
}

# Network Variables
variable "virtual_network_address_space" {
  description = "Address space for the virtual network"
  type        = string
  default     = "10.0.0.0/16"
}

variable "subnet_address_prefixes" {
  description = "Address prefixes for subnets"
  type        = map(string)
  default = {
    app_service = "10.0.1.0/24"
    database    = "10.0.2.0/24"
  }
}

# Security Variables
variable "certificate_password" {
  description = "Password for the SSL certificate"
  type        = string
  sensitive   = true
  default     = ""
}

variable "enable_ddos_protection" {
  description = "Whether to enable DDoS protection"
  type        = bool
  default     = false
}

variable "enable_waf" {
  description = "Whether to enable Web Application Firewall"
  type        = bool
  default     = true
}

variable "waf_mode" {
  description = "Web Application Firewall mode (Detection or Prevention)"
  type        = string
  default     = "Prevention"
  validation {
    condition     = contains(["Detection", "Prevention"], var.waf_mode)
    error_message = "WAF mode must be either Detection or Prevention"
  }
}

variable "blocked_ip_ranges" {
  description = "List of IP ranges to block"
  type        = list(string)
  default     = []
}

variable "allowed_ip_ranges" {
  description = "List of IP ranges to allow"
  type        = list(string)
  default     = []
}

variable "enable_ssl" {
  description = "Whether to enable SSL/TLS"
  type        = bool
  default     = true
}

variable "min_tls_version" {
  description = "Minimum TLS version"
  type        = string
  default     = "1.2"
  validation {
    condition     = contains(["1.0", "1.1", "1.2"], var.min_tls_version)
    error_message = "Minimum TLS version must be 1.0, 1.1, or 1.2"
  }
}

variable "enable_https_only" {
  description = "Whether to enable HTTPS only access"
  type        = bool
  default     = true
}
