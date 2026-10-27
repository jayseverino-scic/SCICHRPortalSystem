namespace SCICHRPortal.Data.DTOs;

public class BiometricsLogFilterQuery
{
    public string? Search { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string? ProjectName { get; set; }
    public string[] SortColumns { get; set; } = ["date", "time"];
    public string[] SortDirections { get; set; } = ["desc", "desc"];

    public virtual bool IsValid() => (Search?.Length ?? 0) <= 200 && (ProjectName?.Length ?? 0) <= 200 &&
        (!StartDate.HasValue || !EndDate.HasValue || StartDate.Value.Date <= EndDate.Value.Date) &&
        SortColumns is { Length: > 0 and <= 5 } && SortDirections is not null &&
        SortColumns.Length == SortDirections.Length &&
        SortColumns.All(column => column is "personnelId" or "lastName" or "firstName" or "date" or "time") &&
        SortDirections.All(direction => direction is "asc" or "desc");
}

public class BiometricsLogPageQuery : BiometricsLogFilterQuery
{
    public int Draw { get; set; }
    public int Start { get; set; }
    public int Length { get; set; } = 5;

    public override bool IsValid() => base.IsValid() && Draw >= 0 && Start >= 0 && Length is -1 or 5 or 10 or 20 or 40 or 80;
}

public class BiometricsLogPage
{
    public int Draw { get; set; }
    public int RecordsTotal { get; set; }
    public int RecordsFiltered { get; set; }
    public List<BiometricsLogPageRow> Data { get; set; } = [];
}

public class BiometricsLogPageRow
{
    public int BiometricsLogId { get; set; }
    public string? PersonnelId { get; set; }
    public string? LastName { get; set; }
    public string? FirstName { get; set; }
    public DateTime? Date { get; set; }
    public DateTime? Time { get; set; }
    public string? LogType { get; set; }
    public string? ProjectName { get; set; }
}
