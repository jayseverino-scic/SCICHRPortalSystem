using SCICHRPortal.Data.DTOs;
using SCICHRPortal.Data.Entities;
using SCICHRPortal.Repository.Interfaces;
using SCICHRPortal.Service.Interfaces;

namespace SCICHRPortal.Service.Implementations
{
    public partial class LeaveService : ILeaveService
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
            return id <= 0 ? Task.FromResult(new LeaveWriteResult(LeaveWriteStatus.Invalid, "Select a valid record."))
                : WriteAsync(id, request, actor, cancellationToken);
        }

        private Task<LeaveWriteResult> WriteAsync(int? id, LeaveWriteRequest request, string actor, CancellationToken cancellationToken,
            string? importEmployeeNumber = null, string? importLeaveType = null)
        {
            var error = Validate(request);
            if (error != null)
                return Task.FromResult(new LeaveWriteResult(LeaveWriteStatus.Invalid, error));

            var from = request.FromDate!.Value.ToDateTime(request.FromTime!.Value, DateTimeKind.Unspecified);
            var to = request.ToDate!.Value.ToDateTime(request.ToTime!.Value, DateTimeKind.Unspecified);
            return LeaveRepository.ExecuteWriteAsync(async () =>
            {
                var existing = id.HasValue ? await LeaveRepository.GetAsync(id.Value, cancellationToken) : null;
                if (id.HasValue && existing == null)
                    return new(LeaveWriteStatus.NotFound, "Leave record was not found.");

                var employeeId = request.EmployeeId!.Value;
                if (importEmployeeNumber != null)
                {
                    // Preliminary batch lookups are only hints: recheck identity and uniqueness
                    // in this row's transaction so metadata edits cannot silently redirect a Leave.
                    var employees = await LeaveRepository.GetImportEmployeesAsync([importEmployeeNumber], cancellationToken);
                    if (employees.Count != 1 || employees[0].EmployeeId != employeeId)
                        return new(LeaveWriteStatus.Invalid, "Employee No. no longer uniquely identifies the selected employee. Review this row.");
                }
                else
                {
                    if (existing?.EmployeeId != employeeId && !await LeaveRepository.EmployeeExistsAsync(employeeId, cancellationToken))
                        return new(LeaveWriteStatus.Invalid, "Select an available employee.");
                }
                var leaveTypeId = request.LeaveTypeId!.Value;
                if (importLeaveType != null)
                {
                    var types = await LeaveRepository.GetImportLeaveTypesAsync([importLeaveType], cancellationToken);
                    if (types.Count != 1 || types[0].LeaveTypeId != leaveTypeId)
                        return new(LeaveWriteStatus.Invalid, "Type of Leave no longer uniquely identifies the selected type. Review this row.");
                }
                else if (existing?.LeaveTypeId != leaveTypeId && !await LeaveRepository.LeaveTypeExistsAsync(leaveTypeId, cancellationToken))
                    return new(LeaveWriteStatus.Invalid, "Select an available Type of Leave.");
                if (await LeaveRepository.HasOverlapAsync(employeeId, from, to, id, cancellationToken))
                    return new(LeaveWriteStatus.Conflict, "This employee already has a Leave record overlapping this period.");

                var now = DateTime.UtcNow;
                var record = existing ?? new Leave { CreatedAt = now, CreatedBy = actor };
                record.EmployeeId = employeeId;
                record.LeaveTypeId = leaveTypeId;
                record.From = from;
                record.To = to;
                if (existing == null)
                    await LeaveRepository.InsertAsync(record, cancellationToken);
                else
                {
                    record.UpdatedAt = now;
                    record.UpdatedBy = actor;
                    await LeaveRepository.SaveAsync(cancellationToken);
                }
                return new(LeaveWriteStatus.Success, Id: record.LeaveId);
            }, cancellationToken);
        }

        private static string? Validate(LeaveWriteRequest request)
        {
            if (request.EmployeeId is null or <= 0) return "Employee is required.";
            if (request.LeaveTypeId is null or <= 0) return "Type of Leave is required.";
            if (request.FromDate is null || request.FromDate == DateOnly.MinValue ||
                request.ToDate is null || request.ToDate == DateOnly.MinValue)
                return "From and To dates are required.";
            if (request.FromTime is null || request.ToTime is null) return "From and To times are required.";
            if (request.FromTime.Value.Ticks % TimeSpan.TicksPerMinute != 0 || request.ToTime.Value.Ticks % TimeSpan.TicksPerMinute != 0)
                return "Enter times in whole minutes.";
            if (request.ToDate.Value.ToDateTime(request.ToTime.Value) <= request.FromDate.Value.ToDateTime(request.FromTime.Value))
                return "The end date and time must be later than the start date and time.";
            return null;
        }

        public Task<LeaveWriteResult> DeleteAsync(int id, string actor, CancellationToken cancellationToken = default)
        {
            if (id <= 0)
                return Task.FromResult(new LeaveWriteResult(LeaveWriteStatus.Invalid, "Select a valid record."));
            return LeaveRepository.ExecuteWriteAsync(async () =>
            {
                var record = await LeaveRepository.GetAsync(id, cancellationToken);
                if (record == null) return new(LeaveWriteStatus.NotFound, "Leave record was not found.");
                record.Deleted = true;
                record.UpdatedAt = DateTime.UtcNow;
                record.UpdatedBy = actor;
                await LeaveRepository.SaveAsync(cancellationToken);
                return new(LeaveWriteStatus.Success, Id: id);
            }, cancellationToken);
        }

        public Task<LeavePage> GetPageAsync(LeavePageQuery query, CancellationToken cancellationToken) =>
            LeaveRepository.GetPageAsync(query, cancellationToken);

        public Task<LeaveRow?> GetDetailsAsync(int id, CancellationToken cancellationToken) =>
            LeaveRepository.GetDetailsAsync(id, cancellationToken);

        public Task<LeaveEmployeeLookupPage> SearchEmployeesAsync(string term, int page, CancellationToken cancellationToken) =>
            LeaveRepository.SearchEmployeesAsync(term, page, cancellationToken);

        public Task<List<LeaveTypeLookupItem>> GetLeaveTypesAsync(CancellationToken cancellationToken) =>
            LeaveRepository.GetLeaveTypesAsync(cancellationToken);

    }
}
