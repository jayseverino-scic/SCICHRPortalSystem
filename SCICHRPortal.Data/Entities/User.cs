using SCICHRPortal.Data.Entities.Metadatas;

namespace SCICHRPortal.Data.Entities
{
    public class User : BaseEntity
    {
        public int UserId { get; set; }
        public string? FirstName { get; set; }

        public string? MiddleName { get; set; }

        public string? LastName { get; set; }

        public string? Username { get; set; }

        public string? Salt { get; set; }

        public string? Password { get; set; }

        public string? Email { get; set; }
        public string? ContactNumber { get; set; }

        public bool IsPasswordChanged { get; set; }

        public int LoginAttempts { get; set; }

        public bool Locked { get; set; }
        public bool IsApproved { get; set; }
        public bool Active { get; set; }

        public virtual ICollection<UserRole>? UserRoles { get; set; }
        //public virtual ICollection<Teacher>? Teachers { get; set; }
    }
}
