using System.Diagnostics;
using Microsoft.Extensions.Logging;
using SCICHRPortal.Data.DTOs;
using SCICHRPortal.Data.Entities;
using SCICHRPortal.Data.Entities.Metadatas;
using SCICHRPortal.Repository.Interfaces;
using SCICHRPortal.Service.Interfaces;
using SCICHRPortal.Utility.Helpers;

namespace SCICHRPortal.Service.Implementations;

public sealed class AttendanceImportService(
    IBiometricsLogService biometrics, IBiometricsLogRepository biometricsRepository,
    IEmployeeTimeLogService timeLogs, IProjectService projects, IEmployeeService employees,
    IEmployeeShiftService shifts, ILogger<AttendanceImportService> logger) : IAttendanceImportService
{
    public async Task<IReadOnlyList<Project>> GetScheduledProjectsAsync(CancellationToken cancellationToken)
    {
        var active = (await projects.GetAllAsync()).Where(project => !string.IsNullOrWhiteSpace(project.Name)).ToList();
        var activeNames = active.Select(project => project.Name ?? "").ToHashSet(StringComparer.OrdinalIgnoreCase);
        var mapped = (await biometricsRepository.GetTimekeepingMappingsAsync(cancellationToken))
            .Where(mapping => activeNames.Contains(mapping.ProjectName)).ToList();
        var byProject = mapped.ToLookup(mapping => mapping.ProjectName, StringComparer.OrdinalIgnoreCase);
        var sharedSerials = mapped.GroupBy(mapping => mapping.SerialNumber, StringComparer.OrdinalIgnoreCase)
            .Where(group => group.Select(mapping => mapping.ProjectName).Distinct(StringComparer.OrdinalIgnoreCase).Count() > 1)
            .Select(group => group.Key).ToHashSet(StringComparer.OrdinalIgnoreCase);

        var eligible = new List<Project>();
        foreach (var group in active.GroupBy(project => project.Name ?? "", StringComparer.OrdinalIgnoreCase))
        {
            var devices = byProject[group.Key].ToList();
            if (devices.Count == 0) continue;
            if (group.Count() > 1 || devices.Any(device => sharedSerials.Contains(device.SerialNumber))
                || devices.GroupBy(device => device.DeviceName, StringComparer.OrdinalIgnoreCase)
                    .Any(device => device.Select(mapping => mapping.SerialNumber).Distinct(StringComparer.OrdinalIgnoreCase).Count() > 1))
            {
                logger.LogWarning("Scheduled attendance import skipped ambiguous timekeeping mapping for project {ProjectName}", group.Key);
                continue;
            }
            eligible.Add(group.Single());
        }
        logger.LogInformation("Scheduled attendance import mapped {MappedProjects} of {ActiveProjects} active projects", eligible.Count, active.Count);
        return eligible;
    }

    public async Task<BiometricsImportResult> ImportBiometricsAsync(DateTime from, DateTime until, string projectName, string actor)
    {
        var stopwatch = Stopwatch.StartNew();
        var (source, aliases) = await biometrics.ImportDbDateRange(from, until, projectName);
        var numbers = source.Where(row => !string.IsNullOrEmpty(row.AccessNumber))
            .Select(row => row.AccessNumber ?? "").Distinct().ToList();
        var byNumber = await biometrics.GetEmployeesInBulkAsync(numbers);
        var rows = new List<BiometricsLog>();
        var now = DateTime.UtcNow;
        foreach (var punch in source)
        {
            if (punch.AccessNumber is not { } number || !byNumber.TryGetValue(number, out var employee)) continue;
            var localTime = TimeZoneInfo.ConvertTime(punch.TimeLogStamp, PhilippineTime.Zone).DateTime;
            rows.Add(new BiometricsLog
            {
                PersonnelId = number,
                LastName = employee.LastName ?? "",
                FirstName = employee.FirstName ?? "",
                Date = punch.RecordDate,
                // Preserve the second precision used by existing biometric records for duplicate matching.
                Time = localTime.AddTicks(-(localTime.Ticks % TimeSpan.TicksPerSecond)),
                LogType = punch.LogType ?? "",
                DeviceName = punch.DeviceSerialNumber ?? "",
                ProjectName = projectName,
                ImportSource = "Biometrics",
                CreatedAt = now,
                CreatedBy = actor
            });
        }
        var result = await biometrics.BulkInsertWithResultAsync(rows, skipExistingPunches: true, deviceAliases: aliases);
        var duplicates = rows.Count - result.Inserted - result.Failed;
        return new(source.Count, rows.Count, source.Count - rows.Count + duplicates, duplicates,
            result.Inserted, result.Failed, result.Errors, stopwatch.ElapsedMilliseconds);
    }

    public async Task<TimeLogImportResult> ImportTimeLogsAsync(DateTime from, DateTime until, string projectName,
        string actor, AttendanceSessionPhase phase = AttendanceSessionPhase.All, ImportAudit? audit = null,
        CancellationToken cancellationToken = default)
    {
        var project = (await projects.GetAllAsync()).FirstOrDefault(row =>
            string.Equals(row.Name, projectName, StringComparison.OrdinalIgnoreCase));
        if (project is null || project.Id <= 0) return new(0, 0, [], ProjectUnavailable: true);

        var contextStart = from.Date.AddDays(-1);
        var contextEnd = until.Date.AddDays(2);
        var punches = (await biometrics.FilterByProjectAndDateRange(contextStart, contextEnd.AddTicks(-1), projectName))
            .ToLookup(row => row.PersonnelId);
        var projectEmployees = (await employees.GetEmployeeByProject(project.Id)).ToList();
        var ids = projectEmployees.Select(row => row.EmployeeId).ToArray();
        var periods = (await shifts.GetPeriodsAsync(ids, contextStart, contextEnd, cancellationToken))
            .ToLookup(row => row.EmployeeId);
        var sessions = new List<EmployeeTimeLog>();
        var errors = new List<string>();
        foreach (var employee in projectEmployees)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var employeePunches = punches[employee.EmployeeNo];
            if (!employeePunches.Any()) continue;
            try
            {
                var result = EmployeeShiftSessions.Build(employee.EmployeeId, employeePunches,
                    periods[employee.EmployeeId].ToList(), from, until, phase);
                sessions.AddRange(result.Sessions);
                errors.AddRange(result.Errors);
            }
            catch (InvalidOperationException)
            {
                errors.Add($"Employee {employee.EmployeeId}: overlapping assignment history; review before importing.");
            }
        }
        if (errors.Count > 0) return new(0, 0, errors);
        cancellationToken.ThrowIfCancellationRequested();
        var summary = await timeLogs.ImportAsync(sessions, actor, audit);
        return new(summary.Imported, summary.Skipped, []);
    }
}
