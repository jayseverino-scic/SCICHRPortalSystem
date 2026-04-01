using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SCICHRPortal.Data.Entities.Metadatas;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography.X509Certificates;

namespace SCICHRPortal.Data.Mappings.Metadatas
{
    public class LeaveTypeMap
    {
        public LeaveTypeMap(EntityTypeBuilder<LeaveType> entityBuilder) 
        {
            entityBuilder.HasKey(e => e.LeaveTypeId);
            entityBuilder.Property(e => e.LeaveDescription).HasMaxLength(256).IsRequired();
            entityBuilder.Property(e => e.AllowedDays).IsRequired();
            entityBuilder.Property(e => e.IsPaid).IsRequired();
        }
    }
}
