namespace SCICHRPortal.Data.DTOs
{
    public sealed class OvertimePageQuery
    {
        public int Draw { get; set; }
        public int Start { get; set; }
        public int Length { get; set; } = 10;
        public string? Search { get; set; }

        public bool IsValid() => Draw >= 0 && Start >= 0 && Start <= int.MaxValue - 80 &&
            Length is 10 or 20 or 40 or 80 && (Search?.Length ?? 0) <= 200;
    }

    public sealed record OvertimePage(int Draw, int RecordsTotal, int RecordsFiltered, IReadOnlyList<OvertimeRow> Data);

    public sealed record OvertimeRow(
        string? EmployeeName,
        string? EmployeeHrisId,
        DateOnly? OvertimeDateFrom,
        TimeOnly? ApprovedStartTime,
        DateOnly? OvertimeDateTo,
        TimeOnly? ApprovedEndTime,
        double? FiledHours,
        double? ApprovedHours,
        string? RequestReference,
        TimeSpan? ClosedAt,
        TimeSpan? DateCreated,
        Guid? SummaryId);
}
