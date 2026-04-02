using System.ComponentModel.DataAnnotations;
using SCICHRPortal.Data.Entities;

namespace SCICHRPortal.API.Models.RequestModels.Authenticated.Users
{
    public class UpdateUserRequestModel
    {
        [Required]
        public int UserId { get; set; }

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
        public string? ContactNumber { get; set; }

        public int RoleId { get; set; }
        public ICollection<UserRole>? UserRoles { get; set; }
    }
}
