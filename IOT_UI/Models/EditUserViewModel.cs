using System.ComponentModel.DataAnnotations;

namespace IOT_UI.Models
{
    public class EditUserViewModel
    {
        public Guid UserID { get; set; }

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
        public string Email { get; set; }

        [Required(ErrorMessage = "First name is required.")]
        [StringLength(50, ErrorMessage = "First name cannot be longer than 50 characters.")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Last name is required.")]
        [StringLength(50, ErrorMessage = "Last name cannot be longer than 50 characters.")]
        public string LastName { get; set; }

        public Guid? CustomerId { get; set; }
        public bool EmailVerified { get; set; } = false;
        public bool IsDeleted { get; set; } = false;
    }
}
