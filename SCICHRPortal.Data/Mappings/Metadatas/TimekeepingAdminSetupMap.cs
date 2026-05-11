using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SCICHRPortal.Data.Entities;
using SCICHRPortal.Data.Entities.Metadatas;
using SCICHRPortal.Data.Enums;

namespace SCICHRPortal.Data.Mappings.Metadatas
{
    public class TimekeepingAdminSetupMap
    {
        public TimekeepingAdminSetupMap(EntityTypeBuilder<TimekeepingAdminSetup> entityBuilder) 
        {
            entityBuilder.HasKey(e => e.SetupId);

            entityBuilder.Property(e => e.AdminPassword).IsRequired().HasMaxLength(20);

  
        }
    }
}
