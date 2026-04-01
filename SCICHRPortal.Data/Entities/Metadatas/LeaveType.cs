
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCICHRPortal.Data.Entities.Metadatas
{
    public class LeaveType : BaseEntity
    {
        public int LeaveTypeId { get; set; }
        public string? LeaveDescription { get; set; }
        public int AllowedDays { get; set; }
        public bool IsPaid { get; set; }
    }
}
