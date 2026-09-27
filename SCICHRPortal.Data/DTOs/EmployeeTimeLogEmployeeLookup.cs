namespace SCICHRPortal.Data.DTOs;

public sealed class EmployeeTimeLogEmployeeLookupItem
{
    public int EmployeeId { get; set; }
    public string? EmployeeNo { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
}

public sealed class EmployeeTimeLogEmployeeLookupPage
{
    public IReadOnlyList<EmployeeTimeLogEmployeeLookupItem> Data { get; set; } = Array.Empty<EmployeeTimeLogEmployeeLookupItem>();
    public bool More { get; set; }
}
