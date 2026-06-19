using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SCICHRPortal.Data.Entities.Metadatas;

namespace SCICHRPortal.Data.Entities
{
    public class EmployeeTimeLog : BaseEntity
    {
        public int TimeLogId { get; set; }
        public int EmployeeId { get; set; }
        public DateTime? DateIn { get; set; }
        public DateTime? DateOut { get; set; }
        public DateTime? TimeIn { get; set; }
        public DateTime? TimeOut { get; set; }
        public DateTime? ShiftStart { get; set; }
        public DateTime? ShiftEnd { get; set; }
        public bool IsFlexibleShift { get; set; }
        public bool IsNoShift { get; set; }
        public bool IsNoBreak { get; set; }
        public string? SystemRemarks { get; set; }
        public Employee? Employee { get; set; }
    }
}
