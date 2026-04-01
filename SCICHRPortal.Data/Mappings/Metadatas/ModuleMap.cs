using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SCICHRPortal.Data.Entities.Metadatas;

namespace SCICHRPortal.Data.Mappings.Metadatas
{
    public class ModuleMap
    {
        public ModuleMap(EntityTypeBuilder<Module> entityBuilder)
        {
            entityBuilder.HasKey(t => t.ModuleId);

            entityBuilder.Property(t => t.Description)
                .HasMaxLength(50)
                .IsRequired();
            entityBuilder.Property(t => t.CreatedBy).HasMaxLength(120);
            entityBuilder.Property(t => t.UpdatedBy).HasMaxLength(120);

        }
    }
}
