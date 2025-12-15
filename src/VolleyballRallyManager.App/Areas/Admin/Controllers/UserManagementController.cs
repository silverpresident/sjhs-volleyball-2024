using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VolleyballRallyManager.App.Areas.Admin.Models;

namespace VolleyballRallyManager.App.Areas.Admin.Controllers
{
    /// <summary>
    /// Controller for managing user accounts and roles
    /// </summary>
    [Area("Admin")]
    [Authorize]
    public class UserManagementController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly ILogger<UserManagementController> _logger;

        public UserManagementController(
            UserManager<IdentityUser> userManager,
            RoleManager<IdentityRole> roleManager,
            SignInManager<IdentityUser> signInManager,
            ILogger<UserManagementController> logger)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _signInManager = signInManager;
            _logger = logger;
        }

        /// <summary>
        /// Display list of all users (available to all authenticated users)
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Index(string? searchTerm, string? roleFilter)
        {
            try
            {
                var users = await _userManager.Users.ToListAsync();
                var userViewModels = new List<UserViewModel>();

                foreach (var user in users)
                {
                    var roles = await _userManager.GetRolesAsync(user);
                    
                    // Apply search filter
                    if (!string.IsNullOrWhiteSpace(searchTerm) &&
                        !user.Email!.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) &&
                        !user.UserName!.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    // Apply role filter
                    if (!string.IsNullOrWhiteSpace(roleFilter) && !roles.Contains(roleFilter))
                    {
                        continue;
                    }

                    userViewModels.Add(new UserViewModel
                    {
                        Id = user.Id,
                        Email = user.Email ?? string.Empty,
                        DisplayName = user.UserName ?? string.Empty,
                        EmailConfirmed = user.EmailConfirmed,
                        IsEnabled = !user.LockoutEnabled || user.LockoutEnd == null || user.LockoutEnd <= DateTimeOffset.UtcNow,
                        LockoutEnabled = user.LockoutEnabled,
                        LockoutEnd = user.LockoutEnd,
                        Roles = roles.ToList()
                    });
                }

                var allRoles = await _roleManager.Roles.Select(r => r.Name!).ToListAsync();

                var viewModel = new UserListViewModel
                {
                    Users = userViewModels.OrderBy(u => u.Email).ToList(),
                    SearchTerm = searchTerm,
                    RoleFilter = roleFilter,
                    AvailableRoles = allRoles
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading user list");
                TempData["ErrorMessage"] = "An error occurred while loading users.";
                return View(new UserListViewModel());
            }
        }

        /// <summary>
        /// Display form to create a new user (admin only)
        /// </summary>
        [HttpGet]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Create()
        {
            try
            {
                var model = new CreateUserViewModel();
                ViewBag.AvailableRoles = await _roleManager.Roles.Select(r => r.Name!).ToListAsync();
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading create user form");
                TempData["ErrorMessage"] = "An error occurred while loading the form.";
                return RedirectToAction(nameof(Index));
            }
        }

        /// <summary>
        /// Process new user creation (admin only)
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Administrator")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateUserViewModel model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.AvailableRoles = await _roleManager.Roles.Select(r => r.Name!).ToListAsync();
                return View(model);
            }

            try
            {
                // Check if user already exists
                var existingUser = await _userManager.FindByEmailAsync(model.Email);
                if (existingUser != null)
                {
                    ModelState.AddModelError(nameof(model.Email), "A user with this email already exists.");
                    ViewBag.AvailableRoles = await _roleManager.Roles.Select(r => r.Name!).ToListAsync();
                    return View(model);
                }

                // Create new user
                var user = new IdentityUser
                {
                    UserName = model.Email,
                    Email = model.Email,
                    EmailConfirmed = true // Auto-confirm for admin-created users
                };

                // Generate a secure random password
                var password = GenerateSecurePassword();

                var result = await _userManager.CreateAsync(user, password);

                if (!result.Succeeded)
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                    ViewBag.AvailableRoles = await _roleManager.Roles.Select(r => r.Name!).ToListAsync();
                    return View(model);
                }

                // Assign roles if specified
                if (model.SelectedRoles != null && model.SelectedRoles.Any())
                {
                    var roleResult = await _userManager.AddToRolesAsync(user, model.SelectedRoles);
                    if (!roleResult.Succeeded)
                    {
                        _logger.LogWarning("Failed to assign roles to user {Email}: {Errors}",
                            model.Email,
                            string.Join(", ", roleResult.Errors.Select(e => e.Description)));
                    }
                }

                _logger.LogInformation("User {Email} created by {AdminUser}", model.Email, User.Identity!.Name);

                TempData["SuccessMessage"] = $"User {model.Email} created successfully. A temporary password has been generated.";
                
                // In a real application, you would send this password via email
                TempData["TempPassword"] = password;
                
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating user {Email}", model.Email);
                ModelState.AddModelError(string.Empty, "An error occurred while creating the user.");
                ViewBag.AvailableRoles = await _roleManager.Roles.Select(r => r.Name!).ToListAsync();
                return View(model);
            }
        }

        /// <summary>
        /// Display form to edit a user (admin only)
        /// </summary>
        [HttpGet]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Edit(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            try
            {
                var user = await _userManager.FindByIdAsync(id);
                if (user == null)
                {
                    return NotFound();
                }

                var roles = await _userManager.GetRolesAsync(user);
                var allRoles = await _roleManager.Roles.Select(r => r.Name!).ToListAsync();

                var model = new EditUserViewModel
                {
                    Id = user.Id,
                    Email = user.Email ?? string.Empty,
                    DisplayName = user.UserName ?? string.Empty,
                    EmailConfirmed = user.EmailConfirmed,
                    IsEnabled = !user.LockoutEnabled || user.LockoutEnd == null || user.LockoutEnd <= DateTimeOffset.UtcNow,
                    SelectedRoles = roles.ToList(),
                    AvailableRoles = allRoles
                };

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading user {UserId} for editing", id);
                TempData["ErrorMessage"] = "An error occurred while loading the user.";
                return RedirectToAction(nameof(Index));
            }
        }

        /// <summary>
        /// Process user updates (admin only)
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Administrator")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(EditUserViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.AvailableRoles = await _roleManager.Roles.Select(r => r.Name!).ToListAsync();
                return View(model);
            }

            try
            {
                var user = await _userManager.FindByIdAsync(model.Id);
                if (user == null)
                {
                    return NotFound();
                }

                // Update user properties
                user.Email = model.Email;
                user.UserName = model.Email;
                user.EmailConfirmed = model.EmailConfirmed;

                // Handle account enable/disable
                if (model.IsEnabled)
                {
                    // Enable account by setting lockout end to null
                    await _userManager.SetLockoutEndDateAsync(user, null);
                }
                else
                {
                    // Disable account by setting lockout end far in the future
                    await _userManager.SetLockoutEndDateAsync(user, DateTimeOffset.MaxValue);
                }

                var updateResult = await _userManager.UpdateAsync(user);
                if (!updateResult.Succeeded)
                {
                    foreach (var error in updateResult.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                    model.AvailableRoles = await _roleManager.Roles.Select(r => r.Name!).ToListAsync();
                    return View(model);
                }

                // Update roles
                var currentRoles = await _userManager.GetRolesAsync(user);
                var rolesToAdd = model.SelectedRoles.Except(currentRoles).ToList();
                var rolesToRemove = currentRoles.Except(model.SelectedRoles).ToList();

                if (rolesToRemove.Any())
                {
                    await _userManager.RemoveFromRolesAsync(user, rolesToRemove);
                }

                if (rolesToAdd.Any())
                {
                    await _userManager.AddToRolesAsync(user, rolesToAdd);
                }

                // Refresh the authentication cookie if the edited user is the current user
                // This ensures role changes take effect immediately without requiring logout/login
                var currentUserId = _userManager.GetUserId(User);
                if (user.Id == currentUserId)
                {
                    await _signInManager.RefreshSignInAsync(user);
                    _logger.LogInformation("Refreshed sign-in for current user after role changes");
                }

                _logger.LogInformation("User {Email} updated by {AdminUser}", model.Email, User.Identity!.Name);

                TempData["SuccessMessage"] = $"User {model.Email} updated successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user {UserId}", model.Id);
                ModelState.AddModelError(string.Empty, "An error occurred while updating the user.");
                model.AvailableRoles = await _roleManager.Roles.Select(r => r.Name!).ToListAsync();
                return View(model);
            }
        }

        /// <summary>
        /// Enable a user account (admin only)
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Administrator")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Enable(string id)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(id);
                if (user == null)
                {
                    return NotFound();
                }

                // Enable account by clearing lockout
                await _userManager.SetLockoutEndDateAsync(user, null);
                
                _logger.LogInformation("User {Email} enabled by {AdminUser}", user.Email, User.Identity!.Name);
                
                TempData["SuccessMessage"] = $"User {user.Email} has been enabled.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error enabling user {UserId}", id);
                TempData["ErrorMessage"] = "An error occurred while enabling the user.";
                return RedirectToAction(nameof(Index));
            }
        }

        /// <summary>
        /// Disable a user account (admin only)
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Administrator")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Disable(string id)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(id);
                if (user == null)
                {
                    return NotFound();
                }

                // Prevent disabling your own account
                if (user.Id == _userManager.GetUserId(User))
                {
                    TempData["ErrorMessage"] = "You cannot disable your own account.";
                    return RedirectToAction(nameof(Index));
                }

                // Disable account by setting lockout end far in the future
                await _userManager.SetLockoutEndDateAsync(user, DateTimeOffset.MaxValue);
                
                _logger.LogInformation("User {Email} disabled by {AdminUser}", user.Email, User.Identity!.Name);
                
                TempData["SuccessMessage"] = $"User {user.Email} has been disabled.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error disabling user {UserId}", id);
                TempData["ErrorMessage"] = "An error occurred while disabling the user.";
                return RedirectToAction(nameof(Index));
            }
        }

        /// <summary>
        /// Display current logged-in user information for debugging (available to all authenticated users)
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> CurrentUser()
        {
            try
            {
                var userId = _userManager.GetUserId(User);
                if (string.IsNullOrEmpty(userId))
                {
                    _logger.LogWarning("CurrentUser called but no user ID found");
                    TempData["ErrorMessage"] = "Unable to retrieve current user information.";
                    return RedirectToAction("Index", "Home", new { area = "Admin" });
                }

                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    _logger.LogWarning("User {UserId} not found in database", userId);
                    TempData["ErrorMessage"] = "User not found in database.";
                    return RedirectToAction("Index", "Home", new { area = "Admin" });
                }

                // Get user roles from UserManager (database)
                var roles = await _userManager.GetRolesAsync(user);

                // Get user claims from UserManager (database)
                var claims = await _userManager.GetClaimsAsync(user);
                var claimsDictionary = claims.ToDictionary(c => c.Type, c => c.Value);

                // Get session claims from ClaimsPrincipal (current request/session)
                var sessionClaims = new Dictionary<string, List<string>>();
                foreach (var claim in User.Claims)
                {
                    if (!sessionClaims.ContainsKey(claim.Type))
                    {
                        sessionClaims[claim.Type] = new List<string>();
                    }
                    sessionClaims[claim.Type].Add(claim.Value);
                }

                // Get all identities from ClaimsPrincipal
                var identities = new List<IdentityInfo>();
                if (User.Identities != null)
                {
                    foreach (var identity in User.Identities)
                    {
                        var identityClaims = new Dictionary<string, string>();
                        foreach (var claim in identity.Claims)
                        {
                            // Use a key that includes index if duplicate claim types exist
                            var key = claim.Type;
                            var counter = 1;
                            while (identityClaims.ContainsKey(key))
                            {
                                key = $"{claim.Type} [{counter}]";
                                counter++;
                            }
                            identityClaims[key] = claim.Value;
                        }

                        identities.Add(new IdentityInfo
                        {
                            Name = identity.Name,
                            AuthenticationType = identity.AuthenticationType,
                            IsAuthenticated = identity.IsAuthenticated,
                            Label = identity.Label,
                            Claims = identityClaims
                        });
                    }
                }

                var viewModel = new CurrentUserViewModel
                {
                    // UserManager data (from database)
                    Id = user.Id,
                    UserName = user.UserName ?? string.Empty,
                    Email = user.Email ?? string.Empty,
                    EmailConfirmed = user.EmailConfirmed,
                    PhoneNumber = user.PhoneNumber,
                    PhoneNumberConfirmed = user.PhoneNumberConfirmed,
                    TwoFactorEnabled = user.TwoFactorEnabled,
                    LockoutEnabled = user.LockoutEnabled,
                    LockoutEnd = user.LockoutEnd,
                    AccessFailedCount = user.AccessFailedCount,
                    Roles = roles.ToList(),
                    Claims = claimsDictionary,
                    
                    // Session/Identity data (from ClaimsPrincipal)
                    IsAuthenticated = User.Identity?.IsAuthenticated ?? false,
                    AuthenticationType = User.Identity?.AuthenticationType,
                    IdentityName = User.Identity?.Name,
                    SessionClaims = sessionClaims,
                    IdentitiesCount = identities.Count,
                    Identities = identities
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading current user information");
                TempData["ErrorMessage"] = "An error occurred while loading user information.";
                return RedirectToAction("Index", "Home", new { area = "Admin" });
            }
        }

        /// <summary>
        /// Generate a secure random password
        /// </summary>
        private string GenerateSecurePassword()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789!@#$%^&*";
            var random = new Random();
            var password = new char[16];
            
            for (int i = 0; i < password.Length; i++)
            {
                password[i] = chars[random.Next(chars.Length)];
            }
            password[4] = random.Next(0, 9).ToString()[0]; //password must contain at least 1 digit
            return new string(password);
        }
    }
}
