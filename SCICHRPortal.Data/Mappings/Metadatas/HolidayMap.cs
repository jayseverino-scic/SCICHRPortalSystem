using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SCICHRPortal.Data.Entities.Metadatas;
using SCICHRPortal.Data.Enums;
using System;
using System.Diagnostics.CodeAnalysis;

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

            entityBuilder.HasMany(holiday => holiday.Projects)
                .WithMany()
                .UsingEntity<HolidayProject>(
                    link => link.HasOne(assignment => assignment.Project).WithMany()
                        .HasForeignKey(assignment => assignment.ProjectId).OnDelete(DeleteBehavior.NoAction),
                    link => link.HasOne(assignment => assignment.Holiday).WithMany()
                        .HasForeignKey(assignment => assignment.HolidayId).OnDelete(DeleteBehavior.NoAction));
       
        }
    }
}
