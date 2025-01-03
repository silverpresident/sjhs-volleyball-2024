terraform {
  required_providers {
    azurerm = {
      source  = "hashicorp/azurerm"
      version = "~> 3.0"
    }
  }
}

provider "azurerm" {
  features {}
}

# Variables
variable "project_name" {
  description = "The name of the project"
  default     = "stjago-vb"
}

variable "environment" {
  description = "The environment (dev, staging, prod)"
  default     = "dev"
}

variable "location" {
  description = "The Azure region to deploy to"
  default     = "eastus"
}

variable "sql_admin_login" {
  description = "The admin username for SQL Server"
}

variable "sql_admin_password" {
  description = "The admin password for SQL Server"
  sensitive   = true
}

# Resource Group
resource "azurerm_resource_group" "rg" {
  name     = "rg-${var.project_name}-${var.environment}"
  location = var.location
  tags = {
    Environment = var.environment
    Project     = var.project_name
  }
}

# SQL Server
resource "azurerm_mssql_server" "sql" {
  name                         = "sql-${var.project_name}-${var.environment}"
  resource_group_name          = azurerm_resource_group.rg.name
  location                     = azurerm_resource_group.rg.location
  version                      = "12.0"
  administrator_login          = var.sql_admin_login
  administrator_login_password = var.sql_admin_password

  tags = {
    Environment = var.environment
    Project     = var.project_name
  }
}

# SQL Database
resource "azurerm_mssql_database" "db" {
  name           = "sqldb-${var.project_name}-${var.environment}"
  server_id      = azurerm_mssql_server.sql.id
  collation      = "SQL_Latin1_General_CP1_CI_AS"
  license_type   = "LicenseIncluded"
  max_size_gb    = 2
  sku_name       = "Basic"
  zone_redundant = false

  tags = {
    Environment = var.environment
    Project     = var.project_name
  }
}

# SignalR Service
resource "azurerm_signalr_service" "signalr" {
  name                = "signalr-${var.project_name}-${var.environment}"
  location            = azurerm_resource_group.rg.location
  resource_group_name = azurerm_resource_group.rg.name

  sku {
    name     = "Free_F1"
    capacity = 1
  }

  cors {
    allowed_origins = ["*"]
  }

  features {
    flag  = "ServiceMode"
    value = "Default"
  }

  tags = {
    Environment = var.environment
    Project     = var.project_name
  }
}

# Key Vault
resource "azurerm_key_vault" "kv" {
  name                = "kv-${var.project_name}-${var.environment}"
  location            = azurerm_resource_group.rg.location
  resource_group_name = azurerm_resource_group.rg.name
  tenant_id          = data.azurerm_client_config.current.tenant_id
  sku_name           = "standard"

  tags = {
    Environment = var.environment
    Project     = var.project_name
  }
}

# Application Insights
resource "azurerm_application_insights" "appinsights" {
  name                = "ai-${var.project_name}-${var.environment}"
  location            = azurerm_resource_group.rg.location
  resource_group_name = azurerm_resource_group.rg.name
  application_type    = "web"

  tags = {
    Environment = var.environment
    Project     = var.project_name
  }
}

# App Service Plan
resource "azurerm_service_plan" "plan" {
  name                = "plan-${var.project_name}-${var.environment}"
  location            = azurerm_resource_group.rg.location
  resource_group_name = azurerm_resource_group.rg.name
  os_type            = "Windows"
  sku_name           = "B1"

  tags = {
    Environment = var.environment
    Project     = var.project_name
  }
}

# Admin App Service
resource "azurerm_windows_web_app" "app" {
  name                = "app-${var.project_name}-app-${var.environment}"
  location            = azurerm_resource_group.rg.location
  resource_group_name = azurerm_resource_group.rg.name
  service_plan_id     = azurerm_service_plan.plan.id

  site_config {
    application_stack {
      current_stack  = "dotnet"
      dotnet_version = "v8.0"
    }
  }

  app_settings = {
    "APPINSIGHTS_INSTRUMENTATIONKEY"        = azurerm_application_insights.appinsights.instrumentation_key
    "APPLICATIONINSIGHTS_CONNECTION_STRING" = azurerm_application_insights.appinsights.connection_string
    "Azure__SignalR__ConnectionString"      = azurerm_signalr_service.signalr.primary_connection_string
    "WEBSITE_RUN_FROM_PACKAGE"             = "1"
  }

  connection_string {
    name  = "DefaultConnection"
    type  = "SQLAzure"
    value = "Server=tcp:${azurerm_mssql_server.sql.fully_qualified_domain_name},1433;Initial Catalog=${azurerm_mssql_database.db.name};Persist Security Info=False;User ID=${var.sql_admin_login};Password=${var.sql_admin_password};MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"
  }

  tags = {
    Environment = var.environment
    Project     = var.project_name
  }
}

# Public App Service
resource "azurerm_windows_web_app" "public" {
  name                = "app-${var.project_name}-public-${var.environment}"
  location            = azurerm_resource_group.rg.location
  resource_group_name = azurerm_resource_group.rg.name
  service_plan_id     = azurerm_service_plan.plan.id

  site_config {
    application_stack {
      current_stack  = "dotnet"
      dotnet_version = "v8.0"
    }
  }

  app_settings = {
    "APPINSIGHTS_INSTRUMENTATIONKEY"        = azurerm_application_insights.appinsights.instrumentation_key
    "APPLICATIONINSIGHTS_CONNECTION_STRING" = azurerm_application_insights.appinsights.connection_string
    "Azure__SignalR__ConnectionString"      = azurerm_signalr_service.signalr.primary_connection_string
    "WEBSITE_RUN_FROM_PACKAGE"             = "1"
  }

  tags = {
    Environment = var.environment
    Project     = var.project_name
  }
}

# Data Sources
data "azurerm_client_config" "current" {}

# Outputs
output "app_app_url" {
  value = "https://${azurerm_windows_web_app.app.default_hostname}"
}

output "public_app_url" {
  value = "https://${azurerm_windows_web_app.public.default_hostname}"
}

output "sql_server_fqdn" {
  value = azurerm_mssql_server.sql.fully_qualified_domain_name
}

output "signalr_connection_string" {
  value     = azurerm_signalr_service.signalr.primary_connection_string
  sensitive = true
}

output "instrumentation_key" {
  value     = azurerm_application_insights.appinsights.instrumentation_key
  sensitive = true
}
