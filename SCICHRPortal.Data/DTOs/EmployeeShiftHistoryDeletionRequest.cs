namespace SCICHRPortal.Data.DTOs
{
    public sealed class EmployeeShiftHistoryDeletionRequest
    {
        public bool ApplyToFilter { get; set; }
        public int ProjectId { get; set; }
        public int ShiftId { get; set; }
        public string FilterType { get; set; } = "All";
        public string? SearchKeyword { get; set; }
        public DateTime? AsOf { get; set; }
        public List<int> SelectedEmployeeIds { get; set; } = [];
        public List<int> ExcludedEmployeeIds { get; set; } = [];
    }
}
