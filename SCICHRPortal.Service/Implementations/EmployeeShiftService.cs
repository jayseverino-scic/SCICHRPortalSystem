using SCICHRPortal.Data.DTOs;
using SCICHRPortal.Data.Entities;
using SCICHRPortal.Repository.Interfaces;
using SCICHRPortal.Service.Interfaces;
using SCICHRPortal.Utility.Helpers;

namespace SCICHRPortal.Service.Implementations
{
    public class EmployeeShiftService : IEmployeeShiftService
    {
        public IEmployeeShiftRepository EmployeeShiftRepository { get; }

        public EmployeeShiftService(IEmployeeShiftRepository employeeShiftRepository)
        {
            EmployeeShiftRepository = employeeShiftRepository;
        }

        public async Task<bool> DeleteAsync(int id, string? actor = null)
        {
            return await EmployeeShiftRepository.DeleteAsync(id, actor);
        }

        public async Task<bool> UpdateAsync(EmployeeShift entity)
        {
            return await EmployeeShiftRepository.UpdateAsync(entity);
        }

        public async Task<bool> UpdateFlagsAsync(int assignedShiftId, EmployeeShiftAssignmentChange change, string actor)
        {
            if (assignedShiftId <= 0 || change.EmployeeId <= 0) return false;
            if (change.IsNoShift) change.IsFlexibleShift = false;
            return await EmployeeShiftRepository.UpdateFlagsAsync(assignedShiftId, change, actor);
        }

        public async Task<Tuple<IEnumerable<EmployeeShift>, int>> FilterAsync(int pageNumber, int pageSize, string searchKeyword)
        {
            return await EmployeeShiftRepository.FilterAsync(pageNumber, pageSize, searchKeyword);
        }

        public async Task<IEnumerable<EmployeeShift>> EmployeeShiftFilter(int departmentId, int shiftId)
        {
            return await EmployeeShiftRepository.EmployeeShiftFilter(departmentId, shiftId);
        }
        public async Task<IEnumerable<EmployeeShift>> EmployeeShiftFilterPerProject(int projectId, int shiftId)
        {
            return await EmployeeShiftRepository.EmployeeShiftFilterPerProject(projectId, shiftId);
        }
        public async Task<EmployeeShiftFilterPage> GetShiftFilterAsync(int projectId, int shiftId, string filterType, int? skip, int? take, string? searchKeyword, CancellationToken cancellationToken = default, DateTime? asOf = null)
        {
            return await EmployeeShiftRepository.GetShiftFilterAsync(projectId, shiftId, filterType, skip, take, searchKeyword, cancellationToken, asOf);
        }
        public async Task<EmployeeShift> GetAsync(int id)
        {
            return await EmployeeShiftRepository.GetAsync(id);
        }

        public async Task<EmployeeShiftAssignmentResult> AssignFilteredAsync(EmployeeShiftFilteredAssignmentRequest request, string actor, CancellationToken cancellationToken = default)
        {
            if (!request.EffectiveStartDate.HasValue || !PhilippineTime.IsLocal(request.EffectiveStartDate) ||
                !PhilippineTime.IsLocal(request.EffectiveEndDate) || !PhilippineTime.IsLocal(request.AsOf) ||
                (request.IsTemporary && (!request.EffectiveEndDate.HasValue || request.EffectiveEndDate <= request.EffectiveStartDate)) ||
                (!request.IsTemporary && request.EffectiveEndDate.HasValue) || request.ProjectId < 0 || request.ShiftId < 0 || request.ScheduleId < 0 ||
                (request.ApplyAssignmentToFilter && request.ScheduleId == null) ||
                request.FlagFilters == null || request.FlagFilters.Count > 3 ||
                request.FlagFilters.Any(filter => filter == null || !PhilippineTime.IsLocal(filter.AsOf) || filter.ProjectId < 0 || filter.ShiftId < 0 ||
                    filter.FilterType is not ("All" or "Assigned" or "Unassigned") || (filter.SearchKeyword?.Length ?? 0) > 200 ||
                    (filter.IsFlexibleShift == null && filter.IsNoShift == null && filter.IsNoBreak == null)) ||
                (!request.ApplyAssignmentToFilter && request.FlagFilters.Count == 0 && (request.Changes == null || request.Changes.Count == 0)) ||
                request.FilterType is not ("All" or "Assigned" or "Unassigned") ||
                (request.SearchKeyword?.Length ?? 0) > 200 || request.Changes == null ||
                request.Changes.Any(change => change == null || change.EmployeeId <= 0) ||
                (request.ScheduleId == null && request.Changes.Any(change => change.IsAssigned && !change.PreserveSchedule)) ||
                request.Changes.Select(change => change.EmployeeId).Distinct().Count() != request.Changes.Count)
            {
                return new(EmployeeShiftAssignmentStatus.Invalid, Message: "Enter valid filters, a schedule and unique employee edits.");
            }
            request.SearchKeyword = request.SearchKeyword?.Trim();
            foreach (var filter in request.FlagFilters) filter.SearchKeyword = filter.SearchKeyword?.Trim();
            foreach (var change in request.Changes)
            {
                if (request.ScheduleId == 0 && !change.PreserveSchedule) change.IsAssigned = false;
                if (change.IsNoShift)
                {
                    change.IsFlexibleShift = false;
                }
            }
            return await EmployeeShiftRepository.AssignFilteredAsync(request, actor, cancellationToken);
        }
        public async Task<IEnumerable<EmployeeShift>> GetAllAsync()
        {
            return await EmployeeShiftRepository.GetAllAsync();
        }
        public async Task<DuplicateMessage> HasDuplicateShift(EmployeeShift entity)
        {
            return await EmployeeShiftRepository.HasDuplicateShift(entity);
        }

        public async Task InsertAsync(EmployeeShift entity)
        {
            await EmployeeShiftRepository.InsertAsync(entity);
        }
        public async Task RemoveRangeAsync(List<EmployeeShift> employeeShifts)
        {
            await EmployeeShiftRepository.RemoveRangeAsync(employeeShifts);
        }
        public async Task UpdateRangeAsync(List<EmployeeShift> employeeShifts)
        {
            await EmployeeShiftRepository.UpdateRangeAsync(employeeShifts);
        }
        public async Task InsertRangeAsync(List<EmployeeShift> employeeShifts)
        {
            await EmployeeShiftRepository.InsertRangeAsync(employeeShifts);
        }
        public async Task<EmployeeShift?> GetByEmployee(int id)
        {
            return await EmployeeShiftRepository.GetByEmployee(id);
        }
        public Task<EmployeeShift?> GetAtAsync(int employeeId, DateTime timestamp, CancellationToken cancellationToken = default) =>
            EmployeeShiftRepository.GetAtAsync(employeeId, timestamp, cancellationToken);

        public Task<List<EmployeeShift>> GetPeriodsAsync(int[] employeeIds, DateTime from, DateTime until, CancellationToken cancellationToken = default) =>
            EmployeeShiftRepository.GetPeriodsAsync(employeeIds, from, until, cancellationToken);

        public Task<EmployeeShiftHistoryPage> GetHistoryAsync(int employeeId, int skip, int take, CancellationToken cancellationToken = default) =>
            EmployeeShiftRepository.GetHistoryAsync(employeeId, skip, take, cancellationToken);
    }
}
