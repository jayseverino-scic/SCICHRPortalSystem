using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SCICHRPortal.Data.Entities;

namespace SCICHRPortal.Data.Mappings
{
    public class UserMap 
    {
        public UserMap(EntityTypeBuilder<User> entityBuilder)
        {
            entityBuilder.HasKey(t => t.UserId);

            entityBuilder.Property(t => t.FirstName)
                .HasMaxLength(60)
                .IsRequired();

            entityBuilder.Property(t => t.MiddleName)
                .HasMaxLength(60);

            entityBuilder.Property(t => t.LastName)
                .HasMaxLength(60)
                .IsRequired();

            entityBuilder.Property(t => t.Username)
                .HasMaxLength(60)
                .IsRequired(false);

            entityBuilder.Property(t => t.Salt)
                .IsRequired();

            entityBuilder.Property(t => t.Password)
                .HasMaxLength(100)
                .IsRequired();

            entityBuilder.Property(t => t.Email)
                .HasMaxLength(100)
                .IsRequired();

            entityBuilder.Property(t => t.ContactNumber)
                .HasMaxLength(60)
                .IsRequired(false);

            entityBuilder.Property(t => t.IsPasswordChanged)
                .IsRequired();

            entityBuilder.Property(t => t.LoginAttempts)
                .IsRequired();

            entityBuilder.Property(t => t.Locked)
                .IsRequired();

            entityBuilder.Property(t => t.IsApproved)
                .IsRequired();
            entityBuilder.Property(t => t.CreatedBy).HasMaxLength(120);
            entityBuilder.Property(t => t.UpdatedBy).HasMaxLength(120);

            entityBuilder.HasData(new User[]
            {
                new User
                {
                    UserId = 1,
                    FirstName = "Super",
                    LastName = "Admin",
                    Username = "superadmin",
                    Email = "superadmin@mail.com",
                    Salt = "ml4A7caIeJit28zFyeiXVA==",
                    Password = "4DRtkqzRrxUk9Px/+Zu7vzTIk5f0dHc4mPgicSMkQzI=",
                    IsPasswordChanged = false,
                    LoginAttempts = 0,
                    Locked = false,
                    IsApproved = true,
                    CreatedAt = new DateTime(2025, 4, 18, 10, 30, 0, DateTimeKind.Utc),
                    CreatedBy = "jun rivas"
                },
            });
        }
    }
}
