using System.ComponentModel.DataAnnotations;

namespace SCICHRPortal.Data.DTOs
{
    public sealed class LeaveWriteRequest
    {
        [Required]
        [Range(1, int.MaxValue)]
        public int? EmployeeId { get; set; }

        [Required]
        [Range(1, int.MaxValue)]
        public int? LeaveTypeId { get; set; }

        [Required]
        public DateOnly? FromDate { get; set; }

        [Required]
        public DateOnly? ToDate { get; set; }

        [Required]
        public TimeOnly? FromTime { get; set; }

        [Required]
        public TimeOnly? ToTime { get; set; }
    }

    public enum LeaveWriteStatus { Success, Invalid, NotFound, Conflict }

    public sealed record LeaveWriteResult(LeaveWriteStatus Status, string? Message = null, int? Id = null);

    public sealed class LeavePersistenceConflictException(Exception innerException)
        : Exception("A concurrent leave persistence conflict occurred.", innerException);

    public sealed class LeavePageQuery
    {
        public int Draw { get; set; }
        public int Start { get; set; }
        public int Length { get; set; } = 10;
        public string? Search { get; set; }
        public string SortColumn { get; set; } = "from";
        public string SortDirection { get; set; } = "desc";

        public bool IsValid() => Draw >= 0 && Start >= 0 && Start <= int.MaxValue - 80 &&
            Length is 10 or 20 or 40 or 80 && (Search?.Length ?? 0) <= 200 &&
            SortColumn is "employeeNo" or "employeeName" or "from" or "to" or "leaveType" &&
            SortDirection is "asc" or "desc";
    }

    public sealed class LeavePage
    {
        public int Draw { get; set; }
        public int RecordsTotal { get; set; }
        public int RecordsFiltered { get; set; }
        public List<LeaveRow> Data { get; set; } = [];
    }

    public sealed class LeaveRow
    {
        public int LeaveId { get; set; }
        public int EmployeeId { get; set; }
        public string? EmployeeNo { get; set; }
        public string? EmployeeName { get; set; }
        public bool EmployeeDeleted { get; set; }
        public int LeaveTypeId { get; set; }
        public string? LeaveType { get; set; }
        public bool LeaveTypeDeleted { get; set; }
        public DateTime From { get; set; }
        public DateTime To { get; set; }
    }

    public sealed class LeaveEmployeeLookupItem
    {
        public int EmployeeId { get; set; }
        public string? EmployeeNo { get; set; }
        public string? EmployeeName { get; set; }
    }

    public sealed class LeaveEmployeeLookupPage
    {
        public List<LeaveEmployeeLookupItem> Data { get; set; } = [];
        public bool More { get; set; }
    }

    public sealed record LeaveTypeLookupItem(int LeaveTypeId, string? Description);

}
