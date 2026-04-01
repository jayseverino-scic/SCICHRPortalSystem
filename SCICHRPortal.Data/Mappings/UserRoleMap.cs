using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SCICHRPortal.Data.Entities;

namespace SCICHRPortal.Data.Mappings
{
    public class UserRoleMap
    {
        public UserRoleMap(EntityTypeBuilder<UserRole> entityBuilder)
        {
            entityBuilder.HasKey(t => t.UserRoleId);

            entityBuilder.Property(t => t.UserId).IsRequired();
            entityBuilder.Property(t => t.RoleId).IsRequired();
            entityBuilder.Property(t => t.CreatedBy).HasMaxLength(120);
            entityBuilder.Property(t => t.UpdatedBy).HasMaxLength(120);

            entityBuilder.HasData(new UserRole[] {
                new UserRole
                {
                    UserRoleId = 1,
                    UserId = 1,
                    RoleId = 1
                }
            });
        }
    }
}
