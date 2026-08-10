using System;
using System.Collections.Generic;
using System.Text;

namespace SCICHRPortal.Data.Entities.Metadatas
{
    public class Project : BaseEntity
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Code { get; set; }
    }
}
