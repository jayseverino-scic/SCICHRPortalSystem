using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SCICHRPortal.Data.Entities.Metadatas;

namespace SCICHRPortal.Data.Mappings.Metadatas
{
    public class PositionMap
    {
        public PositionMap(EntityTypeBuilder<Position> entityBuilder) 
        {
            entityBuilder.HasKey(e => e.PositionId);

            entityBuilder.Property(e => e.PositionName).IsRequired().HasMaxLength(256);
        }
    }
}
