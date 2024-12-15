terraform {
  required_version = ">= 1.0.0"

  required_providers {
    azurerm = {
      source  = "hashicorp/azurerm"
      version = "~> 3.0"
    }
    random = {
      source  = "hashicorp/random"
      version = "~> 3.0"
    }
    time = {
      source  = "hashicorp/time"
      version = "~> 0.9"
    }
  }

  # Uncomment and configure this block to use Azure Storage as backend
  # backend "azurerm" {
  #   resource_group_name  = "rg-terraform-state"
  #   storage_account_name = "stterraformstate"
  #   container_name      = "tfstate"
  #   key                 = "stjago-vb.tfstate"
  #   # Use environment variables or Azure CLI for authentication:
  #   # ARM_SUBSCRIPTION_ID
  #   # ARM_CLIENT_ID
  #   # ARM_CLIENT_SECRET
  #   # ARM_TENANT_ID
  # }
}

# Configure the Azure Provider
provider "azurerm" {
  features {
    key_vault {
      purge_soft_delete_on_destroy = true
      recover_soft_deleted_key_vaults = true
    }
    application_insights {
      disable_generated_rule = false
    }
    resource_group {
      prevent_deletion_if_contains_resources = true
    }
    template_deployment {
      delete_nested_items_during_deletion = true
    }
    virtual_machine {
      delete_os_disk_on_deletion = true
    }
  }

  # Use environment variables or Azure CLI for authentication:
  # ARM_SUBSCRIPTION_ID
  # ARM_CLIENT_ID
  # ARM_CLIENT_SECRET
  # ARM_TENANT_ID
}

# Configure the Random Provider
provider "random" {}

# Configure the Time Provider
provider "time" {}

# Data source for current Azure subscription
data "azurerm_subscription" "current" {}

# Data source for current Azure client configuration
data "azurerm_client_config" "current" {}
