using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SCICHRPortal.Core.Interfaces;
using SCICHRPortal.Data.DTOs;
using SCICHRPortal.Data.Entities;
using SCICHRPortal.Utility.Interface;

namespace SCICHRPortal.Repository.Interfaces
{
    public interface IEmployeeShiftDeviceRepository : IRepository,
        IScopedService,
         IInserter<EmployeeShiftDevice>
    {
        Task<bool> DeleteAsync(int id);
        Task<bool> UpdateAsync(EmployeeShiftDevice entity);
        Task<Tuple<IEnumerable<EmployeeShiftDevice>, int>> FilterAsync(int pageNumber, int pageSize, string searchKeyword);
        Task<IEnumerable<EmployeeShiftDevice>> EmployeeShiftDeviceFilter(string? deviceName, int shiftId);
        Task<EmployeeShiftDevice> GetAsync(int id);
        Task<DuplicateMessage> HasDuplicateShift(EmployeeShiftDevice entity);
        Task<IEnumerable<EmployeeShiftDevice>> GetAllAsync();
        Task RemoveRangeAsync(List<EmployeeShiftDevice> employeeShifts);
        Task UpdateRangeAsync(List<EmployeeShiftDevice> employeeShifts);
        Task InsertRangeAsync(List<EmployeeShiftDevice> employeeShifts);
        Task<EmployeeShiftDevice> GetByEmployeeDevice(int id);
    }
}
