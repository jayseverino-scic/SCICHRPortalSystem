using SCICHRPortal.Core.Interfaces;
using SCICHRPortal.Data.DTOs;
using SCICHRPortal.Data.Entities;
using SCICHRPortal.Data.Entities.Metadatas;
using SCICHRPortal.Repository.Interfaces;
using SCICHRPortal.Service.Interfaces;

namespace SCICHRPortal.Service.Implementations
{
    public class PositionService : IPositionService
    {
        private IPositionRepository PositionRepository { get; }

        public PositionService(IPositionRepository positionRepository)
        {
            PositionRepository = positionRepository;
        }

        public Task<IEnumerable<Position>> FilterAsync(string filter)
        {
            throw new NotImplementedException();
        }

        public Task<Position> GetDuplicateAsync(Position position)
        {
            throw new NotImplementedException();
        }

        public Task<bool> HasDuplicateNameAsync(string name)
        {
            throw new NotImplementedException();
        }

        public async Task InsertAsync(Position entity)
        {
            await PositionRepository.InsertAsync(entity);
        }

        public async Task<bool> UpdateAsync(Position entity)
        {
            return await PositionRepository.UpdateAsync(entity);
        }

        public async Task<IEnumerable<Position>> GetAllAsync()
        {
            return await PositionRepository.GetAllAsync();
        }

        public async Task<Position> GetAsync(int id)
        {
            return await PositionRepository.GetAsync(id);
        }

        public async Task<bool> DeleteAsync(int categoryId)
        {
            return await PositionRepository.DeleteAsync(categoryId);
        }

        public async Task<Tuple<IEnumerable<Position>, int>> FilterAsync(int pageNumber, int pageSize, string searchKeyword)
        {
            return await PositionRepository.FilterAsync(pageNumber, pageSize, searchKeyword);
        }

        public async Task<DuplicateMessage> HasDuplicateName(Position position)
        {
            return await PositionRepository.HasDuplicateName(position);
        }
    }
}
