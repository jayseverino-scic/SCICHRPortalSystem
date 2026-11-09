using SCICHRPortal.Data.DTOs;
using SCICHRPortal.Data.Entities.Metadatas;
using SCICHRPortal.Service.Implementations;
using SCICHRPortal.Utility.Interface;

namespace SCICHRPortal.Service.Interfaces;

public interface IAttendanceImportService : IScopedService
{
    Task<IReadOnlyList<Project>> GetScheduledProjectsAsync(CancellationToken cancellationToken);

    Task<BiometricsImportResult> ImportBiometricsAsync(DateTime from, DateTime until, string projectName, string actor);

    Task<TimeLogImportResult> ImportTimeLogsAsync(DateTime from, DateTime until, string projectName, string actor,
        AttendanceSessionPhase phase = AttendanceSessionPhase.All, ImportAudit? audit = null,
        CancellationToken cancellationToken = default);
}
