using SCICHRPortal.Data.DTOs;
using SCICHRPortal.Data.Entities;
using SCICHRPortal.Repository.Interfaces;
using SCICHRPortal.Service.Interfaces;

namespace SCICHRPortal.Service.Implementations
{
    public class LeaveService : ILeaveService
    {
        private ILeaveRepository LeaveRepository { get; }

        public LeaveService(ILeaveRepository leaveRepository)
        {
            LeaveRepository = leaveRepository;
        }

        public Task<LeaveWriteResult> CreateAsync(LeaveWriteRequest request, string actor, CancellationToken cancellationToken = default)
        {
            return WriteAsync(null, request, actor, cancellationToken);
        }

        public Task<LeaveWriteResult> UpdateAsync(int id, LeaveWriteRequest request, string actor, CancellationToken cancellationToken = default)
        {
            if (id <= 0)
                return Task.FromResult(new LeaveWriteResult(LeaveWriteStatus.Invalid, "Select a valid record."));

            return WriteAsync(id, request, actor, cancellationToken);
        }

        public Task<LeaveWriteResult> DeleteAsync(int id, string actor, CancellationToken cancellationToken = default)
        {
            if (id <= 0)
                return Task.FromResult(new LeaveWriteResult(LeaveWriteStatus.Invalid, "Select a valid record."));

            return ExecuteWriteAsync(async () =>
            {
                var record = await LeaveRepository.GetAsync(id, cancellationToken);
                if (record == null)
                    return new LeaveWriteResult(LeaveWriteStatus.NotFound, "Leave record was not found.");

                var changes = new Leave
                {
                    Deleted = true,
                    UpdatedAt = DateTime.UtcNow,
                    UpdatedBy = actor
                };
                await LeaveRepository.DeleteAsync(record, changes, cancellationToken);
                return new LeaveWriteResult(LeaveWriteStatus.Success, Id: id);
            }, cancellationToken);
        }

        public Task<LeavePage> GetPageAsync(LeavePageQuery query, CancellationToken cancellationToken)
        {
            return LeaveRepository.GetPageAsync(query, cancellationToken);
        }

        public Task<LeaveRow?> GetDetailsAsync(int id, CancellationToken cancellationToken)
        {
            return LeaveRepository.GetDetailsAsync(id, cancellationToken);
        }

        public Task<LeaveEmployeeLookupPage> SearchEmployeesAsync(string term, int page, CancellationToken cancellationToken)
        {
            return LeaveRepository.SearchEmployeesAsync(term, page, cancellationToken);
        }

        public Task<List<LeaveTypeLookupItem>> GetLeaveTypesAsync(CancellationToken cancellationToken)
        {
            return LeaveRepository.GetLeaveTypesAsync(cancellationToken);
        }

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
                var employeeNumber = ImportKey(row.EmployeeNo);
                var matchingEmployees = employeesByNumber.GetValueOrDefault(employeeNumber) ?? [];
                if (matchingEmployees.Length != 1)
                {
                    var error = matchingEmployees.Length == 0
                        ? "Employee No. was not found or is deleted."
                        : "Employee No. matches more than one employee.";
                    result.Errors.Add(new(row.RowNumber, row.EmployeeNo, error));
                    continue;
                }

                var leaveTypeDescription = ImportKey(row.LeaveType);
                var matchingTypes = typesByDescription.GetValueOrDefault(leaveTypeDescription) ?? [];
                if (matchingTypes.Length != 1)
                {
                    var error = matchingTypes.Length == 0
                        ? "Type of Leave was not found or is deleted."
                        : "Type of Leave matches more than one description.";
                    result.Errors.Add(new(row.RowNumber, row.EmployeeNo, error));
                    continue;
                }

                var request = new LeaveWriteRequest
                {
                    EmployeeId = matchingEmployees[0].EmployeeId,
                    LeaveTypeId = matchingTypes[0].LeaveTypeId,
                    FromDate = row.FromDate,
                    FromTime = row.FromTime,
                    ToDate = row.ToDate,
                    ToTime = row.ToTime
                };
                var saved = await ImportRowAsync(request, employeeNumber, leaveTypeDescription, actor, cancellationToken);
                if (saved.Status == LeaveWriteStatus.Success)
                    result.Imported++;
                else
                    result.Errors.Add(new(row.RowNumber, row.EmployeeNo, saved.Message ?? "The record could not be imported."));
            }
            return result;
        }

        private Task<LeaveWriteResult> WriteAsync(int? id, LeaveWriteRequest request, string actor, CancellationToken cancellationToken)
        {
            var (values, error) = Validate(request);
            if (values == null)
                return Task.FromResult(new LeaveWriteResult(LeaveWriteStatus.Invalid, error));

            return ExecuteWriteAsync(async () =>
            {
                var existing = id.HasValue ? await LeaveRepository.GetAsync(id.Value, cancellationToken) : null;
                if (id.HasValue && existing == null)
                    return new LeaveWriteResult(LeaveWriteStatus.NotFound, "Leave record was not found.");

                if (existing?.EmployeeId != values.EmployeeId &&
                    !await LeaveRepository.EmployeeExistsAsync(values.EmployeeId, cancellationToken))
                    return new LeaveWriteResult(LeaveWriteStatus.Invalid, "Select an available employee.");

                if (existing?.LeaveTypeId != values.LeaveTypeId &&
                    !await LeaveRepository.LeaveTypeExistsAsync(values.LeaveTypeId, cancellationToken))
                    return new LeaveWriteResult(LeaveWriteStatus.Invalid, "Select an available Type of Leave.");

                return await SaveLeaveAsync(existing, values, actor, cancellationToken);
            }, cancellationToken);
        }

        private async Task<LeaveWriteResult> SaveLeaveAsync(
            Leave? existing, LeaveValues values, string actor, CancellationToken cancellationToken)
        {
            if (await LeaveRepository.HasOverlapAsync(values.EmployeeId, values.From, values.To, existing?.LeaveId, cancellationToken))
                return new LeaveWriteResult(LeaveWriteStatus.Conflict, "This employee already has a Leave record overlapping this period.");

            var now = DateTime.UtcNow;
            var record = new Leave
            {
                EmployeeId = values.EmployeeId,
                LeaveTypeId = values.LeaveTypeId,
                From = values.From,
                To = values.To
            };

            if (existing == null)
            {
                record.CreatedAt = now;
                record.CreatedBy = actor;
                await LeaveRepository.InsertAsync(record, cancellationToken);
                return new LeaveWriteResult(LeaveWriteStatus.Success, Id: record.LeaveId);
            }

            record.UpdatedAt = now;
            record.UpdatedBy = actor;
            await LeaveRepository.UpdateAsync(existing, record, cancellationToken);
            return new LeaveWriteResult(LeaveWriteStatus.Success, Id: existing.LeaveId);
        }

        private static (LeaveValues? Values, string? Error) Validate(LeaveWriteRequest request)
        {
            if (request.EmployeeId is not int employeeId || employeeId <= 0)
                return (null, "Employee is required.");
            if (request.LeaveTypeId is not int leaveTypeId || leaveTypeId <= 0)
                return (null, "Type of Leave is required.");
            if (request.FromDate is not DateOnly fromDate || fromDate == DateOnly.MinValue ||
                request.ToDate is not DateOnly toDate || toDate == DateOnly.MinValue)
                return (null, "From and To dates are required.");
            if (request.FromTime is not TimeOnly fromTime || request.ToTime is not TimeOnly toTime)
                return (null, "From and To times are required.");
            if (fromTime.Ticks % TimeSpan.TicksPerMinute != 0 || toTime.Ticks % TimeSpan.TicksPerMinute != 0)
                return (null, "Enter times in whole minutes.");

            var from = fromDate.ToDateTime(fromTime, DateTimeKind.Unspecified);
            var to = toDate.ToDateTime(toTime, DateTimeKind.Unspecified);
            if (to <= from)
                return (null, "The end date and time must be later than the start date and time.");

            return (new LeaveValues(employeeId, leaveTypeId, from, to), null);
        }

        private async Task<LeaveWriteResult> ExecuteWriteAsync(
            Func<Task<LeaveWriteResult>> operation, CancellationToken cancellationToken)
        {
            try
            {
                return await LeaveRepository.ExecuteWriteAsync(async () =>
                {
                    var result = await operation();
                    return (result, result.Status == LeaveWriteStatus.Success);
                }, cancellationToken);
            }
            catch (LeavePersistenceConflictException)
            {
                return new LeaveWriteResult(LeaveWriteStatus.Conflict, "The records changed while saving. Reload and try again.");
            }
        }

        private Task<LeaveWriteResult> ImportRowAsync(LeaveWriteRequest request, string employeeNumber,
            string leaveTypeDescription, string actor, CancellationToken cancellationToken)
        {
            var (values, error) = Validate(request);
            if (values == null)
                return Task.FromResult(new LeaveWriteResult(LeaveWriteStatus.Invalid, error));

            return ExecuteWriteAsync(async () =>
            {
                // Recheck batch lookup matches inside the row transaction before saving.
                var employees = await LeaveRepository.GetImportEmployeesAsync([employeeNumber], cancellationToken);
                if (employees.Count != 1 || employees[0].EmployeeId != values.EmployeeId)
                    return new(LeaveWriteStatus.Invalid, "Employee No. no longer uniquely identifies the selected employee. Review this row.");

                var types = await LeaveRepository.GetImportLeaveTypesAsync([leaveTypeDescription], cancellationToken);
                if (types.Count != 1 || types[0].LeaveTypeId != values.LeaveTypeId)
                    return new(LeaveWriteStatus.Invalid, "Type of Leave no longer uniquely identifies the selected type. Review this row.");

                return await SaveLeaveAsync(null, values, actor, cancellationToken);
            }, cancellationToken);
        }

        private static string ImportKey(string value) => value.Trim().ToUpperInvariant();

        private sealed record LeaveValues(int EmployeeId, int LeaveTypeId, DateTime From, DateTime To);
    }
}
