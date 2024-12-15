# This file contains the backend configuration for storing Terraform state.
# By default, it's commented out to use local state. Uncomment and configure
# to use Azure Storage as a backend for team collaboration.

/*
terraform {
  backend "azurerm" {
    # Resource group containing the storage account
    resource_group_name = "rg-terraform-state"

    # Storage account name
    storage_account_name = "stterraformstate"

    # Container name in the storage account
    container_name = "tfstate"

    # State file name
    key = "stjago-vb.tfstate"

    # Use these environment variables for authentication:
    # ARM_SUBSCRIPTION_ID
    # ARM_CLIENT_ID
    # ARM_CLIENT_SECRET
    # ARM_TENANT_ID
  }
}
*/

# Instructions for setting up Azure Storage backend:
#
# 1. Create a resource group:
#    az group create --name rg-terraform-state --location eastus
#
# 2. Create a storage account:
#    az storage account create \
#      --name stterraformstate \
#      --resource-group rg-terraform-state \
#      --location eastus \
#      --sku Standard_LRS \
#      --encryption-services blob
#
# 3. Create a container:
#    az storage container create \
#      --name tfstate \
#      --account-name stterraformstate
#
# 4. Get the storage account key:
#    az storage account keys list \
#      --resource-group rg-terraform-state \
#      --account-name stterraformstate \
#      --query [0].value -o tsv
#
# 5. Set environment variables for authentication:
#    export ARM_SUBSCRIPTION_ID="your-subscription-id"
#    export ARM_CLIENT_ID="your-client-id"
#    export ARM_CLIENT_SECRET="your-client-secret"
#    export ARM_TENANT_ID="your-tenant-id"
#
# 6. Initialize Terraform with backend:
#    terraform init \
#      -backend-config="storage_account_name=stterraformstate" \
#      -backend-config="container_name=tfstate" \
#      -backend-config="key=stjago-vb.tfstate" \
#      -backend-config="resource_group_name=rg-terraform-state"
#
# Note: Replace placeholder values with your actual Azure subscription details.
# It's recommended to use Azure Key Vault or similar service to store these
# sensitive values in a production environment.

# Optional: Configure state locking using Azure Storage blob lease
# This helps prevent concurrent state operations by multiple users
/*
terraform {
  backend "azurerm" {
    # ... other configuration ...
    
    # Enable state locking
    use_microsoft_graph = true
    use_azuread_auth   = true
    
    # Lease timeout in seconds (optional, default is 60)
    lease_timeout      = 60
  }
}
*/
