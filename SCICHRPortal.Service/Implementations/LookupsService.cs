using Microsoft.EntityFrameworkCore;
using SCICHRPortal.Data.DTOs;
using SCICHRPortal.Data.Entities;
using SCICHRPortal.Data.Entities.Metadatas;
using SCICHRPortal.Repository.Interfaces;
using SCICHRPortal.Service.Interfaces;

namespace SCICHRPortal.Service.Implementations
{
    public class LookupsService : ILookupsService
    {
        private ILookupsRepository LookupsRepository { get; }
        public LookupsService(ILookupsRepository lookupsRepository)
        {
            LookupsRepository = lookupsRepository;
        }

        public async Task<IEnumerable<T>> GetAll<T>() where T : BaseEntity
        {
            return await LookupsRepository.GetAll<T>();
        }

        public async Task InsertAsync<T>(T entity) where T : BaseEntity
        {
            await LookupsRepository.InsertAsync<T>(entity);
        }

        public async Task<bool> DeleteModuleTypeAsync(int moduleId)
        {
            return await LookupsRepository.DeleteModuleTypeAsync(moduleId);
        }

        public async Task<bool> UpdateAsync<T>(T entity) where T : BaseEntity
        {
            return await LookupsRepository.UpdateAsync<T>(entity);
        }

        public async Task<Tuple<IEnumerable<Module>, int>> FilterModuleAsync(int pageNumber, int pageSize, string searchKeyword)
        {
            return await LookupsRepository.FilterModuleAsync(pageNumber, pageSize, searchKeyword!);
        }

        public async Task<bool> HasDuplicateName(Module module)
        {
            return await LookupsRepository.HasDuplicateName(module);
        }
    }
}
