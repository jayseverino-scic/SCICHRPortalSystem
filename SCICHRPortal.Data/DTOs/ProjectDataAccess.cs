namespace SCICHRPortal.Data.DTOs;

public sealed record ProjectDataAccess(bool IsRestricted, int? ProjectId = null,
    string? ProjectName = null, string? ProjectCode = null);

public sealed record EmployeeProjectFact(int EmployeeId, int? ProjectId,
    bool? ProjectDeleted, string? ProjectName, string? ProjectCode);

public sealed record ProjectIdentifierFact(int ProjectId, string? Name, string? Code);
