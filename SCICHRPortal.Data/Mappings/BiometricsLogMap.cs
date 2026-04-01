using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SCICHRPortal.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCICHRPortal.Data.Mappings
{
    public class BiometricsLogMap
    {
        public BiometricsLogMap(EntityTypeBuilder<BiometricsLog> entityBuilder) 
        {
            entityBuilder.HasKey(e => e.BiometricsLogId);
        }
    }
}
