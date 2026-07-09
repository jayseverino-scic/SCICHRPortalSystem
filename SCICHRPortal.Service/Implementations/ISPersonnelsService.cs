using SCICHRPortal.Core.Interfaces;
using SCICHRPortal.Data.DTOs;
using SCICHRPortal.Data.TimekeepingTables;
using SCICHRPortal.Repository.Interfaces;
using SCICHRPortal.Service.Interfaces;
namespace SCICHRPortal.Service.Implementations
{
    public class SPersonnelsService : ISPersonnelsService
    {
        private ISPersonnelsRepository SPersonnelsRepository { get; }

        public SPersonnelsService(ISPersonnelsRepository employeeRepository)
        {
            SPersonnelsRepository = employeeRepository;
        }

        public Task<IEnumerable<SPersonnels>> FilterAsync(string filter)
        {
            throw new NotImplementedException();
        }

        public Task<SPersonnels> GetDuplicateAsync(SPersonnels SPersonnels)
        {
            throw new NotImplementedException();
        }

        public async Task<IEnumerable<SPersonnels>> GetAllAsync()
        {
            return await SPersonnelsRepository.GetAllAsync();
        }

        public async Task<SPersonnels> GetAsync(Guid id)
        {
            return await SPersonnelsRepository.GetAsync(id);
        }

        public async Task<Tuple<IEnumerable<SPersonnels>, int>> FilterAsync(int pageNumber, int pageSize, string searchKeyword)
        {
            return await SPersonnelsRepository.FilterAsync(pageNumber, pageSize, searchKeyword);
        }

        public async Task<SPersonnels> GetBySPersonnelsNoAsync(string employeeNo)
        {
            return await SPersonnelsRepository.GetBySPersonnelsNoAsync(employeeNo);
        }
    }
}
