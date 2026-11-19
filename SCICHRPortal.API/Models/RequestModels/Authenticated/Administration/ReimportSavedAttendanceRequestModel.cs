namespace SCICHRPortal.API.Models.RequestModels.Authenticated.Administration;

public sealed record ReimportSavedAttendanceRequestModel(int ProjectId, DateOnly? AttendanceDate);
