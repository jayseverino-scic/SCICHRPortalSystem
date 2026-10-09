namespace SCICHRPortal.Data.DTOs
{
    public sealed class EmployeeShiftFilteredAssignmentRequest
    {
        public DateTime? EffectiveStartDate { get; set; }
        public DateTime? EffectiveEndDate { get; set; }
        public bool IsTemporary { get; set; }
        public DateTime? AsOf { get; set; }
        public int ProjectId { get; set; }
        public int ShiftId { get; set; }
        public string FilterType { get; set; } = "All";
        public string? SearchKeyword { get; set; }
        // Zero explicitly requests unassignment; null means no choice was supplied.
        public int? ScheduleId { get; set; }
        public bool ApplyAssignmentToFilter { get; set; } = true;
        public List<EmployeeShiftFilteredFlags> FlagFilters { get; set; } = [];
        public List<EmployeeShiftAssignmentChange> Changes { get; set; } = [];
    }

    public sealed class EmployeeShiftFilteredFlags
    {
        public DateTime? AsOf { get; set; }
        public int ProjectId { get; set; }
        public int ShiftId { get; set; }
        public string FilterType { get; set; } = "All";
        public string? SearchKeyword { get; set; }
        public bool? IsFlexibleShift { get; set; }
        public bool? IsNoShift { get; set; }
        public bool? IsNoBreak { get; set; }
    }

    public sealed class EmployeeShiftAssignmentChange
    {
        public int EmployeeId { get; set; }
        public bool PreserveSchedule { get; set; }
        public bool IsAssigned { get; set; }
        public bool IsFlexibleShift { get; set; }
        public bool IsNoShift { get; set; }
        public bool IsNoBreak { get; set; }
    }

    public enum EmployeeShiftAssignmentStatus { Success, Invalid, Conflict }

    public sealed record EmployeeShiftAssignmentResult(EmployeeShiftAssignmentStatus Status, int Affected = 0, string? Message = null);

    public sealed class EmployeeShiftAssignmentConflictException : Exception
    {
        public EmployeeShiftAssignmentConflictException() : base("The employee already has an active assignment.") { }
    }
}
