using SCICHRPortal.Core.Interfaces;
using SCICHRPortal.Data.DTOs;
using SCICHRPortal.Data.Entities;
using SCICHRPortal.Data.Entities.Metadatas;
using SCICHRPortal.Repository.Interfaces;
using SCICHRPortal.Service.Interfaces;


namespace SCICHRPortal.Service.Implementations
{
    public class CutOffService : ICutOffService
    {
        private ICutOffRepository CutOffRepository { get; }

        public CutOffService(ICutOffRepository cutOffRepository)
        {
            CutOffRepository = cutOffRepository;
        }

        public Task<IEnumerable<CutOff>> FilterAsync(string filter)
        {
            throw new NotImplementedException();
        }

        public Task<CutOff> GetDuplicateAsync(CutOff cutOff)
        {
            throw new NotImplementedException();
        }

        public Task<bool> HasDuplicateNameAsync(string name)
        {
            throw new NotImplementedException();
        }

        public async Task InsertAsync(CutOff entity)
        {
            await CutOffRepository.InsertAsync(entity);
        }

        public async Task<bool> UpdateAsync(CutOff entity)
        {
            return await CutOffRepository.UpdateAsync(entity);
        }

        public async Task<IEnumerable<CutOff>> GetAllAsync()
        {
            return await CutOffRepository.GetAllAsync();
        }

        public async Task<CutOff> GetAsync(int id)
        {
            return await CutOffRepository.GetAsync(id);
        }

        public async Task<bool> DeleteAsync(int cutOffId)
        {
            return await CutOffRepository.DeleteAsync(cutOffId);
        }

        public async Task<Tuple<IEnumerable<CutOff>, int>> FilterAsync(int pageNumber, int pageSize, string searchKeyword)
        {
            return await CutOffRepository.FilterAsync(pageNumber, pageSize, searchKeyword);
        }

        public async Task<DuplicateMessage> HasDuplicateName(CutOff cutOff)
        {
            return await CutOffRepository.HasDuplicateName(cutOff);
        }
    }
}
