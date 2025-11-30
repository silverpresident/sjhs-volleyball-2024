# ST JAGO VOLLEYBALL RALLY Manager

A comprehensive web application for managing a one-day volleyball competition, featuring real-time updates, administrative functions, and public display capabilities.

## Project Structure

The solution consists of three main projects:

1. **VolleyballRallyManager.Lib**
   - Core business logic
   - Entity models and database context
   - Services and interfaces
   - SignalR hubs
   - Shared configurations

2. **VolleyballRallyManager.App**
   - MVC application for tournament administration
   - User authentication and authorization
   - Match management
   - Team management
   - Bulletins
   - Real-time updates

3. **VolleyballRallyManager.Public**
   - Blazor WebAssembly application for public display
   - Real-time match schedule
   - Live score updates
   - Leaderboard
   - Bulletins feed
   - Update stream

## Prerequisites

- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Visual Studio Code](https://code.visualstudio.com/) or [Visual Studio 2022](https://visualstudio.microsoft.com/vs/)
- [SQL Server](https://www.microsoft.com/sql-server)
- [Azure Account](https://azure.microsoft.com/free/)
- [Azure CLI](https://docs.microsoft.com/cli/azure/install-azure-cli)
- [Terraform](https://www.terraform.io/downloads.html)

## Local Development Setup

1. Clone the repository:
   ```bash
   git clone https://github.com/yourusername/JAGO_VB-APP-2024.git
   cd JAGO_VB-APP-2024
   ```

2. Install required VS Code extensions:
   - Open VS Code
   - Open Command Palette (Ctrl+Shift+P)
   - Run "Extensions: Show Recommended Extensions"
   - Install all workspace recommended extensions

3. Create and configure appsettings.json files:
   ```bash
   # Admin project
   cp src/VolleyballRallyManager.App/appsettings.json.example src/VolleyballRallyManager.App/appsettings.json
   
   # Public project
   cp src/VolleyballRallyManager.Public/wwwroot/appsettings.json.example src/VolleyballRallyManager.Public/wwwroot/appsettings.json
   ```

4. Set up the database:
   ```bash
   # Create database and schema
   sqlcmd -S (localdb)\MSSQLLocalDB -i database/setup.sql
   
   # Apply Entity Framework migrations
   dotnet ef database update --project src/VolleyballRallyManager.Lib --startup-project src/VolleyballRallyManager.App
   ```

5. Configure authentication:
   - Set up [Google OAuth credentials](https://console.cloud.google.com/)
   - Set up [Microsoft OAuth credentials](https://portal.azure.com/#blade/Microsoft_AAD_RegisteredApps/ApplicationsListBlade)
   - Update appsettings.json with your credentials

6. Run the applications:
   ```bash
   # Run Admin app
   cd src/VolleyballRallyManager.App
   dotnet run
   
   # Run Public app
   cd src/VolleyballRallyManager.Public
   dotnet run
   ```

## Azure Deployment

1. Set up Azure resources using Terraform:
   ```bash
   cd terraform
   
   # Initialize Terraform
   terraform init
   
   # Plan the deployment
   terraform plan -out=tfplan
   
   # Apply the changes
   terraform apply tfplan
   ```

2. Configure Azure resources:
   - Set up Azure AD B2C for authentication
   - Configure connection strings
   - Set up SSL certificates
   - Configure SignalR service

3. Deploy the applications:
   ```bash
   # Deploy Admin app
   dotnet publish src/VolleyballRallyManager.App -c Release
   az webapp deployment source config-zip --src bin/Release/net8.0/publish.zip --name your-admin-app --resource-group your-resource-group
   
   # Deploy Public app
   dotnet publish src/VolleyballRallyManager.Public -c Release
   az webapp deployment source config-zip --src bin/Release/net8.0/publish.zip --name your-public-app --resource-group your-resource-group
   ```

## Required Azure Resources

1. **App Services**
   - Admin application (ASP.NET Core)
   - Public application (Blazor WebAssembly)

2. **Azure SQL Database**
   - Production database
   - Automated backups
   - Geo-replication (optional)

3. **Azure SignalR Service**
   - Real-time updates
   - WebSocket support

4. **Azure Key Vault**
   - Secrets management
   - SSL certificates

5. **Application Insights**
   - Application monitoring
   - Performance tracking
   - Error logging

6. **Azure AD B2C**
   - User authentication
   - External identity providers

## Configuration Settings

Key configuration settings required in appsettings.json:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "your-connection-string"
  },
  "Authentication": {
    "Google": {
      "ClientId": "your-client-id",
      "ClientSecret": "your-client-secret"
    },
    "Microsoft": {
      "ClientId": "your-client-id",
      "ClientSecret": "your-client-secret"
    }
  },
  "Azure": {
    "SignalR": {
      "ConnectionString": "your-signalr-connection-string"
    },
    "KeyVault": {
      "Url": "your-keyvault-url"
    }
  }
}
```

## Features

- User authentication with Google and Microsoft accounts
- Real-time match updates using SignalR
- **Announcer System** with priority-based queue management
  - Real-time announcement board with SignalR updates
  - Priority-based sequencing (Urgent, Info, Routine)
  - Automatic repeat handling and re-queuing
  - Call history and statistics tracking
  - Keyboard shortcuts for quick operation
- Markdown support for Bulletins
- Responsive design for all screen sizes
- Automatic page refresh every 2 minutes
- Team statistics and rankings
- Tournament progression tracking
- Administrative dashboard
- Match dispute handling
- Email notifications (optional)

## Database Schema

The database includes tables for:
- Teams
- Matches
- Rounds
- Bulletins
- Match Updates
- Divisions
- **Announcements** (with priority-based queue)
- **AnnouncementHistoryLogs** (call tracking)

See `database/setup.sql` and `database/announcements-setup.sql` for complete schema.

## Contributing

1. Fork the repository
2. Create a feature branch
3. Commit your changes
4. Push to the branch
5. Create a Pull Request

## License

This project is licensed under the MIT License - see the LICENSE file for details.

## Documentation

- [Announcer Feature Guide](docs/ANNOUNCER_FEATURE.md) - Complete guide to the announcement queue system
- [User Management Guide](docs/USER_MANAGEMENT_GUIDE.md) - User administration documentation
- [Scoring Channel](docs/SCORING_CHANNEL.md) - Match scoring system documentation
- [Authentication Setup](docs/AUTHENTICATION_SETUP.md) - OAuth configuration guide

## Support

For support, please contact the development team or create an issue in the repository.


## Covert SVG to ICO
cairosvg -f ico -o src/VolleyballRallyManager.App/wwwroot/favicon.ico src/VolleyballRallyManager.App/wwwroot/favicon.svg
