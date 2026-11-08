namespace SCICHRPortal.Data.DTOs;

public class TardinessFilterQuery
{
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public int ProjectId { get; set; }
    public string? Search { get; set; }
    public string[] SortColumns { get; set; } = ["timeIn"];
    public string[] SortDirections { get; set; } = ["desc"];

    public virtual bool IsValid() => (!FromDate.HasValue || !ToDate.HasValue || FromDate.Value.Date <= ToDate.Value.Date) &&
        (!ToDate.HasValue || ToDate.Value.Date < DateTime.MaxValue.Date) &&
        ProjectId >= 0 && (Search?.Length ?? 0) <= 200 &&
        SortColumns is { Length: > 0 and <= 11 } && SortDirections is not null &&
        SortColumns.Length == SortDirections.Length &&
        SortColumns.All(column => column is "employeeNo" or "employeeName" or "departmentName" or "projectName" or
            "attendanceDate" or "shiftStart" or "shiftEnd" or "timeIn" or "timeOut" or "lateMinutes" or "undertimeMinutes") &&
        SortDirections.All(direction => direction is "asc" or "desc");
}

public class TardinessPageQuery : TardinessFilterQuery
{
    public int Draw { get; set; }
    public int Start { get; set; }
    public int Length { get; set; } = 20;

    public override bool IsValid() => base.IsValid() && Draw >= 0 && Start >= 0 && Length is 5 or 10 or 20 or 40 or 80;
}

public class TardinessRow
{
    public int EmployeeAttendanceId { get; set; }
    public string? EmployeeNo { get; set; }
    public string? EmployeeName { get; set; }
    public string? Position { get; set; }
    public string? DepartmentName { get; set; }
    public string? ProjectName { get; set; }
    public DateTime AttendanceDate { get; set; }
    public DateTime? ShiftStart { get; set; }
    public DateTime? ShiftEnd { get; set; }
    public DateTime TimeIn { get; set; }
    public DateTime TimeOut { get; set; }
    public double LateMinutes { get; set; }
    public double UndertimeMinutes { get; set; }
}

public class TardinessPage
{
    public int Draw { get; set; }
    public int RecordsTotal { get; set; }
    public int RecordsFiltered { get; set; }
    public List<TardinessRow> Data { get; set; } = [];
    public string? Warning { get; set; }
}

public class TardinessExport
{
    public List<TardinessRow> Data { get; set; } = [];
    public string? Warning { get; set; }
}
