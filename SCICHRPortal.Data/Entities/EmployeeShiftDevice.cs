using SCICHRPortal.Data.Entities.Metadatas;
using System;
using System.Collections.Generic;
using System.Text;

namespace SCICHRPortal.Data.Entities
{
    public class EmployeeShiftDevice : BaseEntity
    {
        public int AssignedShiftId { get; set; }
        public int ShiftId { get; set; }
        public int EmployeeId { get; set; }
        public DateTime? ShiftDate { get; set; }
        public DateTime? MondayShiftStart { get; set; }
        public DateTime? MondayShiftEnd { get; set; }
        public DateTime? TuesdayShiftStart { get; set; }
        public DateTime? TuesdayShiftEnd { get; set; }
        public DateTime? WednesdayShiftStart { get; set; }
        public DateTime? WednesdayShiftEnd { get; set; }
        public DateTime? ThursdayShiftStart { get; set; }
        public DateTime? ThursdayShiftEnd { get; set; }
        public DateTime? FridayShiftStart { get; set; }
        public DateTime? FridayShiftEnd { get; set; }
        public DateTime? SaturdayShiftStart { get; set; }
        public DateTime? SaturdayShiftEnd { get; set; }
        public DateTime? SundayShiftStart { get; set; }
        public DateTime? SundayShiftEnd { get; set; }
        public bool IsFlexibleShift { get; set; }
        public bool IsNoShift { get; set; }
        public bool IsNoBreak { get; set; }
        public string? Devicename { get; set; }
        public Employee? Employee { get; set; }
        public Shift? Shift { get; set; }
    }
}
