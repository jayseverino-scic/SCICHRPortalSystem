using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace SCICHRPortal.Data.DTOs
{
    public sealed class OTRequestWriteRequest
    {
        [Required, Range(1, int.MaxValue)]
        public int? EmployeeId { get; set; }
        [Required]
        public DateOnly? FromDate { get; set; }
        [Required]
        public DateOnly? ToDate { get; set; }
        [Required]
        public TimeOnly? FromTime { get; set; }
        [Required]
        public TimeOnly? ToTime { get; set; }
    }
    public enum OTRequestWriteStatus { Success, Invalid, NotFound, Conflict }
    public sealed record OTRequestWriteResult(OTRequestWriteStatus Status, string? Message = null, int? Id = null);

    public sealed class OTRequestPageQuery
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

    public sealed class OTRequestPage
    {
        public int Draw { get; set; }
        public int RecordsTotal { get; set; }
        public int RecordsFiltered { get; set; }
        public List<OTRequestRow> Data { get; set; } = [];
    }

    public sealed class OTRequestRow
    {
        public int LeaveId { get; set; }
        public int EmployeeId { get; set; }
        public string? EmployeeNo { get; set; }
        public string? EmployeeName { get; set; }
        public bool EmployeeDeleted { get; set; }
        public DateTime From { get; set; }
        public DateTime To { get; set; }
    }

    public sealed class OTRequestEmployeeLookupItem
    {
        public int EmployeeId { get; set; }
        public string? EmployeeNo { get; set; }
        public string? EmployeeName { get; set; }
    }

    public sealed class OTRequestEmployeeLookupPage
    {
        public List<OTRequestEmployeeLookupItem> Data { get; set; } = [];
        public bool More { get; set; }
    }

}
