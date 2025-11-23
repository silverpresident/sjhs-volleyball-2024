using System.ComponentModel.DataAnnotations;

namespace VolleyballRallyManager.App.Models
{
    /// <summary>
    /// View model for the login page
    /// </summary>
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email address")]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required")]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; } = string.Empty;

        [Display(Name = "Remember me?")]
        public bool RememberMe { get; set; }

        /// <summary>
        /// URL to redirect to after successful login
        /// </summary>
        public string? ReturnUrl { get; set; }
    }
}
