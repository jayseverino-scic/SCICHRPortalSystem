namespace SCICHRPortal.Data.DTOs;

public class EmployeeTimeLogPageQuery
{
    public int Draw { get; set; }
    public int Start { get; set; }
    public int Length { get; set; } = 5;
    public string? Search { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string? ProjectName { get; set; }
    public string[] SortColumns { get; set; } = ["employeeNo"];
    public string[] SortDirections { get; set; } = ["asc"];

    public bool IsValid() => Draw >= 0 && Start >= 0 && Length is 5 or 10 or 20 or 40 or 80 &&
        (Search?.Length ?? 0) <= 200 && (ProjectName?.Length ?? 0) <= 200 &&
        (!StartDate.HasValue || !EndDate.HasValue || StartDate.Value.Date <= EndDate.Value.Date) &&
        SortColumns is { Length: > 0 and <= 14 } && SortDirections is not null &&
        SortColumns.Length == SortDirections.Length &&
        SortColumns.All(column => column is "employeeNo" or "employeeName" or "dateIn" or "dateOut" or
            "timeIn" or "timeOut" or "shiftStart" or "shiftEnd" or "projectTimeIn" or "projectTimeOut" or
            "deviceTimeIn" or "deviceTimeOut" or "systemRemarks") &&
        SortDirections.All(direction => direction is "asc" or "desc");
}

public class EmployeeTimeLogPage
{
    public int Draw { get; set; }
    public int RecordsTotal { get; set; }
    public int RecordsFiltered { get; set; }
    public List<EmployeeTimeLogPageRow> Data { get; set; } = [];
}

public class EmployeeTimeLogPageRow
{
    public int TimeLogId { get; set; }
    public int EmployeeId { get; set; }
    public string? EmployeeNo { get; set; }
    public string? EmployeeName { get; set; }
    public DateTime? DateIn { get; set; }
    public DateTime? DateOut { get; set; }
    public DateTime? TimeIn { get; set; }
    public DateTime? TimeOut { get; set; }
    public DateTime? ShiftStart { get; set; }
    public DateTime? ShiftEnd { get; set; }
    public bool IsFlexibleShift { get; set; }
    public bool IsNoShift { get; set; }
    public bool IsNoBreak { get; set; }
    public string? ProjectTimeIn { get; set; }
    public string? ProjectTimeOut { get; set; }
    public string? DeviceTimeIn { get; set; }
    public string? DeviceTimeOut { get; set; }
    public string? SystemRemarks { get; set; }
    public bool IsOB { get; set; }
    public bool HasAttachment { get; set; }
    public string? CommentPreview { get; set; }
    public long Version { get; set; }
    public DateTime? CreatedAt { get; set; }
}
