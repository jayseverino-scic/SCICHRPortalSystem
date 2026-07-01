using System;
using System.Collections.Generic;
using System.Text;

namespace SCICHRPortal.Data.TimekeepingTables
{
    public class Groups
    {
        public int Id { get; set; }
        public DateTime DateCreated { get; set; }
        public bool IsDeleted { get; set; }
        public string? Code { get; set; }
        public string? Name { get; set; }  
        public string? Description { get; set; }
        public int GroupTypeId { get; set; }
    }
}
