locals {
  # Common name components
  name_prefix = "stjago-vb"
  full_name   = "${local.name_prefix}-${var.environment}"

  # Resource naming
  resource_names = {
    resource_group     = "rg-${local.full_name}"
    sql_server        = "sql-${local.full_name}"
    sql_database      = "sqldb-${local.full_name}"
    app_service_plan  = "plan-${local.full_name}"
    app_app         = "app-${local.full_name}-app"
    public_app        = "app-${local.full_name}-public"
    signalr           = "signalr-${local.full_name}"
    key_vault         = "kv-${local.full_name}"
    app_insights      = "ai-${local.full_name}"
  }

  # Common tags
  common_tags = merge(var.tags, {
    Environment = var.environment
    Project     = local.name_prefix
    ManagedBy   = "Terraform"
    LastUpdated = timestamp()
  })

  # App settings common between admin and public apps
  common_app_settings = {
    APPINSIGHTS_INSTRUMENTATIONKEY        = azurerm_application_insights.appinsights.instrumentation_key
    APPLICATIONINSIGHTS_CONNECTION_STRING = azurerm_application_insights.appinsights.connection_string
    ASPNETCORE_ENVIRONMENT               = title(var.environment)
    WEBSITE_RUN_FROM_PACKAGE             = "1"
    Azure__SignalR__ConnectionString     = azurerm_signalr_service.signalr.primary_connection_string
  }

  # Admin app specific settings
  app_app_settings = merge(local.common_app_settings, {
    IsAdminApp = "true"
  })

  # Public app specific settings
  public_app_settings = merge(local.common_app_settings, {
    IsAdminApp = "false"
  })

  # Connection string for SQL Database
  sql_connection_string = "Server=tcp:${azurerm_mssql_server.sql.fully_qualified_domain_name},1433;Initial Catalog=${azurerm_mssql_database.db.name};Persist Security Info=False;User ID=${var.sql_admin_login};Password=${var.sql_admin_password};MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"

  # CORS settings
  cors_origins = distinct(concat(var.allowed_origins, [
    "https://${local.resource_names.app_app}.azurewebsites.net",
    "https://${local.resource_names.public_app}.azurewebsites.net"
  ]))

  # Key Vault access policies
  key_vault_access_policies = {
    app_app = {
      tenant_id = data.azurerm_client_config.current.tenant_id
      object_id = azurerm_windows_web_app.app.identity[0].principal_id
      secret_permissions = [
        "Get",
        "List"
      ]
    }
    public_app = {
      tenant_id = data.azurerm_client_config.current.tenant_id
      object_id = azurerm_windows_web_app.public.identity[0].principal_id
      secret_permissions = [
        "Get",
        "List"
      ]
    }
  }

  # Database configuration
  database_config = {
    collation      = "SQL_Latin1_General_CP1_CI_AS"
    license_type   = "LicenseIncluded"
    max_size_gb    = var.sql_database_max_size_gb
    zone_redundant = false
  }

  # SignalR features
  signalr_features = {
    flag  = "ServiceMode"
    value = "Default"
  }

  # App Service configuration
  app_service_config = {
    always_on                = true
    http2_enabled           = true
    minimum_tls_version     = "1.2"
    vnet_route_all_enabled  = false
    websockets_enabled      = true
    use_32_bit_worker      = false
  }

  # Monitoring configuration
  monitoring_config = {
    retention_in_days = var.app_insights_retention_days
    sampling_percentage = 100
  }
}
