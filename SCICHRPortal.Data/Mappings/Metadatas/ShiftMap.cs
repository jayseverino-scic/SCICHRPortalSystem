using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SCICHRPortal.Data.Entities.Metadatas;

namespace SCICHRPortal.Data.Mappings.Metadatas
{
    public class ShiftMap
    {
        public ShiftMap(EntityTypeBuilder<Shift> entityBuilder) 
        {
            entityBuilder.HasKey(x => x.ShiftId);
            entityBuilder.Property(x => x.ShiftName).HasMaxLength(100).IsRequired();
            entityBuilder.Property(x => x.ShiftStart).IsRequired();
            entityBuilder.Property(x => x.ShiftEnd).IsRequired();


            //entityBuilder.HasData(new Shift[] {
            //    new Shift {
            //        ShiftId = 1,
            //        ShiftName = "Day Shift",
            //        ShiftStart = new DateTime(2026,1,1,7,0,0),
            //        ShiftEnd = new DateTime(2026,1,1,16,0,0)
            //    }
            //});
        }
    }
}
