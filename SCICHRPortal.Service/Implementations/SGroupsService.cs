using SCICHRPortal.Core.Interfaces;
using SCICHRPortal.Data.DTOs;
using SCICHRPortal.Repository.Interfaces;
using SCICHRPortal.Service.Interfaces;
using SCICHRPortal.Data.TimekeepingTables;

namespace SCICHRPortal.Service.Implementations
{
    public class SGroupsService : ISGroupsService
    {
        private ISGroupsRepository GroupsRepository { get; }

        public SGroupsService(ISGroupsRepository groupsRepository)
        {
            GroupsRepository = groupsRepository;
        }

        public Task<IEnumerable<SGroups>> FilterAsync(string filter)
        {
            throw new NotImplementedException();
        }

        public async Task<IEnumerable<SGroups>> GetAllAsync()
        {
            return await GroupsRepository.GetAllAsync();
        }

        public async Task<SGroups> GetAsync(Guid id)
        {
            return await GroupsRepository.GetAsync(id);
        }

        public async Task<Tuple<IEnumerable<SGroups>, int>> FilterAsync(int pageNumber, int pageSize, string searchKeyword)
        {
            return await GroupsRepository.FilterAsync(pageNumber, pageSize, searchKeyword);
        }
    }
}
