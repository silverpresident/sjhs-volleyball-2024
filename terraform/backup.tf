# Recovery Services Vault
resource "azurerm_recovery_services_vault" "vault" {
  count               = var.enable_backup ? 1 : 0
  name                = "rsv-${local.full_name}"
  location            = azurerm_resource_group.rg.location
  resource_group_name = azurerm_resource_group.rg.name
  sku                 = "Standard"
  soft_delete_enabled = true

  tags = local.common_tags
}

# Backup Policy for SQL Database
resource "azurerm_backup_policy_vm" "sql" {
  count               = var.enable_backup ? 1 : 0
  name                = "policy-sql-${local.full_name}"
  resource_group_name = azurerm_resource_group.rg.name
  recovery_vault_name = azurerm_recovery_services_vault.vault[0].name

  timezone = "UTC"

  backup {
    frequency = "Daily"
    time      = "23:00"
  }

  retention_daily {
    count = var.backup_retention_days
  }

  retention_weekly {
    count    = 4
    weekdays = ["Sunday"]
  }

  retention_monthly {
    count    = 12
    weekdays = ["Sunday"]
    weeks    = ["First"]
  }

  retention_yearly {
    count    = 1
    weekdays = ["Sunday"]
    weeks    = ["First"]
    months   = ["January"]
  }

  tags = local.common_tags
}

# SQL Database Long-term Retention Policy
resource "azurerm_mssql_database_extended_auditing_policy" "db" {
  database_id                             = azurerm_mssql_database.db.id
  storage_endpoint                        = azurerm_storage_account.backup.primary_blob_endpoint
  storage_account_access_key              = azurerm_storage_account.backup.primary_access_key
  storage_account_access_key_is_secondary = false
  retention_in_days                       = var.backup_retention_days
}

# Storage Account for Backups
resource "azurerm_storage_account" "backup" {
  name                     = "st${replace(local.full_name, "-", "")}backup"
  resource_group_name      = azurerm_resource_group.rg.name
  location                 = azurerm_resource_group.rg.location
  account_tier             = "Standard"
  account_replication_type = "GRS"
  min_tls_version         = "TLS1_2"

  blob_properties {
    versioning_enabled = true
    
    delete_retention_policy {
      days = var.backup_retention_days
    }

    container_delete_retention_policy {
      days = var.backup_retention_days
    }
  }

  network_rules {
    default_action = "Deny"
    bypass         = ["AzureServices"]
    ip_rules       = []
  }

  tags = local.common_tags
}

# Backup Container
resource "azurerm_storage_container" "backup" {
  name                  = "backup"
  storage_account_name  = azurerm_storage_account.backup.name
  container_access_type = "private"
}

# SQL Database Backup - Short-term Retention
resource "azurerm_mssql_database_backup_short_term_retention_policy" "db" {
  database_id = azurerm_mssql_database.db.id
  retention_days = var.backup_retention_days
}

# SQL Database Backup - Long-term Retention
resource "azurerm_mssql_database_backup_long_term_retention_policy" "db" {
  database_id = azurerm_mssql_database.db.id

  weekly_retention  = "P4W"  # 4 weeks
  monthly_retention = "P12M" # 12 months
  yearly_retention  = "P1Y"  # 1 year
  week_of_year     = 1      # First week of the year
}

# App Service Backup Configuration - Admin App
resource "azurerm_app_service_backup" "admin" {
  count                = var.enable_backup ? 1 : 0
  app_service_name     = azurerm_windows_web_app.admin.name
  resource_group_name  = azurerm_resource_group.rg.name
  storage_account_url  = "https://${azurerm_storage_account.backup.name}.blob.core.windows.net/${azurerm_storage_container.backup.name}${azurerm_storage_account.backup.primary_access_key}"

  schedule {
    frequency_interval = 1
    frequency_unit    = "Day"
    keep_at_least_one_backup = true
    retention_period_days    = var.backup_retention_days
    start_time              = "2023-01-01T23:00:00Z"
  }
}

# App Service Backup Configuration - Public App
resource "azurerm_app_service_backup" "public" {
  count                = var.enable_backup ? 1 : 0
  app_service_name     = azurerm_windows_web_app.public.name
  resource_group_name  = azurerm_resource_group.rg.name
  storage_account_url  = "https://${azurerm_storage_account.backup.name}.blob.core.windows.net/${azurerm_storage_container.backup.name}${azurerm_storage_account.backup.primary_access_key}"

  schedule {
    frequency_interval = 1
    frequency_unit    = "Day"
    keep_at_least_one_backup = true
    retention_period_days    = var.backup_retention_days
    start_time              = "2023-01-01T23:30:00Z"
  }
}

# Diagnostic Settings for Recovery Services Vault
resource "azurerm_monitor_diagnostic_setting" "vault" {
  count                      = var.enable_backup ? 1 : 0
  name                       = "diag-vault-${local.full_name}"
  target_resource_id        = azurerm_recovery_services_vault.vault[0].id
  log_analytics_workspace_id = azurerm_log_analytics_workspace.main.id

  log {
    category = "AzureBackupReport"
    enabled  = true

    retention_policy {
      enabled = true
      days    = var.backup_retention_days
    }
  }

  log {
    category = "CoreAzureBackup"
    enabled  = true

    retention_policy {
      enabled = true
      days    = var.backup_retention_days
    }
  }

  metric {
    category = "AllMetrics"
    enabled  = true

    retention_policy {
      enabled = true
      days    = var.backup_retention_days
    }
  }
}
