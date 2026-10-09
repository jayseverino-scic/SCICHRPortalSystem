namespace SCICHRPortal.Data.DTOs
{
    public sealed class EmployeeShiftHistoryPage
    {
        public List<EmployeeShiftHistoryRow> Data { get; set; } = [];
        public int Total { get; set; }
    }

    public sealed class EmployeeShiftHistoryRow
    {
        public int AssignedShiftId { get; set; }
        public string? ShiftName { get; set; }
        public DateTime? EffectiveStartDate { get; set; }
        public DateTime? EffectiveEndDate { get; set; }
        public bool IsTemporary { get; set; }
        public bool IsFlexibleShift { get; set; }
        public bool IsNoShift { get; set; }
        public bool IsNoBreak { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? CreatedBy { get; set; }
    }
}
