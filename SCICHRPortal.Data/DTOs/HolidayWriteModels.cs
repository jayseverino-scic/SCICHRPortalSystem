namespace SCICHRPortal.Data.DTOs;

public sealed class HolidayCreateRequest
{
    public string? HolidayName { get; set; }
    public DateTime? HolidayDate { get; set; }
    public int? HolidayType { get; set; }
    public bool? AllProjects { get; set; }
    public int[]? ProjectIds { get; set; }
}

public sealed class HolidayUpdateRequest
{
    public int HolidayId { get; set; }
    public string? HolidayName { get; set; }
    public DateTime? HolidayDate { get; set; }
    public int? HolidayType { get; set; }
    public bool? AllProjects { get; set; }
    public int[]? ProjectIds { get; set; }
}

public enum HolidayWriteStatus { Success, Invalid, Conflict, NotFound, Forbidden }

public sealed record HolidayWriteResult(HolidayWriteStatus Status, string? Message = null);

public sealed class HolidayPersistenceConflictException(Exception innerException)
    : Exception("A concurrent holiday persistence conflict occurred.", innerException);
