using System.Globalization;
using SCICHRPortal.Data.Entities;

namespace SCICHRPortal.Service.Implementations;

public static class TimeLogChanges
{
    public static bool IsProtected(EmployeeTimeLog record) =>
        string.Equals(record.SystemRemarks, "Biometrics", StringComparison.OrdinalIgnoreCase) ||
        string.Equals(record.SystemRemarks, "File", StringComparison.OrdinalIgnoreCase);

    public static string? Optional(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static IEnumerable<(string Name, object? Before, object? After)> Fields(EmployeeTimeLog before, EmployeeTimeLog after)
    {
        yield return ("Employee", before.EmployeeId, after.EmployeeId);
        yield return ("Date In", before.DateIn, after.DateIn);
        yield return ("Date Out", before.DateOut, after.DateOut);
        yield return ("Time In", before.TimeIn, after.TimeIn);
        yield return ("Time Out", before.TimeOut, after.TimeOut);
        yield return ("Project In", Optional(before.ProjectTimeIn), Optional(after.ProjectTimeIn));
        yield return ("Project Out", Optional(before.ProjectTimeOut), Optional(after.ProjectTimeOut));
        yield return ("Device In", Optional(before.DeviceTimeIn), Optional(after.DeviceTimeIn));
        yield return ("Device Out", Optional(before.DeviceTimeOut), Optional(after.DeviceTimeOut));
    }

    public static string? ProtectedFieldError(EmployeeTimeLog before, EmployeeTimeLog after)
    {
        if (!IsProtected(before)) return null;
        foreach (var field in Fields(before, after))
            if (field.Before is not null && !Equals(field.Before, field.After))
                return $"{field.Name} already has a value and cannot be changed on a {before.SystemRemarks} record.";
        return null;
    }

    public static IReadOnlyList<string> Describe(EmployeeTimeLog before, EmployeeTimeLog after)
    {
        var changes = Fields(before, after).Where(f => !Equals(f.Before, f.After))
            .Select(f => $"{f.Name}: {Format(f.Before, f.Name)} → {Format(f.After, f.Name)}").ToList();
        if (before.IsOB != after.IsOB) changes.Add($"OB: {(before.IsOB ? "Yes" : "No")} → {(after.IsOB ? "Yes" : "No")}");
        return changes;
    }

    private static string Format(object? value, string name) => value switch
    {
        null => "empty",
        DateTime date => date.ToString(name.StartsWith("Time") ? "yyyy-MM-dd hh:mm:ss tt" : "yyyy-MM-dd", CultureInfo.InvariantCulture),
        _ => OneLine(Convert.ToString(value, CultureInfo.InvariantCulture) ?? "")
    };

    public static string Append(string? existing, string actor, IEnumerable<string> actions, DateTime utcNow)
    {
        var date = DateTime.SpecifyKind(utcNow, DateTimeKind.Utc).AddHours(8)
            .ToString("yyyy-MM-dd hh:mm tt", CultureInfo.InvariantCulture);
        var lines = actions.Select(action => $"{OneLine(actor)} - {OneLine(action)} - {date}").ToArray();
        if (lines.Length == 0) return existing ?? "";
        return string.IsNullOrEmpty(existing) ? string.Join("\n", lines) : existing.TrimEnd('\r', '\n') + "\n" + string.Join("\n", lines);
    }

    private static string OneLine(string value) => value.Replace('\r', ' ').Replace('\n', ' ');
}
