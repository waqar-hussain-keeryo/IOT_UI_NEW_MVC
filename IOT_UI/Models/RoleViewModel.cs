using System.ComponentModel.DataAnnotations;

namespace IOT_UI.Models
{
    public class RoleViewModel
    {
        [Required]
        public Guid RoleID { get; set; }

        [Required]
        [StringLength(100)]
        public string RoleName { get; set; }
        public string RoleDescription { get; set; }
    }
}
