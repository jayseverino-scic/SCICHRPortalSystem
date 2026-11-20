using SCICHRPortal.Data.DTOs;

namespace SCICHRPortal.Service.Implementations
{
    public partial class LeaveService
    {
        public async Task<LeaveImportResult> ImportAsync(IReadOnlyList<LeaveImportRow> rows, string actor, CancellationToken cancellationToken = default)
        {
            var result = new LeaveImportResult { TotalRows = rows.Count };
            var numbers = rows.Where(row => row.Error == null).Select(row => ImportKey(row.EmployeeNo)).Distinct().ToArray();
            var employees = numbers.Length == 0 ? [] : await LeaveRepository.GetImportEmployeesAsync(numbers, cancellationToken);
            var employeesByNumber = employees.GroupBy(employee => ImportKey(employee.EmployeeNo ?? ""))
                .ToDictionary(group => group.Key, group => group.ToArray());
            var descriptions = rows.Where(row => row.Error == null).Select(row => ImportKey(row.LeaveType)).Distinct().ToArray();
            var types = descriptions.Length == 0 ? [] : await LeaveRepository.GetImportLeaveTypesAsync(descriptions, cancellationToken);
            var typesByDescription = types.GroupBy(type => ImportKey(type.Description ?? ""))
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
                var matchingTypes = typesByDescription.GetValueOrDefault(ImportKey(row.LeaveType)) ?? [];
                if (error == null && matchingEmployees.Length != 1)
                    error = matchingEmployees.Length == 0 ? "Employee No. was not found or is deleted." : "Employee No. matches more than one employee.";
                if (error == null && matchingTypes.Length != 1)
                    error = matchingTypes.Length == 0 ? "Type of Leave was not found or is deleted." : "Type of Leave matches more than one description.";
                if (error == null)
                {
                    // Each row uses the normal audited, serializable creation path. Rejected
                    // rows never roll back earlier successful rows or overwrite existing leave records.
                    var saved = await WriteAsync(null, new LeaveWriteRequest
                    {
                        EmployeeId = matchingEmployees[0].EmployeeId,
                        LeaveTypeId = matchingTypes[0].LeaveTypeId,
                        FromDate = row.FromDate, FromTime = row.FromTime, ToDate = row.ToDate, ToTime = row.ToTime
                    }, actor, cancellationToken, ImportKey(row.EmployeeNo), ImportKey(row.LeaveType));
                    if (saved.Status == LeaveWriteStatus.Success)
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
