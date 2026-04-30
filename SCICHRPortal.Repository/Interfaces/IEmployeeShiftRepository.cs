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
    public interface IEmployeeShiftRepository : IRepository,
        IScopedService,
         IInserter<EmployeeShift>
    {
        Task<bool> DeleteAsync(int id);
        Task<bool> UpdateAsync(EmployeeShift entity);
        Task<Tuple<IEnumerable<EmployeeShift>, int>> FilterAsync(int pageNumber, int pageSize, string searchKeyword);
        Task<IEnumerable<EmployeeShift>> EmployeeShiftFilter(int departmentId, int shiftId);
        Task<EmployeeShift> GetAsync(int id);
        Task<DuplicateMessage> HasDuplicateShift(EmployeeShift entity);
        Task<IEnumerable<EmployeeShift>> GetAllAsync();
        Task RemoveRangeAsync(List<EmployeeShift> employeeShifts);
        Task UpdateRangeAsync(List<EmployeeShift> employeeShifts);
        Task InsertRangeAsync(List<EmployeeShift> employeeShifts);
        Task<EmployeeShift> GetByEmployee(int id);
    }
}
