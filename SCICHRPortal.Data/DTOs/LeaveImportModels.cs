namespace SCICHRPortal.Data.DTOs
{
    public sealed record LeaveImportRow(
        int RowNumber, string EmployeeNo,
        DateOnly? FromDate, TimeOnly? FromTime, DateOnly? ToDate, TimeOnly? ToTime,
        string LeaveType,
        string? Error = null);

    public sealed record LeaveImportEmployee(int EmployeeId, string? EmployeeNo);

    public sealed record LeaveImportError(int RowNumber, string EmployeeNo, string Message);

    public sealed class LeaveImportResult
    {
        public int TotalRows { get; set; }
        public int Imported { get; set; }
        public int Rejected => Errors.Count;
        public List<LeaveImportError> Errors { get; set; } = [];
    }
}
