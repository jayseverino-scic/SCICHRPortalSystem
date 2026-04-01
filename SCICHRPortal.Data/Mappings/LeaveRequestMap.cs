using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SCICHRPortal.Data.Entities;
using SCICHRPortal.Data.Entities.Metadatas;

namespace SCICHRPortal.Data.Mappings
{
    public class LeaveRequestMap
    {
        public LeaveRequestMap(EntityTypeBuilder<LeaveRequest> entityBuilder) 
        {
            entityBuilder.HasKey(e => e.LeaveRequestId);
            entityBuilder.Property(e => e.EmployeeId).IsRequired();
            entityBuilder.Property(e => e.LeaveTypeId).IsRequired();
            entityBuilder.Property(e => e.DateRequest).IsRequired();
            entityBuilder.Property(e => e.FromDate).IsRequired();
            entityBuilder.Property(e => e.ToDate).IsRequired();
            entityBuilder.Property(e => e.RequestDays).IsRequired();
            entityBuilder.Property(e => e.LeaveReason).HasMaxLength(1000).IsRequired();

            entityBuilder.HasOne(e => e.Employee)
               .WithMany()
               .HasForeignKey(u => u.EmployeeId)
               .OnDelete(DeleteBehavior.NoAction);
            entityBuilder.HasOne(e => e.LeaveType)
               .WithMany()
               .HasForeignKey(u => u.LeaveTypeId)
               .OnDelete(DeleteBehavior.NoAction);
        }
    }
}
