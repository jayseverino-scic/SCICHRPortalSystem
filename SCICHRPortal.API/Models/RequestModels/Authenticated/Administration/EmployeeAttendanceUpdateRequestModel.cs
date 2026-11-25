using SCICHRPortal.Data.DTOs;

namespace SCICHRPortal.API.Models.RequestModels.Authenticated.Administration;

public sealed record EmployeeAttendanceUpdateRequestModel(int ProjectId, DateOnly? AttendanceDate,
    IReadOnlyList<AttendanceApprovalEdit?>? Edits);
