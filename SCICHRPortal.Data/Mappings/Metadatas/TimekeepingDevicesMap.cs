using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SCICHRPortal.Data.Entities;
using SCICHRPortal.Data.Entities.Metadatas;
using System.Security.Cryptography.X509Certificates;

namespace SCICHRPortal.Data.Mappings.Metadatas
{
    public class TimekeepingDevicesMap
    {
        public TimekeepingDevicesMap(EntityTypeBuilder<TimekeepingDevices> entityBuilder) 
        { 
            entityBuilder.HasKey(x => x.Id);
            entityBuilder.Property(x => x.Name).IsRequired();
            entityBuilder.Property(x => x.SerialNumber).IsRequired();
        }
    }
}
