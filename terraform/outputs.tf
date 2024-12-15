# Web App URLs
output "admin_app_url" {
  description = "The URL of the admin web application"
  value       = "https://${azurerm_windows_web_app.admin.default_hostname}"
}

output "public_app_url" {
  description = "The URL of the public web application"
  value       = "https://${azurerm_windows_web_app.public.default_hostname}"
}

# Database Information
output "sql_server_name" {
  description = "The name of the SQL Server"
  value       = azurerm_mssql_server.sql.name
}

output "sql_server_fqdn" {
  description = "The fully qualified domain name of the SQL Server"
  value       = azurerm_mssql_server.sql.fully_qualified_domain_name
}

output "sql_database_name" {
  description = "The name of the SQL Database"
  value       = azurerm_mssql_database.db.name
}

# SignalR Information
output "signalr_service_name" {
  description = "The name of the SignalR service"
  value       = azurerm_signalr_service.signalr.name
}

output "signalr_connection_string" {
  description = "The connection string for the SignalR service"
  value       = azurerm_signalr_service.signalr.primary_connection_string
  sensitive   = true
}

# Application Insights
output "application_insights_name" {
  description = "The name of the Application Insights instance"
  value       = azurerm_application_insights.appinsights.name
}

output "application_insights_instrumentation_key" {
  description = "The instrumentation key for Application Insights"
  value       = azurerm_application_insights.appinsights.instrumentation_key
  sensitive   = true
}

output "application_insights_connection_string" {
  description = "The connection string for Application Insights"
  value       = azurerm_application_insights.appinsights.connection_string
  sensitive   = true
}

# Key Vault
output "key_vault_name" {
  description = "The name of the Key Vault"
  value       = azurerm_key_vault.kv.name
}

output "key_vault_uri" {
  description = "The URI of the Key Vault"
  value       = azurerm_key_vault.kv.vault_uri
}

# Resource Group
output "resource_group_name" {
  description = "The name of the resource group"
  value       = azurerm_resource_group.rg.name
}

# App Service Plan
output "app_service_plan_name" {
  description = "The name of the App Service Plan"
  value       = azurerm_service_plan.plan.name
}

# Important URLs for Configuration
output "important_urls" {
  description = "Important URLs for application configuration"
  value = {
    admin_app     = "https://${azurerm_windows_web_app.admin.default_hostname}"
    public_app    = "https://${azurerm_windows_web_app.public.default_hostname}"
    sql_server    = azurerm_mssql_server.sql.fully_qualified_domain_name
    key_vault     = azurerm_key_vault.kv.vault_uri
    app_insights  = "https://portal.azure.com/#resource${azurerm_application_insights.appinsights.id}/overview"
  }
}

# Connection Strings
output "connection_strings" {
  description = "Connection strings for the applications"
  sensitive   = true
  value = {
    sql_connection_string = "Server=tcp:${azurerm_mssql_server.sql.fully_qualified_domain_name},1433;Initial Catalog=${azurerm_mssql_database.db.name};Persist Security Info=False;User ID=${var.sql_admin_login};Password=${var.sql_admin_password};MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"
    signalr_connection_string = azurerm_signalr_service.signalr.primary_connection_string
    app_insights_connection_string = azurerm_application_insights.appinsights.connection_string
  }
}

# Resource IDs
output "resource_ids" {
  description = "Azure Resource IDs for all created resources"
  value = {
    resource_group     = azurerm_resource_group.rg.id
    sql_server        = azurerm_mssql_server.sql.id
    sql_database      = azurerm_mssql_database.db.id
    signalr_service   = azurerm_signalr_service.signalr.id
    key_vault         = azurerm_key_vault.kv.id
    app_insights      = azurerm_application_insights.appinsights.id
    app_service_plan  = azurerm_service_plan.plan.id
    admin_app         = azurerm_windows_web_app.admin.id
    public_app        = azurerm_windows_web_app.public.id
  }
}
