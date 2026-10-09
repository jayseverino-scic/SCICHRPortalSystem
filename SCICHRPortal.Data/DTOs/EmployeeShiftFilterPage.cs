namespace SCICHRPortal.Data.DTOs
{
    public class EmployeeShiftFilterPage
    {
        public List<EmployeeShiftFilterRow> Data { get; set; } = [];
        public int Total { get; set; }
        public int FilteredTotal { get; set; }
        public bool AllFlexibleShiftSelected { get; set; }
        public bool AllNoShiftSelected { get; set; }
        public bool AllNoBreakSelected { get; set; }
    }

    public class EmployeeShiftFilterRow
    {
        public int EmployeeId { get; set; }
        public string? EmployeeNo { get; set; }
        public string? EmployeeName { get; set; }
        public int? DepartmentId { get; set; }
        public string? DepartmentName { get; set; }
        public int? ProjectId { get; set; }
        public string? ProjectName { get; set; }
        public int AssignedShiftId { get; set; }
        public int ShiftId { get; set; }
        public string? ShiftName { get; set; }
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
        public bool IsAssigned { get; set; }
        public bool IsFlexibleShift { get; set; }
        public bool IsNoShift { get; set; }
        public bool IsNoBreak { get; set; }
    }
}
