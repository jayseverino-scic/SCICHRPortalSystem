using SCICHRPortal.Data.Entities.Metadatas;
using System;
using System.Collections.Generic;
using System.Text;

namespace SCICHRPortal.Data.Entities
{
    public class OTRequest : BaseEntity
    {
        public int OTRequestId { get; set; }
        public int EmployeeId { get; set; }
        public DateTime? DateIn { get; set; }
        public DateTime? DateOut { get; set; }
        public DateTime? TimeIn { get; set; }
        public DateTime? TimeOut { get; set; }
        public DateTime? ShiftStart { get; set; }
        public DateTime? ShiftEnd { get; set; }
        public DateTime DateRequest { get; set; }
        public string? Activity { get; set; }
        public Employee? Employee { get; set; }
    }
}
