using SCICHRPortal.Core.Interfaces;
using SCICHRPortal.Data.DTOs;
using SCICHRPortal.Data.XscribeTables;
using SCICHRPortal.Repository.Interfaces;
using SCICHRPortal.Service.Interfaces;
namespace SCICHRPortal.Service.Implementations
{
    public class XEmployeeService : IXEmployeeService
    {
        private IXEmployeeRepository EmployeeRepository { get; }

        public XEmployeeService(IXEmployeeRepository employeeRepository)
        {
            EmployeeRepository = employeeRepository;
        }

        public Task<IEnumerable<XEmployee>> FilterAsync(string filter)
        {
            throw new NotImplementedException();
        }
        public async Task<IEnumerable<XEmployee>> GetAllAsync()
        {
            return await EmployeeRepository.GetAllAsync();
        }

        public async Task<XEmployee> GetAsync(int id)
        {
            return await EmployeeRepository.GetAsync(id);
        }

        public async Task<Tuple<IEnumerable<XEmployee>, int>> FilterAsync(int pageNumber, int pageSize, string searchKeyword)
        {
            return await EmployeeRepository.FilterAsync(pageNumber, pageSize, searchKeyword);
        }

        public async Task<IEnumerable<XEmployee>> GetEmployeeByDepartment(int departmentId)
        {
            return await EmployeeRepository.GetEmployeeByDepartment(departmentId);
        }

        public async Task<XEmployee> GetByEmployeeNoAsync(string employeeNo)
        {
            return await EmployeeRepository.GetByEmployeeNoAsync(employeeNo);
        }
    }
}
