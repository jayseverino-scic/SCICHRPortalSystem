using System.Linq.Expressions;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using SCICHRPortal.Data.DTOs;
using SCICHRPortal.Data.Entities;

namespace SCICHRPortal.Repository.Implementations;

public partial class EmployeeTimeLogRepository
{
    public async Task<EmployeeTimeLogPage> GetPageAsync(EmployeeTimeLogPageQuery request, CancellationToken cancellationToken = default)
    {
        if (!request.IsValid()) throw new ArgumentException("Invalid time log page query.", nameof(request));
        var query = Context.EmployeeTimeLog.AsNoTracking().Where(e => !e.Deleted);
        var total = await query.CountAsync(cancellationToken);
        if (request.StartDate.HasValue)
        {
            var start = request.StartDate.Value.Date;
            query = query.Where(e => e.DateIn >= start);
        }
        if (request.EndDate.HasValue)
        {
            var end = request.EndDate.Value.Date;
            if (end < DateTime.MaxValue.Date)
            {
                var endExclusive = end.AddDays(1);
                query = query.Where(e => e.DateIn < endExclusive);
            }
            else query = query.Where(e => e.DateIn <= DateTime.MaxValue);
        }
        if (!string.IsNullOrWhiteSpace(request.ProjectName))
        {
            var project = request.ProjectName.ToUpperInvariant();
            query = query.Where(e => e.ProjectTimeIn != null && e.ProjectTimeIn.ToUpper() == project);
        }
        // Match DataTables smart search: all words (or quoted phrases) must occur in the row.
        foreach (Match match in Regex.Matches(request.Search ?? "", "\"([^\"]+)\"|(\\S+)", RegexOptions.None, TimeSpan.FromSeconds(1)))
        {
            var term = (match.Groups[1].Success ? match.Groups[1].Value : match.Value).ToUpperInvariant();
            var text = Expression.Call(SearchableText.Body, nameof(string.ToUpper), Type.EmptyTypes);
            var contains = Expression.Call(text, nameof(string.Contains), Type.EmptyTypes, Expression.Constant(term));
            query = query.Where(Expression.Lambda<Func<EmployeeTimeLog, bool>>(contains, SearchableText.Parameters));
        }
        var filtered = await query.CountAsync(cancellationToken);
        IOrderedQueryable<EmployeeTimeLog>? ordered = null;
        for (var i = 0; i < request.SortColumns.Length; i++)
        {
            var key = SortKey(request.SortColumns[i]);
            var descending = request.SortDirections[i] == "desc";
            ordered = ordered is null
                ? descending ? query.OrderByDescending(key) : query.OrderBy(key)
                : descending ? ordered.ThenByDescending(key) : ordered.ThenBy(key);
        }
        var rows = await ordered!.ThenBy(e => e.TimeLogId).Skip(request.Start).Take(request.Length)
            .Select(e => new EmployeeTimeLogPageRow
            {
                TimeLogId = e.TimeLogId, EmployeeId = e.EmployeeId,
                EmployeeNo = e.Employee == null ? null : e.Employee.EmployeeId.ToString(),
                EmployeeName = (e.Employee == null ? "" : e.Employee.LastName) + "," + (e.Employee == null ? "" : e.Employee.FirstName),
                DateIn = e.DateIn, DateOut = e.DateOut, TimeIn = e.TimeIn, TimeOut = e.TimeOut,
                ShiftStart = e.ShiftStart, ShiftEnd = e.ShiftEnd, IsFlexibleShift = e.IsFlexibleShift,
                IsNoShift = e.IsNoShift, IsNoBreak = e.IsNoBreak, ProjectTimeIn = e.ProjectTimeIn,
                ProjectTimeOut = e.ProjectTimeOut, DeviceTimeIn = e.DeviceTimeIn, DeviceTimeOut = e.DeviceTimeOut,
                SystemRemarks = e.SystemRemarks, CreatedAt = e.CreatedAt
            }).ToListAsync(cancellationToken);
        return new EmployeeTimeLogPage { Draw = request.Draw, RecordsTotal = total, RecordsFiltered = filtered, Data = rows };
    }

    private static Expression<Func<EmployeeTimeLog, object?>> SortKey(string column) => column switch
    {
        "employeeNo" => e => e.EmployeeId,
        "employeeName" => e => e.Employee!.LastName + "," + e.Employee.FirstName,
        "dateIn" => e => e.DateIn, "dateOut" => e => e.DateOut,
        "timeIn" => e => e.TimeIn.HasValue ? e.TimeIn.Value.TimeOfDay : (TimeSpan?)null,
        "timeOut" => e => e.TimeOut.HasValue ? e.TimeOut.Value.TimeOfDay : (TimeSpan?)null,
        "shiftStart" => e => e.ShiftStart.HasValue ? e.ShiftStart.Value.TimeOfDay : (TimeSpan?)null,
        "shiftEnd" => e => e.ShiftEnd.HasValue ? e.ShiftEnd.Value.TimeOfDay : (TimeSpan?)null,
        "projectTimeIn" => e => e.ProjectTimeIn, "projectTimeOut" => e => e.ProjectTimeOut,
        "deviceTimeIn" => e => e.DeviceTimeIn, "deviceTimeOut" => e => e.DeviceTimeOut,
        "systemRemarks" => e => e.SystemRemarks,
        _ => throw new ArgumentException("Unsupported time log sort column.", nameof(column))
    };

    // These expressions translate to SQL; no formatted/client-side row list is materialized for search.
    // Dates and times use the same display format as the grid (MM/DD/YYYY and h:mm A).
    private static readonly Expression<Func<EmployeeTimeLog, string>> SearchableText = e =>
        e.EmployeeId.ToString() + " " + (e.Employee == null ? "" : (e.Employee.LastName ?? "") + "," + (e.Employee.FirstName ?? "")) + " " +
        (e.ProjectTimeIn ?? "") + " " + (e.ProjectTimeOut ?? "") + " " + (e.DeviceTimeIn ?? "") + " " + (e.DeviceTimeOut ?? "") + " " +
        (e.SystemRemarks ?? "") + " " +
        (e.DateIn.HasValue ? e.DateIn.Value.Month.ToString().PadLeft(2, '0') + "/" + e.DateIn.Value.Day.ToString().PadLeft(2, '0') + "/" + e.DateIn.Value.Year.ToString().PadLeft(4, '0') : "") + " " +
        (e.DateOut.HasValue ? e.DateOut.Value.Month.ToString().PadLeft(2, '0') + "/" + e.DateOut.Value.Day.ToString().PadLeft(2, '0') + "/" + e.DateOut.Value.Year.ToString().PadLeft(4, '0') : "") + " " +
        (e.TimeIn.HasValue ? (e.TimeIn.Value.Hour % 12 == 0 ? 12 : e.TimeIn.Value.Hour % 12).ToString() + ":" + e.TimeIn.Value.Minute.ToString().PadLeft(2, '0') + (e.TimeIn.Value.Hour < 12 ? " AM" : " PM") : "") + " " +
        (e.TimeOut.HasValue ? (e.TimeOut.Value.Hour % 12 == 0 ? 12 : e.TimeOut.Value.Hour % 12).ToString() + ":" + e.TimeOut.Value.Minute.ToString().PadLeft(2, '0') + (e.TimeOut.Value.Hour < 12 ? " AM" : " PM") : "") + " " +
        (e.ShiftStart.HasValue ? (e.ShiftStart.Value.Hour % 12 == 0 ? 12 : e.ShiftStart.Value.Hour % 12).ToString() + ":" + e.ShiftStart.Value.Minute.ToString().PadLeft(2, '0') + (e.ShiftStart.Value.Hour < 12 ? " AM" : " PM") : "") + " " +
        (e.ShiftEnd.HasValue ? (e.ShiftEnd.Value.Hour % 12 == 0 ? 12 : e.ShiftEnd.Value.Hour % 12).ToString() + ":" + e.ShiftEnd.Value.Minute.ToString().PadLeft(2, '0') + (e.ShiftEnd.Value.Hour < 12 ? " AM" : " PM") : "");
}
