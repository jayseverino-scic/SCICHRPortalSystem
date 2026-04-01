using SCICHRPortal.Data.DTOs;
using SCICHRPortal.Data.Entities;
using SCICHRPortal.Data.Entities.Metadatas;
using SCICHRPortal.Utility.Interface;

namespace SCICHRPortal.Service.Interfaces
{
    public interface ILookupsService : IScopedService
    {
        Task<IEnumerable<T>> GetAll<T>() where T : BaseEntity;
        Task InsertAsync<T>(T entity) where T : BaseEntity;
        Task<bool> UpdateAsync<T>(T entity) where T : BaseEntity;
       Task<bool> DeleteModuleTypeAsync(int moduleId);
               Task<Tuple<IEnumerable<Module>, int>> FilterModuleAsync(int pageNumber, int pageSize, string searchKeyword);
       
        Task<bool> HasDuplicateName(Module module);
       
    }
}
