using System.ComponentModel.DataAnnotations;
using SCICHRPortal.Data.Entities;

namespace SCICHRPortal.Web.Models.ViewModels.Admin
{
    public class LookupsViewModel
    {
        [MaxLength(50)]
        [Required(ErrorMessage = "Number of Month is required.")]
        public int NumberOfMonth { get; set; }

        [MaxLength(50)]
        [Required(ErrorMessage = "Name is required.")]
        public string? Name { get; set; }

        [MaxLength(60)]
        [Required(ErrorMessage = "User Name is required.")]
        public string? UserName { get; set; }

        [MaxLength(60)]
        [Required(ErrorMessage = "Last Name is required.")]
        public string? LastName { get; set; }

        [MaxLength(60)]
        [Required(ErrorMessage = "First Name is required.")]
        public string? FirstName { get; set; }

        [MaxLength(100)]
        [Required(ErrorMessage = "Full Name is required.")]
        public string? FullName { get; set; }

        [MaxLength(60)]
        [Display(Name = "Email Address")]
        [Required(ErrorMessage = "Email Address is required.")]
        [RegularExpression(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,4}$", ErrorMessage = "invalid email format.")]
        public string? Email { get; set; }

        [MaxLength(60)]
        [Required(ErrorMessage = "Contact Number is required.")]
        public string? ContactNumber { get; set; }

        [Required(ErrorMessage = "User Role is required.")]
        public int RoleId { get; set; }

        [Required(ErrorMessage = "Password is required")]
        [Display(Name = "Password")]
        [StringLength(255, MinimumLength = 8, ErrorMessage = "{0} should have a minimum of 8 characters.")]
        [RegularExpression("^(?=.*\\d)(?=.*[a-zA-Z]).+$", ErrorMessage = "{0} must contain at least one letter and one number.")]
        [DataType(DataType.Password)]
        public string? Password { get; set; }

        [Required(ErrorMessage = "Confirm Password is required.")]
        public string? ConfirmPassword { get; set; }

        [Required(ErrorMessage = "Current Password is required")]
        [Display(Name = "Current Password")]
        public string? CurrentPassword { get; set; }

        [MaxLength(15)]
        public string? StudentNumber { get; set; }

        [MaxLength(30)]
        [Required(ErrorMessage = "Middle Name is required.")]
        public string? MiddleName { get; set; }

        [MaxLength(5)]
        public string? Suffix { get; set; }

        public DateTime? CreatedAt { get; set; }
        public DateTime? DateCreated { get; set; }
        public int? UserId { get; set; }

        //user Details

        [MaxLength(60)]
        [Required(ErrorMessage = "User Name is required.")]
        public string? Username { get; set; }
    }

}
