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
                    Description = "Administrator"
                },
                new Role
                {
                    RoleId = 2,
                    Name = "Admission",
                    Description = "Admission"
                },
                new Role
                {
                    RoleId = 3,
                    Name = "Parent",
                    Description = "Parent"
                },
                  new Role
                {
                    RoleId = 4,
                    Name = "Registrar",
                    Description = "Registrar"
                },
                new Role
                {
                    RoleId = 5,
                    Name = "HS Principal",
                    Description = "HS Principal"
                },
                new Role
                {
                    RoleId = 6,
                    Name = "Elem Principal",
                    Description = "Elem Principal"
                },
                new Role
                {
                    RoleId = 7,
                    Name = "HS Coordinator",
                    Description = "HS Coordinator"
                },
                new Role
                {
                    RoleId = 8,
                    Name = "Elem Coordinator",
                    Description = "Elem Coordinator"
                },
                new Role
                {
                    RoleId = 9,
                    Name = "Teacher Elem",
                    Description = "Teacher Elem"
                },
                new Role
                {
                    RoleId = 10,
                    Name = "Teacher HS",
                    Description = "Teacher HS"
                },
                new Role
                {
                    RoleId = 11,
                    Name = "Librian",
                    Description = "Librian"
                },
                new Role
                {
                    RoleId = 12,
                    Name = "Supply Custodian",
                    Description = "Supply Custodian"
                },
                new Role
                {
                    RoleId = 13,
                    Name = "Student",
                    Description = "Student"
                },
                new Role
                {
                    RoleId = 14,
                    Name = "Other",
                    Description = "Other"
                }
            });

        }
    }
}
