using SCICHRPortal.Core.Interfaces;
using SCICHRPortal.Data.DTOs;
using SCICHRPortal.Data.Entities;
using SCICHRPortal.Utility.Interface;

namespace SCICHRPortal.Repository.Interfaces
{
    public interface IEmployeeShiftRepository : IRepository,
        IScopedService,
         IInserter<EmployeeShift>
    {
        Task<bool> DeleteAsync(int id, string? actor = null);
        Task<bool> UpdateAsync(EmployeeShift entity);
        Task<bool> UpdateFlagsAsync(int assignedShiftId, EmployeeShiftAssignmentChange change, string actor);
        Task<Tuple<IEnumerable<EmployeeShift>, int>> FilterAsync(int pageNumber, int pageSize, string searchKeyword);
        Task<IEnumerable<EmployeeShift>> EmployeeShiftFilter(int departmentId, int shiftId);
        Task<IEnumerable<EmployeeShift>> EmployeeShiftFilterPerProject(int projectId, int shiftId);
        Task<EmployeeShiftFilterPage> GetShiftFilterAsync(int projectId, int shiftId, string filterType, int? skip, int? take, string? searchKeyword, CancellationToken cancellationToken = default, DateTime? asOf = null);
        Task<EmployeeShiftAssignmentResult> AssignFilteredAsync(EmployeeShiftFilteredAssignmentRequest request, string actor, CancellationToken cancellationToken = default);
        Task<EmployeeShift> GetAsync(int id);
        Task<DuplicateMessage> HasDuplicateShift(EmployeeShift entity);
        Task<IEnumerable<EmployeeShift>> GetAllAsync();
        Task RemoveRangeAsync(List<EmployeeShift> employeeShifts);
        Task UpdateRangeAsync(List<EmployeeShift> employeeShifts);
        Task InsertRangeAsync(List<EmployeeShift> employeeShifts);
        Task<EmployeeShift?> GetByEmployee(int id);
        Task<EmployeeShift?> GetAtAsync(int employeeId, DateTime timestamp, CancellationToken cancellationToken = default);
        Task<List<EmployeeShift>> GetPeriodsAsync(int[] employeeIds, DateTime from, DateTime until, CancellationToken cancellationToken = default);
        Task<EmployeeShiftHistoryPage> GetHistoryAsync(int employeeId, int skip, int take, CancellationToken cancellationToken = default);
    }
}
