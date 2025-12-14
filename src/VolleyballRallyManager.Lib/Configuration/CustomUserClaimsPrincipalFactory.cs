using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using System.Security.Claims;

namespace VolleyballRallyManager.Lib.Configuration
{
    /// <summary>
    /// Custom claims principal factory that ensures user roles are loaded into claims
    /// during authentication. This is critical for role-based authorization to work properly.
    /// 
    /// By default, ASP.NET Core Identity does not automatically include user roles in the
    /// authentication claims. This factory explicitly loads roles from the database and
    /// adds them as role claims to the ClaimsPrincipal.
    /// </summary>
    public class CustomUserClaimsPrincipalFactory : UserClaimsPrincipalFactory<IdentityUser, IdentityRole>
    {
        private readonly UserManager<IdentityUser> _userManager;

        public CustomUserClaimsPrincipalFactory(
            UserManager<IdentityUser> userManager,
            RoleManager<IdentityRole> roleManager,
            IOptions<IdentityOptions> options)
            : base(userManager, roleManager, options)
        {
            _userManager = userManager;
        }

        /// <summary>
        /// Generate claims for the user, including role claims from the database.
        /// This method is called whenever a user signs in (password or OAuth).
        /// </summary>
        protected override async Task<ClaimsIdentity> GenerateClaimsAsync(IdentityUser user)
        {
            // Call base method to get standard claims (email, user ID, etc.)
            var identity = await base.GenerateClaimsAsync(user);
            
            // Explicitly load roles from database
            var roles = await _userManager.GetRolesAsync(user);
            
            // Add role claims to the identity
            // This ensures roles are available in User.IsInRole() and [Authorize(Roles="")] checks
            foreach (var role in roles)
            {
                identity.AddClaim(new Claim(ClaimTypes.Role, role));
            }

            return identity;
        }
    }
}
