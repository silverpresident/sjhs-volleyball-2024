# Create Key Vault
resource "azurerm_key_vault" "kv" {
  name                = local.resource_names.key_vault
  location            = azurerm_resource_group.rg.location
  resource_group_name = azurerm_resource_group.rg.name
  tenant_id          = data.azurerm_client_config.current.tenant_id
  sku_name           = var.key_vault_sku

  # Enable RBAC authentication
  enable_rbac_authorization = true
  
  # Enable soft delete and purge protection
  soft_delete_retention_days = 90
  purge_protection_enabled   = true

  # Network access configuration
  network_acls {
    default_action = "Allow"
    bypass         = "AzureServices"
  }

  tags = local.common_tags
}

# Store SQL Server credentials
resource "azurerm_key_vault_secret" "sql_admin_username" {
  name         = "sql-admin-username"
  value        = var.sql_admin_login
  key_vault_id = azurerm_key_vault.kv.id

  tags = local.common_tags
}

resource "azurerm_key_vault_secret" "sql_admin_password" {
  name         = "sql-admin-password"
  value        = var.sql_admin_password
  key_vault_id = azurerm_key_vault.kv.id

  tags = local.common_tags
}

# Store connection strings
resource "azurerm_key_vault_secret" "sql_connection_string" {
  name         = "sql-connection-string"
  value        = local.sql_connection_string
  key_vault_id = azurerm_key_vault.kv.id

  tags = local.common_tags
}

resource "azurerm_key_vault_secret" "signalr_connection_string" {
  name         = "signalr-connection-string"
  value        = azurerm_signalr_service.signalr.primary_connection_string
  key_vault_id = azurerm_key_vault.kv.id

  tags = local.common_tags
}

resource "azurerm_key_vault_secret" "app_insights_connection_string" {
  name         = "app-insights-connection-string"
  value        = azurerm_application_insights.appinsights.connection_string
  key_vault_id = azurerm_key_vault.kv.id

  tags = local.common_tags
}

# Grant access to web apps using managed identities
resource "azurerm_key_vault_access_policy" "admin_app" {
  key_vault_id = azurerm_key_vault.kv.id
  tenant_id    = data.azurerm_client_config.current.tenant_id
  object_id    = azurerm_windows_web_app.admin.identity[0].principal_id

  secret_permissions = [
    "Get",
    "List"
  ]
}

resource "azurerm_key_vault_access_policy" "public_app" {
  key_vault_id = azurerm_key_vault.kv.id
  tenant_id    = data.azurerm_client_config.current.tenant_id
  object_id    = azurerm_windows_web_app.public.identity[0].principal_id

  secret_permissions = [
    "Get",
    "List"
  ]
}

# Grant access to current user/service principal for management
resource "azurerm_key_vault_access_policy" "terraform" {
  key_vault_id = azurerm_key_vault.kv.id
  tenant_id    = data.azurerm_client_config.current.tenant_id
  object_id    = data.azurerm_client_config.current.object_id

  secret_permissions = [
    "Get",
    "List",
    "Set",
    "Delete",
    "Purge",
    "Recover"
  ]
}

# Store additional application settings
resource "azurerm_key_vault_secret" "app_settings" {
  for_each = {
    "Authentication--Google--ClientId"     = var.google_client_id
    "Authentication--Google--ClientSecret" = var.google_client_secret
    "Authentication--Microsoft--ClientId"  = var.microsoft_client_id
    "Authentication--Microsoft--ClientSecret" = var.microsoft_client_secret
    "Email--SendGrid--ApiKey"             = var.sendgrid_api_key
    "Azure--Storage--ConnectionString"     = var.storage_connection_string
  }

  name         = each.key
  value        = each.value
  key_vault_id = azurerm_key_vault.kv.id

  tags = local.common_tags
}
