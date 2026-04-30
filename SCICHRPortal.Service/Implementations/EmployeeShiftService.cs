using SCICHRPortal.Data.DTOs;
using SCICHRPortal.Data.Entities;
using SCICHRPortal.Repository.Interfaces;
using SCICHRPortal.Service.Interfaces;

namespace SCICHRPortal.Service.Implementations
{
    public class EmployeeShiftService : IEmployeeShiftService
    {
        public IEmployeeShiftRepository EmployeeShiftRepository { get; }

        public EmployeeShiftService(IEmployeeShiftRepository employeeShiftRepository)
        {
            EmployeeShiftRepository = employeeShiftRepository;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            return await EmployeeShiftRepository.DeleteAsync(id);
        }

        public async Task<bool> UpdateAsync(EmployeeShift entity)
        {
            return await EmployeeShiftRepository.UpdateAsync(entity);
        }

        public async Task<Tuple<IEnumerable<EmployeeShift>, int>> FilterAsync(int pageNumber, int pageSize, string searchKeyword)
        {
            return await EmployeeShiftRepository.FilterAsync(pageNumber, pageSize, searchKeyword);
        }

        public async Task<IEnumerable<EmployeeShift>> EmployeeShiftFilter(int departmentId, int shiftId)
        {
            return await EmployeeShiftRepository.EmployeeShiftFilter(departmentId, shiftId);
        }
        public async Task<EmployeeShift> GetAsync(int id)
        {
            return await EmployeeShiftRepository.GetAsync(id);
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
        public async Task<EmployeeShift> GetByEmployee(int id)
        {
            return await EmployeeShiftRepository.GetByEmployee(id);
        }
    }
}
