using SCICHRPortal.Data.Entities.Metadatas;

namespace SCICHRPortal.Data.Entities;

public sealed class EmployeeDailyAttendanceImportCheckpoint
{
    public int ProjectId { get; set; }
    public DateOnly AttendanceDate { get; set; }
    public bool DaytimeCompleted { get; set; }
    public bool OvernightCompleted { get; set; }
    public Project? Project { get; set; }
}
