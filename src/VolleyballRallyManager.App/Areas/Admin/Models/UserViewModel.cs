using System.ComponentModel.DataAnnotations;

namespace VolleyballRallyManager.App.Areas.Admin.Models
{
    /// <summary>
    /// View model for displaying user information
    /// </summary>
    public class UserViewModel
    {
        public string Id { get; set; } = string.Empty;

        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [Display(Name = "Display Name")]
        public string DisplayName { get; set; } = string.Empty;

        [Display(Name = "Email Confirmed")]
        public bool EmailConfirmed { get; set; }

        [Display(Name = "Account Enabled")]
        public bool IsEnabled { get; set; }

        [Display(Name = "Lockout Enabled")]
        public bool LockoutEnabled { get; set; }

        [Display(Name = "Lockout End")]
        public DateTimeOffset? LockoutEnd { get; set; }

        [Display(Name = "Roles")]
        public List<string> Roles { get; set; } = new List<string>();

        [Display(Name = "Created Date")]
        public DateTime? CreatedDate { get; set; }
    }

    /// <summary>
    /// View model for creating a new user
    /// </summary>
    public class CreateUserViewModel
    {
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email address")]
        [Display(Name = "Email Address")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Display name is required")]
        [StringLength(100, ErrorMessage = "Display name must be between 2 and 100 characters", MinimumLength = 2)]
        [Display(Name = "Display Name")]
        public string DisplayName { get; set; } = string.Empty;

        [Display(Name = "Assign Roles")]
        public List<string> SelectedRoles { get; set; } = new List<string>();

        [Display(Name = "Send Welcome Email")]
        public bool SendWelcomeEmail { get; set; } = true;
    }

    /// <summary>
    /// View model for editing an existing user
    /// </summary>
    public class EditUserViewModel
    {
        public string Id { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email address")]
        [Display(Name = "Email Address")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Display name is required")]
        [StringLength(100, ErrorMessage = "Display name must be between 2 and 100 characters", MinimumLength = 2)]
        [Display(Name = "Display Name")]
        public string DisplayName { get; set; } = string.Empty;

        [Display(Name = "Email Confirmed")]
        public bool EmailConfirmed { get; set; }

        [Display(Name = "Account Enabled")]
        public bool IsEnabled { get; set; }

        [Display(Name = "Assigned Roles")]
        public List<string> SelectedRoles { get; set; } = new List<string>();

        [Display(Name = "Available Roles")]
        public List<string> AvailableRoles { get; set; } = new List<string>();
    }

    /// <summary>
    /// View model for the user list page
    /// </summary>
    public class UserListViewModel
    {
        public List<UserViewModel> Users { get; set; } = new List<UserViewModel>();
        public string? SearchTerm { get; set; }
        public string? RoleFilter { get; set; }
        public List<string> AvailableRoles { get; set; } = new List<string>();
    }

    /// <summary>
    /// View model for displaying current user debug information
    /// </summary>
    public class CurrentUserViewModel
    {
        [Display(Name = "User ID")]
        public string Id { get; set; } = string.Empty;

        [Display(Name = "User Name")]
        public string UserName { get; set; } = string.Empty;

        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [Display(Name = "Email Confirmed")]
        public bool EmailConfirmed { get; set; }

        [Display(Name = "Phone Number")]
        public string? PhoneNumber { get; set; }

        [Display(Name = "Phone Number Confirmed")]
        public bool PhoneNumberConfirmed { get; set; }

        [Display(Name = "Two-Factor Enabled")]
        public bool TwoFactorEnabled { get; set; }

        [Display(Name = "Lockout Enabled")]
        public bool LockoutEnabled { get; set; }

        [Display(Name = "Lockout End")]
        public DateTimeOffset? LockoutEnd { get; set; }

        [Display(Name = "Access Failed Count")]
        public int AccessFailedCount { get; set; }

        [Display(Name = "Roles")]
        public List<string> Roles { get; set; } = new List<string>();

        [Display(Name = "Claims")]
        public Dictionary<string, string> Claims { get; set; } = new Dictionary<string, string>();

        [Display(Name = "Is Authenticated")]
        public bool IsAuthenticated { get; set; }

        [Display(Name = "Authentication Type")]
        public string? AuthenticationType { get; set; }
    }
}
