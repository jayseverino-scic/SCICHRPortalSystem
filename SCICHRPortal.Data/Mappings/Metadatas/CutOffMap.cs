using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SCICHRPortal.Data.Entities.Metadatas;

namespace SCICHRPortal.Data.Mappings.Metadatas
{
    public class CutOffMap
    {
        public CutOffMap(EntityTypeBuilder<CutOff> entityBuilder)
        { 
            entityBuilder.HasKey(e => e.CutOffId);
            entityBuilder.Property(e => e.StartDate).IsRequired();
            entityBuilder.Property(e => e.EndDate).IsRequired();
        }
    }
}
