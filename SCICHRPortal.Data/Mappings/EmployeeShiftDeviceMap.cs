using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SCICHRPortal.Data.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace SCICHRPortal.Data.Mappings
{
    public class EmployeeShiftDeviceMap
    {
        public EmployeeShiftDeviceMap(EntityTypeBuilder<EmployeeShiftDevice> entityBuilder)
        {
            entityBuilder.HasKey(e => e.AssignedShiftId);
            entityBuilder.Property(e => e.ShiftId).IsRequired();
            entityBuilder.Property(e => e.EmployeeId).IsRequired();
            entityBuilder.Property(e => e.ShiftDate).IsRequired();
            entityBuilder.Property(e => e.ShiftStart).IsRequired();
            entityBuilder.Property(e => e.ShiftEnd).IsRequired();

            entityBuilder.HasOne(e => e.Employee)
               .WithMany()
               .HasForeignKey(u => u.EmployeeId)
               .OnDelete(DeleteBehavior.NoAction);

            entityBuilder.HasOne(e => e.Shift)
               .WithMany()
               .HasForeignKey(u => u.ShiftId)
               .OnDelete(DeleteBehavior.NoAction);
        }
    }
}
