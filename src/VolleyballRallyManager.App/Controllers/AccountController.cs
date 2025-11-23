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

        public AccountController(
            SignInManager<IdentityUser> signInManager,
            UserManager<IdentityUser> userManager,
            ILogger<AccountController> logger)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _logger = logger;
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
                // Attempt to sign in with the provided credentials
                var result = await _signInManager.PasswordSignInAsync(
                    model.Email,
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

                var user = await _userManager.FindByEmailAsync(email);
                if (user == null)
                {
                    // Create a new user
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

                // Add the external login to the user
                var addLoginResult = await _userManager.AddLoginAsync(user, info);
                if (!addLoginResult.Succeeded)
                {
                    _logger.LogError("Failed to add external login for {Email}: {Errors}",
                        email,
                        string.Join(", ", addLoginResult.Errors.Select(e => e.Description)));
                    TempData["ErrorMessage"] = "Failed to link external login to account.";
                    return RedirectToAction(nameof(Login), new { ReturnUrl = returnUrl });
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
