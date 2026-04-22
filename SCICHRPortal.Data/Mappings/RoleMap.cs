using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SCICHRPortal.Data.Entities;

namespace SCICHRPortal.Data.Mappings
{
    public class RoleMap 
    {
        public RoleMap(EntityTypeBuilder<Role> entityBuilder)
        {
            entityBuilder.HasKey(t => t.RoleId);

            entityBuilder.Property(t => t.Name)
                .HasMaxLength(60)
                .IsRequired();

            entityBuilder.Property(t => t.Description)
                .HasMaxLength(100)
                .IsRequired();
            entityBuilder.Property(t => t.CreatedBy).HasMaxLength(120);
            entityBuilder.Property(t => t.UpdatedBy).HasMaxLength(120);

            entityBuilder.HasData(new Role[] {
                new Role
                {
                    RoleId = 1,
                    Name = "Administrator",
                    Description = "Administrator",
                    CreatedAt = new DateTime(2025, 4, 18, 10, 30, 0, DateTimeKind.Utc),
                    CreatedBy = "jun rivas"
                },
                new Role
                {
                    RoleId = 2,
                    Name = "Other",
                    Description = "Other",
                    CreatedAt = new DateTime(2025, 4, 18, 10, 30, 0, DateTimeKind.Utc),
                    CreatedBy = "jun rivas"
                }
            });

        }
    }
}
