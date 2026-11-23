using System.ComponentModel.DataAnnotations;

namespace SCICHRPortal.Web.Models.ViewModels.Login
{
    public class LoginViewModel
    {
        public bool IsLogin { get; set; }
        [Required, EmailAddress, MaxLength(100), Display(Name = "Email address")]
        public string? Email { get; set; }
        [Required, MaxLength(255), DataType(DataType.Password)]
        public string? Password { get; set; }
    }
}
