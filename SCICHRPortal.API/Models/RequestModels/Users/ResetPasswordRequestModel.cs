using System.ComponentModel.DataAnnotations;

namespace SCICHRPortal.API.Models.RequestModels.Authenticated.Users
{
    public class ResetPasswordRequestModel
    {
        public string? Username { get; set; }

        [Required]
        [StringLength(255, MinimumLength = 8, ErrorMessage = "{0} should have a minimum of 8 characters.")]
        [RegularExpression("^(?=.*\\d)(?=.*[a-zA-Z]).+$", ErrorMessage = "{0} must contain at least one letter and one number.")]
        [DataType(DataType.Password)]
        public string? Password { get; set; }

        [StringLength(255, MinimumLength = 8, ErrorMessage = "{0} should have a minimum of 8 characters.")]
        [RegularExpression("^(?=.*\\d)(?=.*[a-zA-Z]).+$", ErrorMessage = "{0} must contain at least one letter and one number.")]
        [DataType(DataType.Password)]
        public string? CurrentPassword { get; set; }

        public int UserId { get; set; }
    }
}
