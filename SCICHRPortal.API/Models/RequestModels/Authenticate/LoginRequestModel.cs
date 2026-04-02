using System.ComponentModel.DataAnnotations;

namespace SCICHRPortal.API.Models.RequestModels.Authenticate
{
    public class LoginRequestModel
    {
        [Required]
        public string? Email { get; set; }

        [Required]
        public string? Password { get; set; }
    }
}
