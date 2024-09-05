using System.ComponentModel.DataAnnotations;

namespace IOT_UI.Models
{
    public class UsersViewModel
    {
        [Required]
        public Guid UserID { get; set; }

        [Required]
        public string Email { get; set; }

        [Required]
        [StringLength(50, MinimumLength = 2)]
        public string FirstName { get; set; }

        [Required]
        [StringLength(50, MinimumLength = 2)]
        public string LastName { get; set; }

        public string? CustomerEmail { get; set; }

        [Required]
        [StringLength(30, MinimumLength = 6)]
        public string Password { get; set; }

        [Required]
        [StringLength(30, MinimumLength = 6)]
        [Compare("Password", ErrorMessage = "Passwords do not match.")]
        public string ConfirmPassword { get; set; }

        public bool EmailVerified { get; set; } = false;
        public bool IsDeleted { get; set; } = false;
    }
}
