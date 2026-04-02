using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Diagnostics.CodeAnalysis;
using SCICHRPortal.Data.Entities.Metadatas;
using SCICHRPortal.Data.Enums;

namespace SCICHRPortal.Data.Mappings.Metadatas
{
    public class HolidayMap
    {
        public HolidayMap(EntityTypeBuilder<Holiday> entityBuilder)
        {
            entityBuilder.HasKey(h => h.HolidayId);

            entityBuilder.Property(h => h.HolidayName).IsRequired().HasMaxLength(256);
            entityBuilder.Property(h => h.HolidayDate).IsRequired();
            entityBuilder.Property(h => h.HolidayType).IsRequired().HasMaxLength(100);

            //entityBuilder.HasData(new Holiday[]
            //{
            //    new Holiday
            //    {
            //        HolidayId = 1,
            //        HolidayName = "New Year",
            //        HolidayType = (int) HolidayType.Regular,
            //        HolidayDate =Convert.ToDateTime("2023-01-01 00:00:00.000000")
            //    },
            //    new Holiday
            //    {
            //        HolidayId = 2,
            //        HolidayName = "Indepdence Day",
            //        HolidayType = (int) HolidayType.Regular,
            //        HolidayDate = Convert.ToDateTime("2023-06-12 00:00:00.000000")
            //    },
            //    new Holiday
            //    {
            //        HolidayId = 3,
            //        HolidayName = "Christmas Day",
            //        HolidayType = (int) HolidayType.Regular,
            //        HolidayDate = Convert.ToDateTime("2022-09-14 00:00:00.000000")
            //    },
            //    new Holiday
            //    {
            //        HolidayId = 4,
            //        HolidayName = "Araw ng Kagitingan",
            //        HolidayType = (int) HolidayType.Regular,
            //        HolidayDate = Convert.ToDateTime("2023-12-25 00:00:00.000000")
            //    },
            //    new Holiday
            //    {
            //        HolidayId = 5,
            //        HolidayName = "Labor Day",
            //        HolidayType = (int) HolidayType.Regular,
            //        HolidayDate = Convert.ToDateTime("2023-05-01 00:00:00.000000")
            //    },
            //    new Holiday
            //    {
            //        HolidayId = 6,
            //        HolidayName = "Andres Bonifacio Day",
            //        HolidayType= (int) HolidayType.Regular,
            //        HolidayDate = Convert.ToDateTime("2023-11-30 00:00:00.000000")
            //    },
            //    new Holiday
            //    {
            //        HolidayId = 7,
            //        HolidayName = "Rizal Day",
            //        HolidayType = (int) HolidayType.Regular,
            //        HolidayDate = Convert.ToDateTime("2023-12-30 00:00:00.000000")
            //    }
            //});
        }
    }
}
