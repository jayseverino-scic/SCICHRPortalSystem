using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SCICHRPortal.Data.Entities.Metadatas;
using System;
using System.Collections.Generic;
using System.Text;

namespace SCICHRPortal.Data.Mappings.Metadatas
{
    public class ProjectMap
    {
        public ProjectMap(EntityTypeBuilder<Project> entityBuilder) 
        {
            entityBuilder.HasKey(x => x.Id);
            entityBuilder.Property(x => x.Name).IsRequired();
            entityBuilder.Property(x => x.Code).IsRequired();
        }
    }
}
