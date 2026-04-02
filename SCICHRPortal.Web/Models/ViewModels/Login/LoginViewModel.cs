using System.ComponentModel.DataAnnotations;

namespace SCICHRPortal.Web.Models.ViewModels.Login
{
    public class LoginViewModel
    {

        public bool IsLogin { get; set; } = false;

        [MaxLength(60)]
        [Required(ErrorMessage = "User Name is required.")]
        public string? UserName { get; set; }

        [MaxLength(60)]
        [Required(ErrorMessage = "Last Name is required.")]
        public string? LastName { get; set; }
        [MaxLength(30)]

        [Required(ErrorMessage = "Middle Name is required.")]
        public string? MiddleName { get; set; }


        [MaxLength(60)]
        [Required(ErrorMessage = "First Name is required.")]
        public string? FirstName { get; set; }

        [MaxLength(60)]
        [Display(Name = "Email Address")]
        [RegularExpression(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,4}$", ErrorMessage = "Invalid email format.")]
        [Required(ErrorMessage = "Email is required")]
        public string? Email { get; set; }

        [MaxLength(60)]
        [Required(ErrorMessage = "Contact Number is required.")]
        public string? ContactNumber { get; set; }


        [Required(ErrorMessage = "Role is required.")]
        public string? RoleId { get; set; }

        [Required(ErrorMessage = "Password is required")]
        [Display(Name = "Password")]
        [StringLength(255, MinimumLength = 8, ErrorMessage = "{0} should have a minimum of 8 characters.")]
        [RegularExpression("^(?=.*\\d)(?=.*[a-zA-Z]).+$", ErrorMessage = "{0} must contain at least one letter and one number.")]
        [DataType(DataType.Password)]
        public string? Password { get; set; }

        [Required(ErrorMessage = "Confirm Password is required.")]
        public string? ConfirmPassword { get; set; }

    }
}
