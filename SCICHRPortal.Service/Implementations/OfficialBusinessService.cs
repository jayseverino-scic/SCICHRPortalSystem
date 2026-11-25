using SCICHRPortal.Data.DTOs;
using SCICHRPortal.Data.Entities;
using SCICHRPortal.Repository.Interfaces;
using SCICHRPortal.Service.Interfaces;

namespace SCICHRPortal.Service.Implementations
{
    public partial class OfficialBusinessService : IOfficialBusinessService
    {
        private IOfficialBusinessRepository OfficialBusinessRepository { get; }

        public OfficialBusinessService(IOfficialBusinessRepository officialBusinessRepository)
        {
            OfficialBusinessRepository = officialBusinessRepository;
        }

        public Task<OfficialBusinessWriteResult> CreateAsync(OfficialBusinessWriteRequest request, string actor, CancellationToken cancellationToken = default)
        {
            return WriteAsync(null, request, actor, cancellationToken);
        }

        public Task<OfficialBusinessWriteResult> UpdateAsync(int id, OfficialBusinessWriteRequest request, string actor, CancellationToken cancellationToken = default)
        {
            return id <= 0 ? Task.FromResult(new OfficialBusinessWriteResult(OfficialBusinessWriteStatus.Invalid, "Select a valid record."))
                : WriteAsync(id, request, actor, cancellationToken);
        }

        private Task<OfficialBusinessWriteResult> WriteAsync(int? id, OfficialBusinessWriteRequest request, string actor, CancellationToken cancellationToken,
            string? importEmployeeNumber = null)
        {
            var error = Validate(request);
            if (error != null)
                return Task.FromResult(new OfficialBusinessWriteResult(OfficialBusinessWriteStatus.Invalid, error));

            var from = request.FromDate!.Value.ToDateTime(request.FromTime!.Value, DateTimeKind.Unspecified);
            var to = request.ToDate!.Value.ToDateTime(request.ToTime!.Value, DateTimeKind.Unspecified);
            return OfficialBusinessRepository.ExecuteWriteAsync(async () =>
            {
                var existing = id.HasValue ? await OfficialBusinessRepository.GetAsync(id.Value, cancellationToken) : null;
                if (id.HasValue && existing == null)
                    return new(OfficialBusinessWriteStatus.NotFound, "Official Business record was not found.");

                var employeeId = request.EmployeeId!.Value;
                if (importEmployeeNumber != null)
                {
                    // Preliminary batch lookups are only hints: recheck identity and uniqueness
                    // in this row's transaction so metadata edits cannot silently redirect an OB.
                    var employees = await OfficialBusinessRepository.GetImportEmployeesAsync([importEmployeeNumber], cancellationToken);
                    if (employees.Count != 1 || employees[0].EmployeeId != employeeId)
                        return new(OfficialBusinessWriteStatus.Invalid, "Employee No. no longer uniquely identifies the selected employee. Review this row.");
                }
                else
                {
                    if (existing?.EmployeeId != employeeId && !await OfficialBusinessRepository.EmployeeExistsAsync(employeeId, cancellationToken))
                        return new(OfficialBusinessWriteStatus.Invalid, "Select an available employee.");
                }
                if (await OfficialBusinessRepository.HasOverlapAsync(employeeId, from, to, id, cancellationToken))
                    return new(OfficialBusinessWriteStatus.Conflict, "This employee already has an Official Business record overlapping this period.");

                var now = DateTime.UtcNow;
                var changes = new OfficialBusiness
                {
                    EmployeeId = employeeId,
                    From = from,
                    To = to
                };

                if (existing == null)
                {
                    changes.CreatedAt = now;
                    changes.CreatedBy = actor;
                    await OfficialBusinessRepository.InsertAsync(changes, cancellationToken);
                    return new(OfficialBusinessWriteStatus.Success, Id: changes.OfficialBusinessId);
                }

                changes.UpdatedAt = now;
                changes.UpdatedBy = actor;
                await OfficialBusinessRepository.UpdateAsync(existing, changes, cancellationToken);
                return new(OfficialBusinessWriteStatus.Success, Id: existing.OfficialBusinessId);
            }, cancellationToken);
        }

        private static string? Validate(OfficialBusinessWriteRequest request)
        {
            if (request.EmployeeId is null or <= 0) return "Employee is required.";
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

        public Task<OfficialBusinessWriteResult> DeleteAsync(int id, string actor, CancellationToken cancellationToken = default)
        {
            if (id <= 0)
                return Task.FromResult(new OfficialBusinessWriteResult(OfficialBusinessWriteStatus.Invalid, "Select a valid record."));

            return OfficialBusinessRepository.ExecuteWriteAsync(async () =>
            {
                var record = await OfficialBusinessRepository.GetAsync(id, cancellationToken);
                if (record == null)
                    return new(OfficialBusinessWriteStatus.NotFound, "Official Business record was not found.");

                var changes = new OfficialBusiness
                {
                    Deleted = true,
                    UpdatedAt = DateTime.UtcNow,
                    UpdatedBy = actor
                };
                await OfficialBusinessRepository.DeleteAsync(record, changes, cancellationToken);
                return new(OfficialBusinessWriteStatus.Success, Id: id);
            }, cancellationToken);
        }

        public Task<OfficialBusinessPage> GetPageAsync(OfficialBusinessPageQuery query, CancellationToken cancellationToken) =>
            OfficialBusinessRepository.GetPageAsync(query, cancellationToken);

        public Task<OfficialBusinessRow?> GetDetailsAsync(int id, CancellationToken cancellationToken) =>
            OfficialBusinessRepository.GetDetailsAsync(id, cancellationToken);

        public Task<OfficialBusinessEmployeeLookupPage> SearchEmployeesAsync(string term, int page, CancellationToken cancellationToken) =>
            OfficialBusinessRepository.SearchEmployeesAsync(term, page, cancellationToken);

    }
}
