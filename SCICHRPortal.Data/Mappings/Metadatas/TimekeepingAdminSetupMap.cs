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

            entityBuilder.Property(e => e.ShiftLateMinuteGracePeriod).IsRequired();
            entityBuilder.Property(e => e.BreakLateMinuteGracePeriod).IsRequired();
            entityBuilder.Property(e => e.ShiftLateTotalMinuteLimit).IsRequired();
            entityBuilder.Property(e => e.BreakLateTotalMinuteLimit).IsRequired();
            entityBuilder.Property(e => e.NoTimeLogCountLimit).IsRequired();
            entityBuilder.Property(e => e.NoLeaveAbsentCountLimit).IsRequired();

            //entityBuilder.HasData(new TimekeepingAdminSetup[]
            //{
            //    new TimekeepingAdminSetup
            //    {
            //        SetupId = 1,
            //        ShiftLateMinuteGracePeriod = 0,
            //        BreakLateMinuteGracePeriod = 0,
            //        ShiftLateTotalMinuteLimit = 0,
            //        BreakLateTotalMinuteLimit = 0,
            //        NoTimeLogCountLimit = 0,
            //        NoLeaveAbsentCountLimit = 0
            //    }
            //});
        }
    }
}
