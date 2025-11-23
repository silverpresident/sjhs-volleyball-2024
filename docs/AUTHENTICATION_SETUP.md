# Authentication Setup Guide

This guide explains how to set up and use the custom authentication system in the Volleyball Rally Manager application.

## Overview

The application uses a custom authentication system that supports:
- **Google OAuth** (primary authentication method)
- **Traditional username/password login** (secondary option)
- ASP.NET Core Identity for user management
- Role-based authorization (Administrator, Judge, Scorekeeper)

## Features

- ✅ Custom login page with modern UI design (green and gold theme)
- ✅ Google OAuth integration with "Sign in with Google" button
- ✅ Traditional username/password login as fallback
- ✅ Secure cookie-based authentication
- ✅ Anti-forgery token protection
- ✅ Account lockout after failed login attempts
- ✅ Return URL preservation for post-login redirects
- ✅ Custom Access Denied page
- ✅ Logout functionality

## Google OAuth Setup

### Step 1: Create a Google Cloud Project

1. Go to [Google Cloud Console](https://console.cloud.google.com/)
2. Create a new project or select an existing one
3. Name your project (e.g., "St Jago Volleyball Rally Manager")

### Step 2: Configure OAuth Consent Screen

1. Navigate to **APIs & Services** > **OAuth consent screen**
2. Select **External** user type (unless you have a Google Workspace account)
3. Fill in the required information:
   - **App name**: St Jago Volleyball Rally Manager
   - **User support email**: Your email address
   - **Developer contact information**: Your email address
4. Click **Save and Continue**
5. Skip the **Scopes** section (default scopes are sufficient)
6. Add test users if needed during development
7. Click **Save and Continue**

### Step 3: Create OAuth 2.0 Credentials

1. Navigate to **APIs & Services** > **Credentials**
2. Click **Create Credentials** > **OAuth 2.0 Client ID**
3. Select **Web application** as the application type
4. Configure the following:
   - **Name**: Volleyball Rally Manager Web Client
   - **Authorized JavaScript origins**:
     - `https://localhost:5001` (for development)
     - `https://yourdomain.com` (for production)
   - **Authorized redirect URIs**:
     - `https://localhost:5001/signin-google` (for development)
     - `https://yourdomain.com/signin-google` (for production)
5. Click **Create**
6. Copy the **Client ID** and **Client Secret** - you'll need these for configuration

### Step 4: Configure Application Settings

#### Development Environment

1. Open `src/VolleyballRallyManager.App/appsettings.json`
2. Update the Google authentication section:

```json
{
  "Authentication": {
    "Google": {
      "ClientId": "YOUR_CLIENT_ID_HERE.apps.googleusercontent.com",
      "ClientSecret": "YOUR_CLIENT_SECRET_HERE"
    }
  }
}
```

**⚠️ IMPORTANT**: Never commit real credentials to source control!

#### Production Environment

For production, store credentials in Azure Key Vault or environment variables:

**Using Azure Key Vault:**
```json
{
  "Authentication": {
    "Google": {
      "ClientId": "@Microsoft.KeyVault(SecretUri=https://your-vault.vault.azure.net/secrets/GoogleClientId/)",
      "ClientSecret": "@Microsoft.KeyVault(SecretUri=https://your-vault.vault.azure.net/secrets/GoogleClientSecret/)"
    }
  }
}
```

**Using Environment Variables:**
Set these environment variables in your Azure App Service:
- `Authentication__Google__ClientId`
- `Authentication__Google__ClientSecret`

## Database Setup

The authentication system requires database tables for user management. These are created automatically by Entity Framework migrations.

### Running Migrations

```bash
# From the project root
cd src/VolleyballRallyManager.App
dotnet ef database update
```

The following tables will be created:
- `AspNetUsers` - User accounts
- `AspNetRoles` - User roles
- `AspNetUserRoles` - User-role relationships
- `AspNetUserLogins` - External login providers (Google)
- `AspNetUserClaims` - User claims
- `AspNetUserTokens` - Authentication tokens

## User Roles

The system supports three roles:

1. **Administrator** - Full access to all features
2. **Judge** - Can manage matches and scores
3. **Scorekeeper** - Can update scores only

### Configuring Default Role Assignments

Edit `appsettings.json` to configure which email addresses get which roles:

```json
{
  "VolleyBallRallyManager": {
    "AdminEmail": "admin@example.com",
    "DefaultJudgeEmails": [
      "judge1@example.com",
      "judge2@example.com"
    ],
    "DefaultScorekeeperEmails": [
      "scorekeeper1@example.com",
      "scorekeeper2@example.com"
    ]
  }
}
```

These roles are automatically assigned during Google OAuth authentication.

## Usage

### Accessing the Login Page

Users can access the login page in several ways:
1. Click the **Login** link in the navigation bar
2. Navigate directly to `/Account/Login`
3. Attempt to access a protected resource (will redirect to login)

### Logging In

**Option 1: Google Sign-In (Recommended)**
1. Click the **Sign in with Google** button
2. Select your Google account
3. Grant permissions (first time only)
4. You'll be redirected to the application

**Option 2: Username/Password**
1. Enter your email address
2. Enter your password
3. Optionally check "Remember me"
4. Click **Sign In**

### Logging Out

1. Click your username in the navigation bar
2. Select **Logout** from the dropdown menu
3. You'll be redirected to the home page

### Access Denied

If you try to access a resource you don't have permission for:
1. You'll be redirected to `/Account/AccessDenied`
2. The page explains why access was denied
3. You can either go home or sign in with a different account

## Testing

### Testing Google OAuth (Development)

1. Ensure your `appsettings.json` has valid Google credentials
2. Start the application: `dotnet run`
3. Navigate to `https://localhost:5001/Account/Login`
4. Click **Sign in with Google**
5. Sign in with a Google account
6. Verify you're redirected back to the application

### Testing Traditional Login

To test username/password login, you'll need to create a user account first. You can do this:

1. **Via Seeding**: Add a user to `DatabaseInitialization.cs`
2. **Via API**: Create a registration endpoint (not included in this implementation)
3. **Directly in Database**: Use SQL to insert a user (for testing only)

Example SQL to create a test user:
```sql
-- This is for testing only - passwords should be created through the Identity API
-- Password: "Test123" (hashed)
INSERT INTO AspNetUsers (Id, UserName, NormalizedUserName, Email, NormalizedEmail, EmailConfirmed, PasswordHash, SecurityStamp, ConcurrencyStamp)
VALUES (
  NEWID(),
  'test@example.com',
  'TEST@EXAMPLE.COM',
  'test@example.com',
  'TEST@EXAMPLE.COM',
  1,
  'AQAAAAIAAYagAAAAEF2VzZzZzZzZzZzZzZzZzZzZzZzZzZzZzZzZzZzZzZzZzZzZzZzZzZzZzZzZzZzZw==',
  NEWID(),
  NEWID()
);
```

## Security Best Practices

### ✅ Implemented Security Features

- ✅ Anti-forgery tokens on all forms
- ✅ Secure cookie settings (HttpOnly, Secure, SameSite)
- ✅ Account lockout after 5 failed attempts
- ✅ Return URL validation (prevents open redirects)
- ✅ HTTPS enforcement
- ✅ Comprehensive error logging
- ✅ User-friendly error messages (no sensitive info leaked)

### 🔒 Additional Recommendations

1. **Enable HTTPS**: Always use HTTPS in production
2. **Rotate Secrets**: Regularly rotate OAuth credentials
3. **Monitor Logs**: Review authentication logs for suspicious activity
4. **Update Dependencies**: Keep NuGet packages up to date
5. **Configure CORS**: Restrict CORS to trusted origins only
6. **Enable 2FA**: Consider implementing two-factor authentication
7. **Password Policy**: Adjust password requirements in `AuthenticationExtensions.cs`

## Troubleshooting

### "Google ClientId not configured" Error

**Problem**: Application crashes on startup
**Solution**: Ensure `Authentication:Google:ClientId` is set in `appsettings.json`

### "Redirect URI mismatch" Error

**Problem**: OAuth error after clicking "Sign in with Google"
**Solution**: 
1. Check that redirect URI in Google Cloud Console matches exactly
2. Include protocol (https://) and port number if applicable
3. URI is case-sensitive

### "Error loading external login information"

**Problem**: OAuth callback fails
**Solution**:
1. Clear browser cookies
2. Verify OAuth consent screen is published
3. Check if test users are configured (for external user type)

### Users Can't Access Admin Area

**Problem**: Users get Access Denied
**Solution**:
1. Verify user email is in `AdminEmail` or role configuration
2. Check that roles are being assigned in `OnCreatingTicket` event
3. Ensure `[Authorize]` attributes are correct on controllers

### Database Connection Issues

**Problem**: Can't connect to database
**Solution**:
1. Verify connection string in `appsettings.json`
2. Ensure database server is running
3. Check that migrations have been applied: `dotnet ef database update`

## API Endpoints

The AccountController provides the following endpoints:

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/Account/Login` | Display login page |
| POST | `/Account/Login` | Process username/password login |
| POST | `/Account/ExternalLogin` | Initiate Google OAuth |
| GET | `/Account/ExternalLoginCallback` | Handle OAuth callback |
| POST | `/Account/Logout` | Sign out user |
| GET | `/Account/AccessDenied` | Display access denied page |

## Architecture

### Components

1. **AccountController** (`Controllers/AccountController.cs`)
   - Handles all authentication actions
   - Includes comprehensive error handling
   - Provides detailed logging

2. **AuthenticationExtensions** (`Lib/Extensions/AuthenticationExtensions.cs`)
   - Configures Identity and Google OAuth
   - Sets up authorization policies
   - Manages cookie settings

3. **LoginViewModel** (`Models/LoginViewModel.cs`)
   - Data model for login form
   - Includes validation attributes

4. **Views**
   - `Login.cshtml` - Custom login page
   - `AccessDenied.cshtml` - Access denied page
   - `_LoginPartial.cshtml` - Navigation bar login/logout UI

### Authentication Flow

```
┌─────────────┐
│   Browser   │
└──────┬──────┘
       │
       ├──── GET /Account/Login
       │
       ▼
┌─────────────────┐
│  Login Page     │
│  - Google OAuth │
│  - Username/Pwd │
└──────┬──────────┘
       │
       ├──── Option 1: Click "Sign in with Google"
       │     POST /Account/ExternalLogin
       │     ▼
       │     ┌───────────────────┐
       │     │  Google OAuth     │
       │     │  Authentication   │
       │     └─────────┬─────────┘
       │               │
       │               ▼
       │     GET /Account/ExternalLoginCallback
       │     - Create/find user
       │     - Assign roles
       │     - Sign in
       │
       ├──── Option 2: Enter credentials
       │     POST /Account/Login
       │     - Validate credentials
       │     - Check lockout
       │     - Sign in
       │
       ▼
┌─────────────────┐
│  Redirect to    │
│  Return URL or  │
│  Admin Home     │
└─────────────────┘
```

## Next Steps

After setting up authentication, consider:

1. **Registration**: Implement a user registration page
2. **Password Recovery**: Add forgot password functionality  
3. **Email Confirmation**: Enable email verification
4. **Two-Factor Authentication**: Add 2FA support
5. **User Management**: Create admin pages to manage users and roles
6. **Audit Logging**: Track authentication events

## Support

For issues or questions:
- Check the troubleshooting section above
- Review application logs in Azure Application Insights
- Contact your system administrator

---

**Last Updated**: November 2025
