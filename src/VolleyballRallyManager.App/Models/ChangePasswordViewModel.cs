using System.ComponentModel.DataAnnotations;

namespace VolleyballRallyManager.App.Models
{
    /// <summary>
    /// ViewModel for changing user password
    /// </summary>
    public class ChangePasswordViewModel
    {
        /// <summary>
        /// Current password for verification
        /// </summary>
        [Required(ErrorMessage = "Current password is required")]
        [DataType(DataType.Password)]
        [Display(Name = "Current Password")]
        public string CurrentPassword { get; set; } = string.Empty;

        /// <summary>
        /// New password to set
        /// </summary>
        [Required(ErrorMessage = "New password is required")]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "New Password")]
        public string NewPassword { get; set; } = string.Empty;

        /// <summary>
        /// Confirmation of new password
        /// </summary>
        [Required(ErrorMessage = "Password confirmation is required")]
        [DataType(DataType.Password)]
        [Display(Name = "Confirm New Password")]
        [Compare("NewPassword", ErrorMessage = "The new password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}
