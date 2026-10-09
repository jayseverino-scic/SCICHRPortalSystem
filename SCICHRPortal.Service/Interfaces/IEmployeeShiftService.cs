using SCICHRPortal.Core.Interfaces;
using SCICHRPortal.Data.DTOs;
using SCICHRPortal.Data.Entities;
using SCICHRPortal.Utility.Interface;

namespace SCICHRPortal.Service.Interfaces
{
    public interface IEmployeeShiftService : IScopedService,
       IInserter<EmployeeShift>
    {
        Task<bool> DeleteAsync(int id);
        Task<bool> UpdateAsync(EmployeeShift entity);
        Task<bool> UpdateFlagsAsync(int assignedShiftId, EmployeeShiftAssignmentChange change, string actor);
        Task<Tuple<IEnumerable<EmployeeShift>, int>> FilterAsync(int pageNumber, int pageSize, string searchKeyword);
        Task<IEnumerable<EmployeeShift>> EmployeeShiftFilter(int departmentId, int shiftId);
        Task<IEnumerable<EmployeeShift>> EmployeeShiftFilterPerProject(int departmentId, int shiftId);
        Task<EmployeeShiftFilterPage> GetShiftFilterAsync(int projectId, int shiftId, string filterType, int? skip, int? take, string? searchKeyword, CancellationToken cancellationToken = default);
        Task<EmployeeShiftAssignmentResult> AssignFilteredAsync(EmployeeShiftFilteredAssignmentRequest request, string actor, CancellationToken cancellationToken = default);
        Task<EmployeeShift> GetAsync(int id);
        Task<DuplicateMessage> HasDuplicateShift(EmployeeShift entity);
        Task<IEnumerable<EmployeeShift>> GetAllAsync();
        Task RemoveRangeAsync(List<EmployeeShift> employeeShifts);
        Task UpdateRangeAsync(List<EmployeeShift> employeeShifts);
        Task InsertRangeAsync(List<EmployeeShift> employeeShifts);
        Task<EmployeeShift> GetByEmployee(int id);
    }
}
