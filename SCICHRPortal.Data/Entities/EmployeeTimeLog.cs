using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SCICHRPortal.Data.Entities.Metadatas;
using SCICHRPortal.Data.TimekeepingTables;
using SCICHRPortal.Data.XscribeTables;

namespace SCICHRPortal.Data.Entities
{
    public class EmployeeTimeLog : BaseEntity
    {
        public int TimeLogId { get; set; }
        [Column("id")]
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
        public string? DeviceTimeIn { get; set;  }
        public string? DeviceTimeOut { get; set; }
        public string? ProjecTimeIn { get; set; }
        public string? ProjectTimeOut { get; set; }
        public XEmployee? Employee { get; set; }
        public XCompany_Branch? XCompany_Branch { get; set; }
        public SZKDevices? SZKDevices { get; set; }
    }
}
