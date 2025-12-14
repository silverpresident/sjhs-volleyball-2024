# Role Loading Fix Documentation

## Problem Summary

User roles were not being loaded into authentication claims during login, causing role-based authorization to fail. This affected both password-based login and Google OAuth authentication.

### Root Cause

ASP.NET Core Identity does **not** automatically include user roles in the authentication claims by default. When users signed in (via password or OAuth), their roles were stored in the database (`AspNetUserRoles` table) but were not loaded into the `ClaimsPrincipal`, making them unavailable for:

- `[Authorize(Roles="...")]` attributes
- `User.IsInRole()` method calls
- Role-based authorization policies

## Solution Overview

The fix involves **four key changes**:

1. **Created a Custom Claims Principal Factory** - Explicitly loads roles from database into claims
2. **Registered the Custom Factory** - Ensures it's used for all authentication
3. **Fixed Google OAuth Role Assignment** - Assigns roles to database instead of transient claims
4. **Enhanced OAuth User Mapping** - Handles all scenarios (new users, existing users, linking)
5. **Added Role Refresh on Edit** - Immediate role changes without re-login

## Changes Made

### 1. Custom Claims Principal Factory

**File**: `src/VolleyballRallyManager.Lib/Configuration/CustomUserClaimsPrincipalFactory.cs` (NEW)

Created a custom factory that overrides `GenerateClaimsAsync()` to explicitly load user roles from the database and add them as role claims:

```csharp
public class CustomUserClaimsPrincipalFactory : UserClaimsPrincipalFactory<IdentityUser, IdentityRole>
{
    protected override async Task<ClaimsIdentity> GenerateClaimsAsync(IdentityUser user)
    {
        var identity = await base.GenerateClaimsAsync(user);
        var roles = await _userManager.GetRolesAsync(user);
        
        foreach (var role in roles)
        {
            identity.AddClaim(new Claim(ClaimTypes.Role, role));
        }
        
        return identity;
    }
}
```

**Why This Works**: This method is called every time a user signs in (password or OAuth), ensuring roles are always loaded into the authentication cookie/principal.

### 2. Register Custom Factory

**File**: `src/VolleyballRallyManager.Lib/Extensions/AuthenticationExtensions.cs`

**Changes**:
- Added `using VolleyballRallyManager.Lib.Configuration;`
- Added `using Microsoft.Extensions.Logging;`
- Registered the custom factory after Identity configuration:

```csharp
services.AddScoped<IUserClaimsPrincipalFactory<IdentityUser>, 
    CustomUserClaimsPrincipalFactory>();
```

### 3. Fixed Google OAuth Role Assignment

**File**: `src/VolleyballRallyManager.Lib/Extensions/AuthenticationExtensions.cs`

**Changed**: The `OnCreatingTicket` event handler from adding transient claims to assigning persistent database roles:

**Before** (didn't work for existing users):
```csharp
context.Identity?.AddClaim(new Claim(ClaimTypes.Role, "Administrator"));
```

**After** (works for all users):
```csharp
var user = await userManager.FindByEmailAsync(email);
if (!await userManager.IsInRoleAsync(user, "Administrator"))
{
    await userManager.AddToRoleAsync(user, "Administrator");
}
```

**Key Improvement**: Roles are now assigned to the database, and then loaded by the `CustomUserClaimsPrincipalFactory`.

### 4. Enhanced AccountController OAuth Handling

**File**: `src/VolleyballRallyManager.App/Controllers/AccountController.cs`

**Changes**:
- Added `IConfiguration` dependency injection
- Enhanced `ExternalLoginCallback()` method to handle three scenarios:
  1. **New Google OAuth user**: Creates account + assigns roles
  2. **Existing local user linking Google**: Links OAuth + assigns roles if missing
  3. **Existing OAuth user**: Roles loaded automatically by custom factory
- Added helper method `AssignRolesBasedOnConfigurationAsync()`:

```csharp
private async Task AssignRolesBasedOnConfigurationAsync(IdentityUser user, string email)
{
    // Reads configuration and assigns appropriate roles to the database
}
```

**Key Improvement**: All OAuth scenarios now result in database roles that are loaded into claims.

### 5. Added Role Refresh on Edit

**File**: `src/VolleyballRallyManager.App/Areas/Admin/Controllers/UserManagementController.cs`

**Changes**:
- Added `SignInManager<IdentityUser>` dependency injection
- Added refresh logic in `Edit()` POST method:

```csharp
var currentUserId = _userManager.GetUserId(User);
if (user.Id == currentUserId)
{
    await _signInManager.RefreshSignInAsync(user);
}
```

**Key Improvement**: When an admin edits their own roles, changes take effect immediately without requiring logout/login.

## How It Works Now

### Authentication Flow

```
User Login (Password or OAuth)
    ↓
SignInManager calls CustomUserClaimsPrincipalFactory
    ↓
GenerateClaimsAsync() method executed
    ↓
Loads roles from AspNetUserRoles table via UserManager.GetRolesAsync()
    ↓
Adds role claims to ClaimsIdentity
    ↓
Authentication cookie created with roles
    ↓
User.IsInRole() and [Authorize(Roles="")] work correctly
```

### Scenario Coverage

| Scenario | Before | After |
|----------|---------|-------|
| **Password login** | ❌ No roles | ✅ Roles loaded from database |
| **New Google OAuth user** | ⚠️ No roles assigned | ✅ Roles assigned to database + loaded into claims |
| **Existing user → First Google OAuth** | ⚠️ Links but no roles | ✅ Links + assigns roles if missing + loads into claims |
| **Returning OAuth user** | ❌ No roles | ✅ Roles loaded from database |
| **Role changes via admin** | ❌ Not reflected until re-login | ✅ Loaded from database on every login |
| **Editing own roles** | ❌ Requires logout/login | ✅ Immediately refreshed |

## Testing Checklist

After deploying these changes, verify:

- ✅ **Password Login**: Users with database roles can access protected areas
- ✅ **Google OAuth (New User)**: New OAuth users get roles from configuration
- ✅ **Google OAuth (Existing User)**: Existing local users can link OAuth and keep/get roles
- ✅ **Google OAuth (Returning)**: OAuth users' roles load correctly on subsequent logins
- ✅ **Role Edit (Other User)**: Editing another user's roles updates database
- ✅ **Role Edit (Self)**: Editing own roles takes effect immediately
- ✅ **Authorization Attributes**: `[Authorize(Roles="Administrator")]` works correctly
- ✅ **Role Checks**: `User.IsInRole("Administrator")` returns correct value
- ✅ **Navigation Menu**: Role-based menu items appear/hide correctly

## Configuration Requirements

Ensure `appsettings.json` includes:

```json
{
  "VolleyBallRallyManager": {
    "AdminEmail": "admin@example.com",
    "DefaultAdminEmails": [
      "admin1@example.com",
      "admin2@example.com"
    ],
    "DefaultJudgeEmails": [
      "judge1@example.com",
      "judge2@example.com"
    ],
    "DefaultScorekeeperEmails": [
      "scorer1@example.com",
      "scorer2@example.com"
    ]
  }
}
```

## Database Impact

No database schema changes required. The fix works with the existing Identity tables:

- `AspNetUsers` - User accounts
- `AspNetRoles` - Available roles
- `AspNetUserRoles` - User-role assignments
- `AspNetUserLogins` - External OAuth logins

## Files Modified

1. ✅ **NEW**: `src/VolleyballRallyManager.Lib/Configuration/CustomUserClaimsPrincipalFactory.cs`
2. ✅ **MODIFIED**: `src/VolleyballRallyManager.Lib/Extensions/AuthenticationExtensions.cs`
3. ✅ **MODIFIED**: `src/VolleyballRallyManager.App/Controllers/AccountController.cs`
4. ✅ **MODIFIED**: `src/VolleyballRallyManager.App/Areas/Admin/Controllers/UserManagementController.cs`
5. ✅ **NEW**: `docs/ROLE_LOADING_FIX.md` (this file)

## Technical Details

### Why Default Behavior Doesn't Load Roles

ASP.NET Core Identity separates role storage (database) from claim generation (authentication) for performance reasons. By default, `UserClaimsPrincipalFactory` only adds:
- User ID claim
- User name claim
- Email claim (if email is confirmed)
- Security stamp claim

Roles are intentionally **not** included unless you:
1. Override the claims principal factory (our solution)
2. Manually add role claims during authentication
3. Use a custom middleware to load roles

### Performance Considerations

- **Database Query**: One additional query per login to load roles (`UserManager.GetRolesAsync()`)
- **Cookie Size**: Slightly larger authentication cookies (role claims added)
- **Caching**: Roles are cached in the authentication cookie until it expires
- **Refresh Cost**: Role changes require cookie refresh (automatic on login, manual via `RefreshSignInAsync()`)

### Security Notes

- ✅ Roles are stored in database (persistent)
- ✅ Role claims are signed in authentication cookie (tamper-proof)
- ✅ Role changes are tracked via Identity's security stamp
- ✅ Token invalidation works correctly
- ✅ No additional attack surface introduced

## Troubleshooting

### Roles Still Not Loading

1. **Check Database**: Verify roles exist in `AspNetUserRoles` table
   ```sql
   SELECT u.Email, r.Name as Role
   FROM AspNetUsers u
   JOIN AspNetUserRoles ur ON u.Id = ur.UserId
   JOIN AspNetRoles r ON ur.RoleId = r.Id
   ```

2. **Clear Browser Cookies**: Old authentication cookies may be cached

3. **Check Custom Factory Registration**: Ensure it's registered in DI container

4. **Enable Logging**: Check logs for role assignment errors

### OAuth Users Have No Roles

1. **Check Configuration**: Verify email is in `appsettings.json` role lists
2. **Check Assignment Logic**: Review `AssignRolesBasedOnConfigurationAsync()` execution
3. **Check OnCreatingTicket**: Verify event handler is executing (add logging)

### Role Changes Not Taking Effect

1. **User Must Logout/Login**: Unless editing own roles (which auto-refreshes)
2. **Check Security Stamp**: Role changes should update security stamp
3. **Check Cookie Expiration**: Old cookies may still be valid

## Related Documentation

- [`docs/AUTHENTICATION_SETUP.md`](./AUTHENTICATION_SETUP.md) - Authentication configuration
- [`docs/USER_MANAGEMENT_GUIDE.md`](./USER_MANAGEMENT_GUIDE.md) - User and role management

---

**Version**: 1.0  
**Date**: December 2024  
**Author**: System  
