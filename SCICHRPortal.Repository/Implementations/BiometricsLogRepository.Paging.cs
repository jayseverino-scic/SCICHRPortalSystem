using System.Linq.Expressions;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using SCICHRPortal.Data.DTOs;
using SCICHRPortal.Data.Entities;

namespace SCICHRPortal.Repository.Implementations;

public partial class BiometricsLogRepository
{
    public async Task<BiometricsLogPage> GetPageAsync(BiometricsLogPageQuery request, CancellationToken cancellationToken = default)
    {
        if (!request.IsValid()) throw new ArgumentException("Invalid biometrics page query.", nameof(request));
        var query = Context.BiometricsLog.AsNoTracking().Where(row => !row.Deleted);
        var total = await query.CountAsync(cancellationToken);
        query = ApplyFilters(query, request);
        var filtered = await query.CountAsync(cancellationToken);
        IQueryable<BiometricsLog> pageQuery = ApplySort(query, request);
        // DataTables uses -1 for all rows matching the current filters and sort.
        if (request.Length != -1) pageQuery = pageQuery.Skip(request.Start).Take(request.Length);
        var rows = await pageQuery
            .Select(PageRow).ToListAsync(cancellationToken);
        return new BiometricsLogPage { Draw = request.Draw, RecordsTotal = total, RecordsFiltered = filtered, Data = rows };
    }

    public async Task<List<BiometricsLogPageRow>> GetExportAsync(BiometricsLogFilterQuery request, CancellationToken cancellationToken = default)
    {
        if (!request.IsValid()) throw new ArgumentException("Invalid biometrics export query.", nameof(request));
        var query = Context.BiometricsLog.AsNoTracking().Where(row => !row.Deleted);
        return await ApplySort(ApplyFilters(query, request), request).Select(PageRow).ToListAsync(cancellationToken);
    }

    private static IQueryable<BiometricsLog> ApplyFilters(IQueryable<BiometricsLog> query, BiometricsLogFilterQuery request)
    {
        if (request.StartDate.HasValue)
        {
            var start = request.StartDate.Value.Date;
            query = query.Where(row => row.Date >= start);
        }
        if (request.EndDate.HasValue)
        {
            var end = request.EndDate.Value.Date;
            if (end < DateTime.MaxValue.Date)
            {
                var endExclusive = end.AddDays(1);
                query = query.Where(row => row.Date < endExclusive);
            }
            else query = query.Where(row => row.Date <= DateTime.MaxValue);
        }
        if (!string.IsNullOrWhiteSpace(request.ProjectName))
        {
            var project = request.ProjectName.ToUpperInvariant();
            query = query.Where(row => row.ProjectName != null && row.ProjectName.ToUpper() == project);
        }
        // Preserve DataTables smart search over displayed text, including formatted dates/times.
        foreach (Match match in Regex.Matches(request.Search ?? "", "\"([^\"]+)\"|(\\S+)", RegexOptions.None, TimeSpan.FromSeconds(1)))
        {
            var term = (match.Groups[1].Success ? match.Groups[1].Value : match.Value).ToUpperInvariant();
            var text = Expression.Call(SearchableText.Body, nameof(string.ToUpper), Type.EmptyTypes);
            var contains = Expression.Call(text, nameof(string.Contains), Type.EmptyTypes, Expression.Constant(term));
            query = query.Where(Expression.Lambda<Func<BiometricsLog, bool>>(contains, SearchableText.Parameters));
        }
        return query;
    }

    private static IOrderedQueryable<BiometricsLog> ApplySort(IQueryable<BiometricsLog> query, BiometricsLogFilterQuery request)
    {
        IOrderedQueryable<BiometricsLog>? ordered = null;
        for (var i = 0; i < request.SortColumns.Length; i++)
        {
            Expression<Func<BiometricsLog, object?>> key = request.SortColumns[i] switch
            {
                "personnelId" => row => row.PersonnelId,
                "lastName" => row => row.LastName,
                "firstName" => row => row.FirstName,
                "date" => row => row.Date,
                "time" => row => row.Time.HasValue ? row.Time.Value.TimeOfDay : (TimeSpan?)null,
                _ => throw new ArgumentException("Unsupported biometrics sort column.", nameof(request))
            };
            var descending = request.SortDirections[i] == "desc";
            // PostgreSQL puts nulls first for DESC; missing punches must follow dated ones.
            if (descending && request.SortColumns[i] is "date" or "time")
            {
                Expression<Func<BiometricsLog, bool>> missing = request.SortColumns[i] == "date"
                    ? row => !row.Date.HasValue : row => !row.Time.HasValue;
                ordered = ordered is null ? query.OrderBy(missing) : ordered.ThenBy(missing);
            }
            ordered = ordered is null
                ? descending ? query.OrderByDescending(key) : query.OrderBy(key)
                : descending ? ordered.ThenByDescending(key) : ordered.ThenBy(key);
        }
        return ordered!.ThenBy(row => row.BiometricsLogId);
    }

    private static readonly Expression<Func<BiometricsLog, BiometricsLogPageRow>> PageRow = row => new BiometricsLogPageRow
    {
        BiometricsLogId = row.BiometricsLogId, PersonnelId = row.PersonnelId,
        LastName = row.LastName, FirstName = row.FirstName, Date = row.Date, Time = row.Time,
        LogType = row.LogType, ProjectName = row.ProjectName
    };

    private static readonly Expression<Func<BiometricsLog, string>> SearchableText = row =>
        (row.PersonnelId ?? "") + " " + (row.LastName ?? "") + " " + (row.FirstName ?? "") + " " +
        (row.LogType ?? "") + " " + (row.ProjectName ?? "") + " " +
        (row.Date.HasValue ? row.Date.Value.Month.ToString().PadLeft(2, '0') + "/" + row.Date.Value.Day.ToString().PadLeft(2, '0') + "/" + row.Date.Value.Year.ToString().PadLeft(4, '0') : "") + " " +
        (row.Time.HasValue ? (row.Time.Value.Hour % 12 == 0 ? 12 : row.Time.Value.Hour % 12).ToString() + ":" + row.Time.Value.Minute.ToString().PadLeft(2, '0') + (row.Time.Value.Hour < 12 ? " AM" : " PM") : "");
}
