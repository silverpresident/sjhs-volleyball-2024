# Application Insights
resource "azurerm_application_insights" "appinsights" {
  name                = local.resource_names.app_insights
  location            = azurerm_resource_group.rg.location
  resource_group_name = azurerm_resource_group.rg.name
  application_type    = "web"
  retention_in_days   = var.app_insights_retention_days
  sampling_percentage = local.monitoring_config.sampling_percentage

  tags = local.common_tags
}

# Action Group for Alerts
resource "azurerm_monitor_action_group" "main" {
  name                = "ag-${local.full_name}"
  resource_group_name = azurerm_resource_group.rg.name
  short_name          = "alerts"

  dynamic "email_receiver" {
    for_each = var.alert_email_addresses
    content {
      name                    = "email${email_receiver.key + 1}"
      email_address          = email_receiver.value
      use_common_alert_schema = true
    }
  }

  tags = local.common_tags
}

# CPU Usage Alert
resource "azurerm_monitor_metric_alert" "cpu_usage" {
  count               = var.enable_monitoring ? 1 : 0
  name                = "alert-cpu-${local.full_name}"
  resource_group_name = azurerm_resource_group.rg.name
  scopes = [
    azurerm_service_plan.plan.id
  ]
  description = "Alert when CPU usage exceeds threshold"

  criteria {
    metric_namespace = "Microsoft.Web/serverfarms"
    metric_name      = "CpuPercentage"
    aggregation      = "Average"
    operator         = "GreaterThan"
    threshold        = 80
  }

  window_size        = "PT15M"
  frequency          = "PT5M"
  severity           = 2
  action_group_id    = azurerm_monitor_action_group.main.id

  tags = local.common_tags
}

# Memory Usage Alert
resource "azurerm_monitor_metric_alert" "memory_usage" {
  count               = var.enable_monitoring ? 1 : 0
  name                = "alert-memory-${local.full_name}"
  resource_group_name = azurerm_resource_group.rg.name
  scopes = [
    azurerm_service_plan.plan.id
  ]
  description = "Alert when memory usage exceeds threshold"

  criteria {
    metric_namespace = "Microsoft.Web/serverfarms"
    metric_name      = "MemoryPercentage"
    aggregation      = "Average"
    operator         = "GreaterThan"
    threshold        = 80
  }

  window_size        = "PT15M"
  frequency          = "PT5M"
  severity           = 2
  action_group_id    = azurerm_monitor_action_group.main.id

  tags = local.common_tags
}

# HTTP Server Errors Alert
resource "azurerm_monitor_metric_alert" "http_5xx" {
  count               = var.enable_monitoring ? 1 : 0
  name                = "alert-http5xx-${local.full_name}"
  resource_group_name = azurerm_resource_group.rg.name
  scopes = [
    azurerm_windows_web_app.app.id,
    azurerm_windows_web_app.public.id
  ]
  description = "Alert when HTTP 5xx errors exceed threshold"

  criteria {
    metric_namespace = "Microsoft.Web/sites"
    metric_name      = "Http5xx"
    aggregation      = "Total"
    operator         = "GreaterThan"
    threshold        = 10
  }

  window_size        = "PT15M"
  frequency          = "PT5M"
  severity           = 1
  action_group_id    = azurerm_monitor_action_group.main.id

  tags = local.common_tags
}

# Response Time Alert
resource "azurerm_monitor_metric_alert" "response_time" {
  count               = var.enable_monitoring ? 1 : 0
  name                = "alert-response-time-${local.full_name}"
  resource_group_name = azurerm_resource_group.rg.name
  scopes = [
    azurerm_windows_web_app.app.id,
    azurerm_windows_web_app.public.id
  ]
  description = "Alert when response time exceeds threshold"

  criteria {
    metric_namespace = "Microsoft.Web/sites"
    metric_name      = "AverageResponseTime"
    aggregation      = "Average"
    operator         = "GreaterThan"
    threshold        = 5000  # 5 seconds
  }

  window_size        = "PT15M"
  frequency          = "PT5M"
  severity           = 2
  action_group_id    = azurerm_monitor_action_group.main.id

  tags = local.common_tags
}

# Database DTU Usage Alert
resource "azurerm_monitor_metric_alert" "dtu_usage" {
  count               = var.enable_monitoring ? 1 : 0
  name                = "alert-dtu-${local.full_name}"
  resource_group_name = azurerm_resource_group.rg.name
  scopes = [
    azurerm_mssql_database.db.id
  ]
  description = "Alert when DTU usage exceeds threshold"

  criteria {
    metric_namespace = "Microsoft.Sql/servers/databases"
    metric_name      = "dtu_consumption_percent"
    aggregation      = "Average"
    operator         = "GreaterThan"
    threshold        = 80
  }

  window_size        = "PT15M"
  frequency          = "PT5M"
  severity           = 2
  action_group_id    = azurerm_monitor_action_group.main.id

  tags = local.common_tags
}

# Log Analytics Workspace
resource "azurerm_log_analytics_workspace" "main" {
  name                = "log-${local.full_name}"
  location            = azurerm_resource_group.rg.location
  resource_group_name = azurerm_resource_group.rg.name
  sku                 = "PerGB2018"
  retention_in_days   = var.app_insights_retention_days

  tags = local.common_tags
}

# Link App Services to Log Analytics
resource "azurerm_monitor_diagnostic_setting" "app_app" {
  name                       = "diag-app-app-${local.full_name}"
  target_resource_id        = azurerm_windows_web_app.app.id
  log_analytics_workspace_id = azurerm_log_analytics_workspace.main.id

  log {
    category = "AppServiceHTTPLogs"
    enabled  = true

    retention_policy {
      enabled = true
      days    = var.app_insights_retention_days
    }
  }

  metric {
    category = "AllMetrics"
    enabled  = true

    retention_policy {
      enabled = true
      days    = var.app_insights_retention_days
    }
  }
}

resource "azurerm_monitor_diagnostic_setting" "public_app" {
  name                       = "diag-public-app-${local.full_name}"
  target_resource_id        = azurerm_windows_web_app.public.id
  log_analytics_workspace_id = azurerm_log_analytics_workspace.main.id

  log {
    category = "AppServiceHTTPLogs"
    enabled  = true

    retention_policy {
      enabled = true
      days    = var.app_insights_retention_days
    }
  }

  metric {
    category = "AllMetrics"
    enabled  = true

    retention_policy {
      enabled = true
      days    = var.app_insights_retention_days
    }
  }
}
