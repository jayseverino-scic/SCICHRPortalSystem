using SCICHRPortal.Core.Interfaces;
using SCICHRPortal.Data.DTOs;
using SCICHRPortal.Data.Entities;
using SCICHRPortal.Repository.Interfaces;
using SCICHRPortal.Service.Interfaces;

namespace SCICHRPortal.Service.Implementations
{
    public class RoleService : IRoleService
    {
        private IRoleRepository RoleRepository { get; }

        public RoleService(IRoleRepository roleRepository)
        {
            RoleRepository = roleRepository;
        }

        public Task<IEnumerable<Role>> FilterAsync(string filter)
        {
            throw new NotImplementedException();
        }

        public Task<Role> GetDuplicateAsync(Role role)
        {
            throw new NotImplementedException();
        }

        public Task<bool> HasDuplicateNameAsync(string name)
        {
            throw new NotImplementedException();
        }

        public async Task InsertAsync(Role entity)
        {
            await RoleRepository.InsertAsync(entity);
        }

        public async Task<bool> UpdateAsync(Role entity)
        {
            return await RoleRepository.UpdateAsync(entity);
        }

        public async Task<IEnumerable<Role>> GetAllAsync()
        {
            return await RoleRepository.GetAllAsync();
        }

        public async Task<Role> GetAsync(int id)
        {
            return await RoleRepository.GetAsync(id);
        }

        public async Task<bool> DeleteAsync(int roleId)
        {
            return await RoleRepository.DeleteAsync(roleId);
        }

        public async Task<Tuple<IEnumerable<Role>, int>> FilterAsync(int pageNumber, int pageSize, string searchKeyword)
        {
            return await RoleRepository.FilterAsync(pageNumber, pageSize, searchKeyword);
        }

        public async Task<DuplicateMessage> HasDuplicateName(Role role)
        {
            return await RoleRepository.HasDuplicateName(role);
        }
    }
}
