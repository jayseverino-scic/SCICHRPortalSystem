using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SCICHRPortal.Data.Entities.Metadatas;
using System;
using System.Collections.Generic;
using System.Text;

namespace SCICHRPortal.Data.Mappings.Metadatas
{
    public class DeviceMap
    {
        public DeviceMap(EntityTypeBuilder<Device> entityBuilder) 
        {
            entityBuilder.HasKey(x => x.Id);
            entityBuilder.Property(x => x.Name).IsRequired();
            entityBuilder.Property(x => x.SerialNumber).IsRequired();
        }
    }
}
