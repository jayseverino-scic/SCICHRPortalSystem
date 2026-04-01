using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SCICHRPortal.Data.Entities.Metadatas;

namespace SCICHRPortal.Data.Entities
{
    public class EmployeeShift : BaseEntity
    {
        public int AssignedShiftId { get; set; }
        public int ShiftId { get; set; }
        public int EmployeeId { get; set; }
        public int DepartmentId { get; set; }
        public DateTime? ShiftDate { get; set; }
        public DateTime? ShiftStart {  get; set; }
        public DateTime? ShiftEnd { get; set; }
        public DateTime? BreakStart { get; set; }
        public DateTime? BreakEnd {  get; set; }
        public bool IsFlexibleShift { get; set; }
        public bool IsFlexibleBreak { get; set; }
        public bool IsNoShift { get; set; }
        public bool IsNoBreak { get; set; }
        public Employee? Employee { get; set; }
        public Department? Department { get; set; }
        public Shift? Shift { get; set; }
    }
}
