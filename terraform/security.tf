# Azure AD Application for Admin App
resource "azuread_application" "app" {
  display_name = "app-${local.full_name}-app"
  
  web {
    homepage_url = "https://${azurerm_windows_web_app.app.default_hostname}"
    redirect_uris = [
      "https://${azurerm_windows_web_app.app.default_hostname}/signin-oidc",
      "https://${azurerm_windows_web_app.app.default_hostname}/signin-google",
      "https://${azurerm_windows_web_app.app.default_hostname}/signin-microsoft"
    ]

    implicit_grant {
      access_token_issuance_enabled = true
      id_token_issuance_enabled     = true
    }
  }

  required_resource_access {
    resource_app_id = "00000003-0000-0000-c000-000000000000" # Microsoft Graph

    resource_access {
      id   = "e1fe6dd8-ba31-4d61-89e7-88639da4683d" # User.Read
      type = "Scope"
    }
  }

  tags = ["terraform", "volleyball-rally", local.full_name]
}

# Azure AD Service Principal for Admin App
resource "azuread_service_principal" "app" {
  application_id = azuread_application.app.application_id
  
  tags = ["terraform", "volleyball-rally", local.full_name]
}

# Web Application Firewall Policy
resource "azurerm_web_application_firewall_policy" "main" {
  name                = "waf-${local.full_name}"
  resource_group_name = azurerm_resource_group.rg.name
  location            = azurerm_resource_group.rg.location

  custom_rules {
    name      = "BlockKnownBadIPs"
    priority  = 1
    rule_type = "MatchRule"

    match_conditions {
      match_variables {
        variable_name = "RemoteAddr"
      }
      operator           = "IPMatch"
      negation_condition = false
      match_values       = ["192.168.1.0/24"] # Example IP range to block
    }

    action = "Block"
  }

  policy_settings {
    enabled                     = true
    mode                       = "Prevention"
    request_body_check         = true
    file_upload_limit_in_mb    = 100
    max_request_body_size_in_kb = 128
  }

  managed_rules {
    managed_rule_set {
      type    = "OWASP"
      version = "3.2"
    }
  }

  tags = local.common_tags
}

# DDoS Protection Plan
resource "azurerm_network_ddos_protection_plan" "main" {
  name                = "ddos-${local.full_name}"
  location            = azurerm_resource_group.rg.location
  resource_group_name = azurerm_resource_group.rg.name

  tags = local.common_tags
}

# SQL Server Firewall Rules
resource "azurerm_mssql_firewall_rule" "azure_services" {
  name             = "AllowAzureServices"
  server_id        = azurerm_mssql_server.sql.id
  start_ip_address = "0.0.0.0"
  end_ip_address   = "0.0.0.0"
}

resource "azurerm_mssql_firewall_rule" "app_service" {
  name             = "AllowAppService"
  server_id        = azurerm_mssql_server.sql.id
  start_ip_address = azurerm_subnet.app_service.address_prefixes[0]
  end_ip_address   = azurerm_subnet.app_service.address_prefixes[0]
}

# SQL Server Auditing
resource "azurerm_mssql_server_extended_auditing_policy" "sql" {
  server_id                               = azurerm_mssql_server.sql.id
  storage_endpoint                        = azurerm_storage_account.backup.primary_blob_endpoint
  storage_account_access_key              = azurerm_storage_account.backup.primary_access_key
  storage_account_access_key_is_secondary = false
  retention_in_days                       = var.backup_retention_days
}

# SQL Server Security Alert Policy
resource "azurerm_mssql_server_security_alert_policy" "sql" {
  resource_group_name = azurerm_resource_group.rg.name
  server_name         = azurerm_mssql_server.sql.name
  state              = "Enabled"
  email_account_admins = true
  email_addresses     = var.alert_email_addresses
}

# SQL Database Transparent Data Encryption
resource "azurerm_mssql_database_extended_auditing_policy" "db_encryption" {
  database_id                             = azurerm_mssql_database.db.id
  storage_endpoint                        = azurerm_storage_account.backup.primary_blob_endpoint
  storage_account_access_key              = azurerm_storage_account.backup.primary_access_key
  storage_account_access_key_is_secondary = false
  retention_in_days                       = var.backup_retention_days
}

# Role Assignments
resource "azurerm_role_assignment" "app_app_contributor" {
  scope                = azurerm_windows_web_app.app.id
  role_definition_name = "Contributor"
  principal_id         = azuread_service_principal.app.object_id
}

resource "azurerm_role_assignment" "app_app_key_vault" {
  scope                = azurerm_key_vault.kv.id
  role_definition_name = "Key Vault Secrets User"
  principal_id         = azuread_service_principal.app.object_id
}

# IP Restrictions for App Services
resource "azurerm_app_service_virtual_network_swift_connection" "app" {
  app_service_id = azurerm_windows_web_app.app.id
  subnet_id      = azurerm_subnet.app_service.id
}

resource "azurerm_app_service_virtual_network_swift_connection" "public" {
  app_service_id = azurerm_windows_web_app.public.id
  subnet_id      = azurerm_subnet.app_service.id
}

# TLS Policy
resource "azurerm_app_service_certificate" "main" {
  name                = "cert-${local.full_name}"
  resource_group_name = azurerm_resource_group.rg.name
  location            = azurerm_resource_group.rg.location
  pfx_blob           = filebase64("${path.module}/certificates/cert.pfx")
  password           = var.certificate_password
}

# Configure TLS settings for web apps
resource "azurerm_app_service_custom_hostname_binding" "app" {
  hostname            = azurerm_windows_web_app.app.default_hostname
  app_service_name    = azurerm_windows_web_app.app.name
  resource_group_name = azurerm_resource_group.rg.name
  ssl_state          = "SniEnabled"
  thumbprint         = azurerm_app_service_certificate.main.thumbprint
}

resource "azurerm_app_service_custom_hostname_binding" "public" {
  hostname            = azurerm_windows_web_app.public.default_hostname
  app_service_name    = azurerm_windows_web_app.public.name
  resource_group_name = azurerm_resource_group.rg.name
  ssl_state          = "SniEnabled"
  thumbprint         = azurerm_app_service_certificate.main.thumbprint
}
