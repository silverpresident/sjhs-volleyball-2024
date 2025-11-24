# User Management System Guide

## Overview

The User Management system provides administrators with comprehensive tools to manage user accounts and role assignments in the Volleyball Rally Manager application.

## Features

### ✅ Implemented Features

1. **User List (Available to All Authenticated Users)**
   - View all users in the system
   - See user roles and status
   - Search users by email or name
   - Filter users by role
   - View account status (enabled/disabled)
   - Check email confirmation status

2. **Add New Users (Admin Only)**
   - Create user accounts with email and display name
   - Assign multiple roles during creation
   - Auto-generate secure temporary passwords
   - Automatic email confirmation
   - Password displayed to admin after creation

3. **Edit Users (Admin Only)**
   - Update email addresses
   - Modify display names
   - Enable/disable accounts
   - Manage email confirmation status
   - Add or remove user roles
   - Quick actions for enable/disable

4. **Enable/Disable Users (Admin Only)**
   - Quick enable/disable buttons
   - Prevents self-disabling
   - Immediate effect on login ability
   - Confirmation dialogs for safety

## Access Control

### Permissions

| Action | Required Role | Notes |
|--------|--------------|-------|
| View User List | Any Authenticated User | Read-only access |
| Add User | Administrator | Full creation privileges |
| Edit User | Administrator | Can modify all user properties |
| Enable User | Administrator | Can enable any disabled account |
| Disable User | Administrator | Cannot disable own account |

## User Roles

### Available Roles

1. **Administrator**
   - Full system access
   - Can manage users
   - Can manage tournaments
   - Can manage all features

2. **Judge**
   - Can manage matches and scores
   - Can view tournaments
   - Limited administrative access

3. **Scorekeeper**
   - Can update match scores only
   - Read-only access to other features
   - Limited permissions

### Role Assignment

- Users can have multiple roles simultaneously
- Roles can be assigned during user creation
- Roles can be modified by editing the user
- Role changes take effect immediately
- All role assignments are logged

## Usage Guide

### Viewing Users

1. Navigate to **Admin > User Management**
2. View the list of all users
3. Use search box to find specific users
4. Use role filter to show users with specific roles
5. Click **Reset** to clear filters

### Creating a New User

1. Navigate to **Admin > User Management**
2. Click **Add New User** button
3. Enter required information:
   - **Email Address**: Will be used as username
   - **Display Name**: How the user appears in the system
4. Select roles to assign (optional)
5. Click **Create User**
6. Copy the temporary password shown (share securely with user)
7. User should change password on first login

**Important Notes:**
- A secure 16-character password is auto-generated
- Email is automatically confirmed
- Username is set to the email address
- Password is only shown once after creation

### Editing a User

1. Navigate to **Admin > User Management**
2. Find the user in the list
3. Click the **Edit** (pencil icon) button
4. Modify any of the following:
   - Email address (updates username)
   - Display name
   - Account enabled/disabled status
   - Email confirmed status
   - Assigned roles
5. Click **Save Changes**

**Important Notes:**
- Changing email updates the username
- Disabling an account prevents login immediately
- You cannot disable your own account
- Role changes are effective immediately

### Enabling/Disabling Users

**From User List:**
1. Find the user in the list
2. Click the **Disable** (ban icon) or **Enable** (check icon) button
3. Confirm the action

**From Edit Page:**
1. Open the user for editing
2. Use the quick action button at the bottom
3. Confirm the action

OR

1. Toggle the "Account Enabled" switch in the form
2. Click **Save Changes**

## User Account States

### Enabled Account
- User can log in normally
- All permissions are active
- Indicated by green "Enabled" badge

### Disabled Account
- User cannot log in
- Sessions are terminated
- Indicated by red "Disabled" badge
- Can be re-enabled by administrator

### Email Confirmed vs. Unconfirmed
- **Confirmed** (✓): Email has been verified
- **Unconfirmed** (✗): Email not verified
- Admin-created users are auto-confirmed
- Can be manually toggled by administrators

## Security Considerations

### Password Management

1. **Auto-Generated Passwords**
   - 16 characters long
   - Contains uppercase, lowercase, numbers, and symbols
   - Cryptographically secure random generation
   - Displayed only once after creation

2. **Password Sharing**
   - Share passwords securely (encrypted email, secure messaging)
   - Users should change password on first login
   - Consider implementing password reset flow

### Account Protection

1. **Self-Disable Prevention**
   - Administrators cannot disable their own accounts
   - Prevents accidental lockouts
   - Other administrators can disable accounts if needed

2. **Audit Logging**
   - All user management actions are logged
   - Includes: create, edit, enable, disable actions
   - Logs include administrator who performed action
   - Check application logs for audit trail

3. **Role-Based Access**
   - Only administrators can manage users
   - Standard users can only view user list
   - All actions require authentication
   - Anti-forgery tokens protect all forms

## Search and Filtering

### Search Functionality
- Searches both email and display name fields
- Case-insensitive search
- Partial matching supported
- Real-time results

### Role Filtering
- Filter by any available role
- Shows only users with selected role
- Can combine with search
- "All Roles" option shows everyone

### Reset Filters
- Click "Reset" button to clear all filters
- Returns to full user list
- Useful for starting fresh search

## User Interface

### User List Page

```
┌─────────────────────────────────────────────────┐
│  User Management                    [Add User]  │
├─────────────────────────────────────────────────┤
│  Search: [____________] Role: [____] [Search]   │
├─────────────────────────────────────────────────┤
│  Email  │ Name │ Roles │ Status │ Confirmed │ Actions │
│  ─────────────────────────────────────────────  │
│  user@  │ John │ Admin │ ✓      │ ✓         │ [Edit][Disable] │
│  ...    │ ...  │ ...   │ ...    │ ...       │ ...  │
└─────────────────────────────────────────────────┘
```

### Color Coding

- **Green**: Success states, enabled accounts
- **Red**: Disabled accounts, errors
- **Blue**: Role badges, information
- **Yellow**: Warning states

## API Endpoints

The UserManagementController provides these endpoints:

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/Admin/UserManagement` | List all users |
| GET | `/Admin/UserManagement/Create` | Show create form |
| POST | `/Admin/UserManagement/Create` | Process user creation |
| GET | `/Admin/UserManagement/Edit/{id}` | Show edit form |
| POST | `/Admin/UserManagement/Edit` | Process user updates |
| POST | `/Admin/UserManagement/Enable/{id}` | Enable user account |
| POST | `/Admin/UserManagement/Disable/{id}` | Disable user account |

## Best Practices

### For Administrators

1. **User Creation**
   - Use descriptive display names
   - Assign appropriate roles from start
   - Share passwords securely
   - Document role assignments

2. **Role Management**
   - Follow principle of least privilege
   - Review role assignments regularly
   - Remove unnecessary roles promptly
   - Document role changes

3. **Account Management**
   - Disable accounts when users leave
   - Don't delete accounts (for audit trail)
   - Re-enable accounts when users return
   - Keep email addresses up to date

4. **Security**
   - Regularly review user list
   - Check for unused accounts
   - Monitor login activity
   - Report suspicious activity

### For Users

1. **Password Security**
   - Change temporary password immediately
   - Use strong, unique passwords
   - Don't share passwords
   - Report lost passwords to admin

2. **Account Information**
   - Keep email address current
   - Report access issues promptly
   - Use appropriate roles only
   - Log out when finished

## Troubleshooting

### Cannot Create User

**Problem**: Error when creating new user

**Solutions:**
1. Check if email already exists
2. Ensure email format is valid
3. Verify display name length (2-100 characters)
4. Check if roles are available
5. Review application logs

### Cannot Disable User

**Problem**: Disable button doesn't work

**Solutions:**
1. Check if you're trying to disable your own account
2. Verify you have Administrator role
3. Ensure account is currently enabled
4. Check for any error messages

### User Cannot Log In

**Problem**: User reports cannot log in

**Solutions:**
1. Check if account is enabled
2. Verify email address is correct
3. Check if email is confirmed
4. Reset password if needed
5. Review login logs

### Roles Not Working

**Problem**: User has role but no permissions

**Solutions:**
1. Verify role is correctly assigned
2. Check authorization policies
3. Have user log out and back in
4. Review role configuration
5. Check application logs

## Technical Details

### Database Schema

The system uses ASP.NET Core Identity tables:

- **AspNetUsers**: User accounts
- **AspNetRoles**: Available roles
- **AspNetUserRoles**: User-role assignments
- **AspNetUserLogins**: External logins (Google OAuth)

### ViewModels

1. **UserViewModel**: Display user information
2. **CreateUserViewModel**: Create new user form
3. **EditUserViewModel**: Edit user form
4. **UserListViewModel**: User list with filters

### Security Features

- Anti-forgery tokens on all forms
- Role-based authorization
- Confirmation dialogs for destructive actions
- Comprehensive error handling
- Detailed logging

## Future Enhancements

Consider implementing:

1. **Password Reset Flow**
   - Self-service password reset
   - Email verification for reset
   - Temporary reset tokens

2. **User Registration**
   - Self-service registration
   - Email verification required
   - Admin approval workflow

3. **Advanced Filtering**
   - Filter by email confirmation
   - Filter by account status
   - Date range filters

4. **Bulk Operations**
   - Bulk role assignments
   - Bulk enable/disable
   - Export user list

5. **Activity Monitoring**
   - Last login tracking
   - Login history
   - Failed login attempts
   - Action audit log UI

## Support

For issues or questions:
- Review this documentation
- Check application logs
- Contact system administrator
- Report bugs through proper channels

---

**Version**: 1.0  
**Last Updated**: November 2025  
