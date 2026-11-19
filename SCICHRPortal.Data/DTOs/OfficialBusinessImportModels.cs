namespace SCICHRPortal.Data.DTOs
{
    public sealed record OfficialBusinessImportRow(
        int RowNumber, string EmployeeNo,
        DateOnly? FromDate, TimeOnly? FromTime, DateOnly? ToDate, TimeOnly? ToTime,
        string? Error = null);

    public sealed record OfficialBusinessImportEmployee(int EmployeeId, string? EmployeeNo);

    public sealed record OfficialBusinessImportError(int RowNumber, string EmployeeNo, string Message);

    public sealed class OfficialBusinessImportResult
    {
        public int TotalRows { get; set; }
        public int Imported { get; set; }
        public int Rejected => Errors.Count;
        public List<OfficialBusinessImportError> Errors { get; set; } = [];
    }
}
