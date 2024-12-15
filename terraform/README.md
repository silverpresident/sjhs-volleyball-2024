# Terraform Configuration for ST JAGO VOLLEYBALL RALLY

This directory contains Terraform configurations to provision and manage the Azure infrastructure required for the ST JAGO VOLLEYBALL RALLY application.

## Infrastructure Components

- Azure App Services (Admin and Public web applications)
- Azure SQL Database
- Azure SignalR Service
- Azure Key Vault
- Application Insights
- Azure Storage Account (for Terraform state)

## Prerequisites

1. [Terraform](https://www.terraform.io/downloads.html) (>= 1.0.0)
2. [Azure CLI](https://docs.microsoft.com/en-us/cli/azure/install-azure-cli)
3. Azure Subscription with required permissions

## Getting Started

1. Login to Azure:
   ```bash
   az login
   az account set --subscription "Your Subscription ID"
   ```

2. Initialize Terraform:
   ```bash
   terraform init
   ```

3. Create a `terraform.tfvars` file:
   ```bash
   cp terraform.tfvars.example terraform.tfvars
   ```

4. Edit `terraform.tfvars` with your desired values.

5. Set sensitive variables using environment variables:
   ```bash
   export TF_VAR_sql_admin_login="your-admin-username"
   export TF_VAR_sql_admin_password="your-secure-password"
   ```

## Usage

1. Plan the infrastructure changes:
   ```bash
   terraform plan -out=tfplan
   ```

2. Apply the changes:
   ```bash
   terraform apply tfplan
   ```

3. Destroy infrastructure (if needed):
   ```bash
   terraform destroy
   ```

## Directory Structure

```
terraform/
├── main.tf                 # Main infrastructure configuration
├── variables.tf            # Variable definitions
├── outputs.tf             # Output definitions
├── locals.tf              # Local variables
├── versions.tf            # Provider and version configurations
├── backend.tf             # State backend configuration
├── terraform.tfvars       # Variable values (git-ignored)
├── terraform.tfvars.example # Example variable values
└── .gitignore            # Git ignore rules
```

## Remote State

By default, the state is stored locally. To enable remote state:

1. Create Azure Storage resources:
   ```bash
   az group create --name rg-terraform-state --location eastus
   
   az storage account create \
     --name stterraformstate \
     --resource-group rg-terraform-state \
     --location eastus \
     --sku Standard_LRS
   
   az storage container create \
     --name tfstate \
     --account-name stterraformstate
   ```

2. Uncomment and configure the backend block in `backend.tf`

3. Reinitialize Terraform:
   ```bash
   terraform init -migrate-state
   ```

## Environment Variables

Required environment variables:
- `ARM_SUBSCRIPTION_ID`
- `ARM_CLIENT_ID`
- `ARM_CLIENT_SECRET`
- `ARM_TENANT_ID`

Optional environment variables:
- `TF_VAR_sql_admin_login`
- `TF_VAR_sql_admin_password`
- `TF_LOG=DEBUG` (for troubleshooting)

## Tagging Strategy

Resources are tagged with:
- Environment (dev/staging/prod)
- Project name
- Managed by (Terraform)
- Last updated timestamp

## Security Considerations

1. Use Azure Key Vault for sensitive values
2. Enable encryption at rest for all supported resources
3. Use managed identities where possible
4. Implement network security rules
5. Enable monitoring and logging

## Cost Management

- Free tier resources are used where possible in development
- Scale up resources in production as needed
- Monitor resource usage and costs
- Use auto-scaling where appropriate

## Maintenance

1. Regularly update provider versions
2. Monitor for security advisories
3. Review and update access policies
4. Maintain documentation
5. Test infrastructure changes in development first

## Troubleshooting

1. Check Azure portal for resource status
2. Review Terraform logs:
   ```bash
   export TF_LOG=DEBUG
   terraform plan
   ```
3. Verify Azure CLI authentication
4. Check resource quotas and limits
5. Review error messages in terraform.tfstate

## Contributing

1. Create a feature branch
2. Make changes and test
3. Update documentation
4. Submit pull request
5. Wait for review and approval

## Additional Resources

- [Azure Provider Documentation](https://registry.terraform.io/providers/hashicorp/azurerm/latest/docs)
- [Terraform Best Practices](https://www.terraform-best-practices.com/)
- [Azure Architecture Center](https://docs.microsoft.com/en-us/azure/architecture/)
