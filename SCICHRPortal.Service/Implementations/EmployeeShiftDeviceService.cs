using SCICHRPortal.Data.DTOs;
using SCICHRPortal.Data.Entities;
using SCICHRPortal.Repository.Interfaces;
using SCICHRPortal.Service.Interfaces;

namespace SCICHRPortal.Service.Implementations
{
    public class EmployeeShiftDeviceService : IEmployeeShiftDeviceService
    {
        public IEmployeeShiftDeviceRepository EmployeeShiftDeviceRepository { get; }

        public EmployeeShiftDeviceService(IEmployeeShiftDeviceRepository employeeShiftRepository)
        {
            EmployeeShiftDeviceRepository = employeeShiftRepository;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            return await EmployeeShiftDeviceRepository.DeleteAsync(id);
        }

        public async Task<bool> UpdateAsync(EmployeeShiftDevice entity)
        {
            return await EmployeeShiftDeviceRepository.UpdateAsync(entity);
        }

        public async Task<Tuple<IEnumerable<EmployeeShiftDevice>, int>> FilterAsync(int pageNumber, int pageSize, string searchKeyword)
        {
            return await EmployeeShiftDeviceRepository.FilterAsync(pageNumber, pageSize, searchKeyword);
        }

        public async Task<IEnumerable<EmployeeShiftDevice>> EmployeeShiftDeviceFilter(string? deviceName, int shiftId)
        {
            return await EmployeeShiftDeviceRepository.EmployeeShiftDeviceFilter(deviceName, shiftId);
        }
        public async Task<EmployeeShiftDevice> GetAsync(int id)
        {
            return await EmployeeShiftDeviceRepository.GetAsync(id);
        }
        public async Task<IEnumerable<EmployeeShiftDevice>> GetAllAsync()
        {
            return await EmployeeShiftDeviceRepository.GetAllAsync();
        }
        public async Task<DuplicateMessage> HasDuplicateShift(EmployeeShiftDevice entity)
        {
            return await EmployeeShiftDeviceRepository.HasDuplicateShift(entity);
        }

        public async Task InsertAsync(EmployeeShiftDevice entity)
        {
            await EmployeeShiftDeviceRepository.InsertAsync(entity);
        }
        public async Task RemoveRangeAsync(List<EmployeeShiftDevice> employeeShifts)
        {
            await EmployeeShiftDeviceRepository.RemoveRangeAsync(employeeShifts);
        }
        public async Task UpdateRangeAsync(List<EmployeeShiftDevice> employeeShifts)
        {
            await EmployeeShiftDeviceRepository.UpdateRangeAsync(employeeShifts);
        }
        public async Task InsertRangeAsync(List<EmployeeShiftDevice> employeeShifts)
        {
            await EmployeeShiftDeviceRepository.InsertRangeAsync(employeeShifts);
        }
        public async Task<EmployeeShiftDevice> GetByEmployeeDevice(int id)
        {
            return await EmployeeShiftDeviceRepository.GetByEmployeeDevice(id);
        }
    }
}
