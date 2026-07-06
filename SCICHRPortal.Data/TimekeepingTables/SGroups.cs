using System;
using System.Collections.Generic;
using System.Text;

namespace SCICHRPortal.Data.TimekeepingTables
{
    public class SGroups
    {
        public Guid Id { get; set; }
        public DateTimeOffset DateCreated { get; set; }
        public bool IsDeleted { get; set; }
        public string? Code { get; set; }
        public string? Name { get; set; }  
        public string? Description { get; set; }
        public Guid GroupTypeId { get; set; }
    }
}
