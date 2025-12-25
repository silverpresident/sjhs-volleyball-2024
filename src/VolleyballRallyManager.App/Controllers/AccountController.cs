using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using VolleyballRallyManager.App.Models;

namespace VolleyballRallyManager.App.Controllers
{
    /// <summary>
    /// Handles user authentication including traditional login and Google OAuth
    /// 
    /// To obtain Google OAuth credentials:
    /// 1. Go to Google Cloud Console: https://console.cloud.google.com/
    /// 2. Create a new project or select an existing one
    /// 3. Navigate to "APIs & Services" > "Credentials"
    /// 4. Click "Create Credentials" > "OAuth 2.0 Client ID"
    /// 5. Configure the OAuth consent screen if prompted
    /// 6. Select "Web application" as application type
    /// 7. Add authorized redirect URI: https://yourdomain.com/signin-google (or https://localhost:5001/signin-google for development)
    /// 8. Copy the Client ID and Client Secret to appsettings.json under Authentication:Google
    /// </summary>
    public class AccountController : Controller
    {
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ILogger<AccountController> _logger;
        private readonly IConfiguration _configuration;

        public AccountController(
            SignInManager<IdentityUser> signInManager,
            UserManager<IdentityUser> userManager,
            ILogger<AccountController> logger,
            IConfiguration configuration)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _logger = logger;
            _configuration = configuration;
        }

        /// <summary>
        /// Display the login page
        /// </summary>
        /// <param name="returnUrl">URL to redirect to after successful login</param>
        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login(string? returnUrl = null)
        {
            // Prevent already authenticated users from accessing login page
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToLocal(returnUrl);
            }

            ViewData["ReturnUrl"] = returnUrl;
            return View(new LoginViewModel { ReturnUrl = returnUrl });
        }

        /// <summary>
        /// Process traditional username/password login
        /// </summary>
        /// <param name="model">Login credentials</param>
        /// <param name="returnUrl">URL to redirect to after successful login</param>
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var userName = model.Email;
                if (userName.Contains('@'))
                {
                    var user = await _userManager.FindByEmailAsync(model.Email);
                    if (user == null)
                    {
                        ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                        return View(model);
                    }
                    else
                    {
                        userName = user.UserName;
                    }
                }
                // Attempt to sign in with the provided credentials
                var result = await _signInManager.PasswordSignInAsync(
                    userName,
                    model.Password,
                    model.RememberMe,
                    lockoutOnFailure: true);

                if (result.Succeeded)
                {
                    _logger.LogInformation("User {Email} logged in successfully", model.Email);
                    return RedirectToLocal(returnUrl);
                }

                if (result.RequiresTwoFactor)
                {
                    // Two-factor authentication would be handled here if enabled
                    ModelState.AddModelError(string.Empty, "Two-factor authentication is required.");
                    return View(model);
                }

                if (result.IsLockedOut)
                {
                    _logger.LogWarning("User {Email} account locked out", model.Email);
                    ModelState.AddModelError(string.Empty, "Your account has been locked due to multiple failed login attempts. Please try again later.");
                    return View(model);
                }

                // Invalid login attempt
                _logger.LogWarning("Invalid login attempt for user {Email}", model.Email);
                ModelState.AddModelError(string.Empty, "Invalid email or password.");
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login for user {Email}", model.Email);
                ModelState.AddModelError(string.Empty, "An error occurred during login. Please try again.");
                return View(model);
            }
        }

        /// <summary>
        /// Initiate external authentication (Google OAuth)
        /// </summary>
        /// <param name="provider">The authentication provider (e.g., "Google")</param>
        /// <param name="returnUrl">URL to redirect to after successful authentication</param>
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public IActionResult ExternalLogin(string provider, string? returnUrl = null)
        {
            // Request a redirect to the external login provider
            var redirectUrl = Url.Action(nameof(ExternalLoginCallback), "Account", new { returnUrl });
            var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
            return Challenge(properties, provider);
        }

        /// <summary>
        /// Handle the callback from external authentication provider (Google)
        /// </summary>
        /// <param name="returnUrl">URL to redirect to after successful authentication</param>
        /// <param name="remoteError">Error message from the external provider</param>
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> ExternalLoginCallback(string? returnUrl = null, string? remoteError = null)
        {
            returnUrl ??= Url.Content("~/");

            if (remoteError != null)
            {
                _logger.LogError("Error from external provider: {Error}", remoteError);
                TempData["ErrorMessage"] = $"Error from external provider: {remoteError}";
                return RedirectToAction(nameof(Login), new { ReturnUrl = returnUrl });
            }

            try
            {
                // Get the information about the user from the external login provider
                var info = await _signInManager.GetExternalLoginInfoAsync();
                if (info == null)
                {
                    _logger.LogError("Failed to load external login information");
                    TempData["ErrorMessage"] = "Error loading external login information.";
                    return RedirectToAction(nameof(Login), new { ReturnUrl = returnUrl });
                }

                // Sign in the user with this external login provider if the user already has a login
                var result = await _signInManager.ExternalLoginSignInAsync(
                    info.LoginProvider,
                    info.ProviderKey,
                    isPersistent: false,
                    bypassTwoFactor: true);

                if (result.Succeeded)
                {
                    _logger.LogInformation("User logged in with {Provider} provider", info.LoginProvider);
                    return RedirectToLocal(returnUrl);
                }

                if (result.IsLockedOut)
                {
                    _logger.LogWarning("User account locked out");
                    return RedirectToAction(nameof(AccessDenied));
                }

                // If the user does not have an account, create one
                var email = info.Principal.FindFirstValue(ClaimTypes.Email);
                if (string.IsNullOrEmpty(email))
                {
                    _logger.LogError("Email claim not received from {Provider}", info.LoginProvider);
                    TempData["ErrorMessage"] = "Email information not received from the provider.";
                    return RedirectToAction(nameof(Login), new { ReturnUrl = returnUrl });
                }

                // Try to find existing user by email
                var user = await _userManager.FindByEmailAsync(email);
                bool isNewUser = false;

                if (user == null)
                {
                    // Create a new user
                    isNewUser = true;
                    user = new IdentityUser
                    {
                        UserName = email,
                        Email = email,
                        EmailConfirmed = true // Auto-confirm since it's from Google
                    };

                    var createResult = await _userManager.CreateAsync(user);
                    if (!createResult.Succeeded)
                    {
                        _logger.LogError("Failed to create user account for {Email}: {Errors}",
                            email,
                            string.Join(", ", createResult.Errors.Select(e => e.Description)));
                        TempData["ErrorMessage"] = "Failed to create user account.";
                        return RedirectToAction(nameof(Login), new { ReturnUrl = returnUrl });
                    }

                    _logger.LogInformation("Created new user account for {Email}", email);
                }
                else
                {
                    _logger.LogInformation("Found existing user account for {Email}, linking Google login", email);
                }

                // Add the external login to the user (if not already linked)
                var existingLogins = await _userManager.GetLoginsAsync(user);
                if (!existingLogins.Any(l => l.LoginProvider == info.LoginProvider && l.ProviderKey == info.ProviderKey))
                {
                    var addLoginResult = await _userManager.AddLoginAsync(user, info);
                    if (!addLoginResult.Succeeded)
                    {
                        _logger.LogError("Failed to add external login for {Email}: {Errors}",
                            email,
                            string.Join(", ", addLoginResult.Errors.Select(e => e.Description)));
                        TempData["ErrorMessage"] = "Failed to link external login to account.";
                        return RedirectToAction(nameof(Login), new { ReturnUrl = returnUrl });
                    }
                    _logger.LogInformation("Linked Google login to user {Email}", email);
                }

                // Assign roles based on configuration (for new users OR existing users without roles)
                var userRoles = await _userManager.GetRolesAsync(user);
                if (isNewUser || !userRoles.Any())
                {
                    await AssignRolesBasedOnConfigurationAsync(user, email);
                }

                // Sign in the user
                await _signInManager.SignInAsync(user, isPersistent: false, info.LoginProvider);
                _logger.LogInformation("User {Email} signed in with {Provider} provider", email, info.LoginProvider);

                return RedirectToLocal(returnUrl);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during external login callback");
                TempData["ErrorMessage"] = "An error occurred during authentication. Please try again.";
                return RedirectToAction(nameof(Login), new { ReturnUrl = returnUrl });
            }
        }

        /// <summary>
        /// Sign out the current user
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            try
            {
                await _signInManager.SignOutAsync();
                _logger.LogInformation("User logged out");
                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during logout");
                return RedirectToAction("Index", "Home");
            }
        }

        /// <summary>
        /// Display access denied page when user lacks required authorization
        /// </summary>
        [HttpGet]
        [AllowAnonymous]
        public IActionResult AccessDenied()
        {
            return View();
        }

        /// <summary>
        /// Display the change password page
        /// </summary>
        [HttpGet]
        [Authorize]
        public IActionResult ChangePassword()
        {
            return View(new ChangePasswordViewModel());
        }

        /// <summary>
        /// Process password change request
        /// </summary>
        /// <param name="model">Password change information</param>
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    _logger.LogError("Unable to load user for password change");
                    return RedirectToAction(nameof(Login));
                }

                // Check if user has a password (users who signed up via OAuth may not)
                var hasPassword = await _userManager.HasPasswordAsync(user);
                
                if (!hasPassword)
                {
                    // If user doesn't have a password, add one (first-time password setup)
                    var addPasswordResult = await _userManager.AddPasswordAsync(user, model.NewPassword);
                    if (addPasswordResult.Succeeded)
                    {
                        _logger.LogInformation("User {Email} successfully set their password", user.Email);
                        await _signInManager.RefreshSignInAsync(user);
                        TempData["SuccessMessage"] = "Your password has been set successfully.";
                        return RedirectToAction("Index", "Home", new { area = "Admin" });
                    }
                    
                    foreach (var error in addPasswordResult.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                    return View(model);
                }

                // User has a password, so change it
                var changePasswordResult = await _userManager.ChangePasswordAsync(
                    user,
                    model.CurrentPassword,
                    model.NewPassword);

                if (changePasswordResult.Succeeded)
                {
                    _logger.LogInformation("User {Email} successfully changed their password", user.Email);
                    await _signInManager.RefreshSignInAsync(user);
                    TempData["SuccessMessage"] = "Your password has been changed successfully.";
                    return RedirectToAction("Index", "Home", new { area = "Admin" });
                }

                foreach (var error in changePasswordResult.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during password change for user");
                ModelState.AddModelError(string.Empty, "An error occurred while changing your password. Please try again.");
                return View(model);
            }
        }

        /// <summary>
        /// Assign roles to user based on email configuration
        /// </summary>
        private async Task AssignRolesBasedOnConfigurationAsync(IdentityUser user, string email)
        {
            try
            {
                var adminEmails = _configuration.GetSection("VolleyBallRallyManager:DefaultAdminEmails").Get<string[]>();
                var judgeEmails = _configuration.GetSection("VolleyBallRallyManager:DefaultJudgeEmails").Get<string[]>();
                var scorekeeperEmails = _configuration.GetSection("VolleyBallRallyManager:DefaultScorekeeperEmails").Get<string[]>();

                if (adminEmails?.Contains(email, StringComparer.InvariantCultureIgnoreCase) == true)
                {
                    if (!await _userManager.IsInRoleAsync(user, "Administrator"))
                    {
                        await _userManager.AddToRoleAsync(user, "Administrator");
                        _logger.LogInformation("Assigned Administrator role to user {Email}", email);
                    }
                }
                else if (judgeEmails?.Contains(email, StringComparer.InvariantCultureIgnoreCase) == true)
                {
                    if (!await _userManager.IsInRoleAsync(user, "Judge"))
                    {
                        await _userManager.AddToRoleAsync(user, "Judge");
                        _logger.LogInformation("Assigned Judge role to user {Email}", email);
                    }
                }
                else if (scorekeeperEmails?.Contains(email, StringComparer.InvariantCultureIgnoreCase) == true)
                {
                    if (!await _userManager.IsInRoleAsync(user, "Scorekeeper"))
                    {
                        await _userManager.AddToRoleAsync(user, "Scorekeeper");
                        _logger.LogInformation("Assigned Scorekeeper role to user {Email}", email);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error assigning roles to user {Email}", email);
            }
        }

        /// <summary>
        /// Safely redirect to local URL, preventing open redirect vulnerabilities
        /// </summary>
        /// <param name="returnUrl">The URL to redirect to</param>
        private IActionResult RedirectToLocal(string? returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            else
            {
                return RedirectToAction("Index", "Home", new { area = "Admin" });
            }
        }
    }
}
