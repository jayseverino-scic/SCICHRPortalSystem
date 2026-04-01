using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using SCICHRPortal.Data.Entities.Metadatas;

namespace SCICHRPortal.Data.Entities
{
    public class EmployeeAttendance : BaseEntity
    {
        public int EmployeeAttendanceId { get; set; }
        public int TimeLogId { get; set; }
        public int EmployeeId { get; set; }
        public DateTime TimeIn { get; set; }
        public DateTime TimeOut { get; set; }
        public DateTime? BreakIn { get; set; }
        public DateTime? BreakOut { get; set; }
        public DateTime? ShiftStart { get; set; }
        public DateTime? ShiftEnd { get; set; }
        public DateTime? BreakStart { get; set; }
        public DateTime? BreakEnd { get; set; }
        public double ShiftHours { get; set; }
        public double RegularHour { get; set; }
        public double TotalLoggedHours { get; set; }
        public bool ApprovedOT { get; set; }
        public double OTHours { get; set; }
        public double NDHours { get; set; }
        public double ShiftLate {  get; set; }
        public double ShiftUndertime { get; set; }
        public double BreakLate { get; set; }
        public double BreakUndertime { get; set; }
        public bool IsFlexibleShift { get; set; }
        public bool IsFlexibleBreak { get; set; }
        public bool IsNoBreak { get; set; }
        public bool IsNoShift { get; set; }
        public bool ApprovedHoliday { get; set; }
        public bool ApprovedHolidayOT { get; set; }
        public bool ApprovedSPHoliday { get; set; }
        public bool ApprovedSPHolidayOT { get; set; }
        public bool ApprovedRestDay { get; set; }
        public bool ApprovedRestDayOT { get; set; }
        public Employee? Employee { get; set; }
        public EmployeeTimeLog? EmployeeTimeLog { get; set; }
    }
}
