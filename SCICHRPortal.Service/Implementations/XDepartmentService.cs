using SCICHRPortal.Core.Interfaces;
using SCICHRPortal.Data.DTOs;
using SCICHRPortal.Data.XscribeTables;
using SCICHRPortal.Repository.Interfaces;
using SCICHRPortal.Service.Interfaces;

namespace SCICHRPortal.Service.Implementations
{
    public class XDepartmentService : IXDepartmentService
    {
        private IXDepartmentRepository DepartmentRepository { get; }

        public XDepartmentService(IXDepartmentRepository departmentRepository)
        {
            DepartmentRepository = departmentRepository;
        }

        public Task<IEnumerable<XDepartment>> FilterAsync(string filter)
        {
            throw new NotImplementedException();
        }

        public async Task<IEnumerable<XDepartment>> GetAllAsync()
        {
            return await DepartmentRepository.GetAllAsync();
        }

        public async Task<XDepartment> GetAsync(int id)
        {
            return await DepartmentRepository.GetAsync(id);
        }

        public async Task<Tuple<IEnumerable<XDepartment>, int>> FilterAsync(int pageNumber, int pageSize, string searchKeyword)
        {
            return await DepartmentRepository.FilterAsync(pageNumber, pageSize, searchKeyword);
        }
    }
}
