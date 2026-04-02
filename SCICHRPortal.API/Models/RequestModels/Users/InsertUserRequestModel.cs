using System.ComponentModel.DataAnnotations;
using SCICHRPortal.Data.Entities;

namespace SCICHRPortal.API.Models.RequestModels.Authenticated.Users
{
    public class InsertUserRequestModel
    {
        [Required]
        [MaxLength(60)]
        public string? FirstName { get; set; }

        [MaxLength(60)]
        public string? MiddleName { get; set; }

        [Required]
        [MaxLength(60)]
        public string? LastName { get; set; }

        [Required]
        [EmailAddress]
        public string? Email { get; set; }

        [Required]
        [MaxLength(60)]
        public string? ContactNumber { get; set; }

        //[Required]
        public int RoleId { get; set; }

        public ICollection<UserRole>? UserRoles { get; set; }

        public bool? IsApproved { get; set; }
    }
}
