using SCICHRPortal.Data.DTOs;

namespace SCICHRPortal.Service.Implementations
{
    public partial class OfficialBusinessService
    {
        public async Task<OfficialBusinessImportResult> ImportAsync(IReadOnlyList<OfficialBusinessImportRow> rows, string actor, CancellationToken cancellationToken = default)
        {
            var result = new OfficialBusinessImportResult { TotalRows = rows.Count };
            var numbers = rows.Where(row => row.Error == null).Select(row => ImportKey(row.EmployeeNo)).Distinct().ToArray();
            var employees = numbers.Length == 0 ? [] : await OfficialBusinessRepository.GetImportEmployeesAsync(numbers, cancellationToken);
            var employeesByNumber = employees.GroupBy(employee => ImportKey(employee.EmployeeNo ?? ""))
                .ToDictionary(group => group.Key, group => group.ToArray());
            foreach (var row in rows)
            {
                cancellationToken.ThrowIfCancellationRequested();
                if (row.Error != null)
                {
                    result.Errors.Add(new(row.RowNumber, row.EmployeeNo, row.Error));
                    continue;
                }
                string? error = null;
                var matchingEmployees = employeesByNumber.GetValueOrDefault(ImportKey(row.EmployeeNo)) ?? [];
                if (error == null && matchingEmployees.Length != 1)
                    error = matchingEmployees.Length == 0 ? "Employee No. was not found or is deleted." : "Employee No. matches more than one employee.";
                if (error == null)
                {
                    // Each row uses the normal audited, serializable creation path. Rejected
                    // rows never roll back earlier successful rows or overwrite existing OBs.
                    var saved = await WriteAsync(null, new OfficialBusinessWriteRequest
                    {
                        EmployeeId = matchingEmployees[0].EmployeeId,
                        FromDate = row.FromDate, FromTime = row.FromTime, ToDate = row.ToDate, ToTime = row.ToTime
                    }, actor, cancellationToken, ImportKey(row.EmployeeNo));
                    if (saved.Status == OfficialBusinessWriteStatus.Success)
                    {
                        result.Imported++;
                        continue;
                    }
                    error = saved.Message ?? "The record could not be imported.";
                }
                result.Errors.Add(new(row.RowNumber, row.EmployeeNo, error));
            }
            return result;
        }

        private static string ImportKey(string value) => value.Trim().ToUpperInvariant();
    }
}
