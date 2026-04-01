using SCICHRPortal.Core.Interfaces;
using SCICHRPortal.Data.DTOs;
using SCICHRPortal.Data.Entities;
using SCICHRPortal.Data.Entities.Metadatas;
using SCICHRPortal.Repository.Interfaces;
using SCICHRPortal.Service.Interfaces;

namespace SCICHRPortal.Service.Implementations
{
    public class ShiftService : IShiftService
    {
        private IShiftRepository ShiftRepository { get; }

        public ShiftService(IShiftRepository shiftRepository)
        {
            ShiftRepository = shiftRepository;
        }

        public Task<IEnumerable<Shift>> FilterAsync(string filter)
        {
            throw new NotImplementedException();
        }

        public Task<Shift> GetDuplicateAsync(Shift shift)
        {
            throw new NotImplementedException();
        }

        public Task<bool> HasDuplicateNameAsync(string name)
        {
            throw new NotImplementedException();
        }

        public async Task InsertAsync(Shift entity)
        {
            await ShiftRepository.InsertAsync(entity);
        }

        public async Task<bool> UpdateAsync(Shift entity)
        {
            return await ShiftRepository.UpdateAsync(entity);
        }

        public async Task<IEnumerable<Shift>> GetAllAsync()
        {
            return await ShiftRepository.GetAllAsync();
        }

        public async Task<Shift> GetAsync(int id)
        {
            return await ShiftRepository.GetAsync(id);
        }

        public async Task<bool> DeleteAsync(int shiftId)
        {
            return await ShiftRepository.DeleteAsync(shiftId);
        }

        public async Task<Tuple<IEnumerable<Shift>, int>> FilterAsync(int pageNumber, int pageSize, string searchKeyword)
        {
            return await ShiftRepository.FilterAsync(pageNumber, pageSize, searchKeyword);
        }

        public async Task<DuplicateMessage> HasDuplicateName(Shift shift)
        {
            return await ShiftRepository.HasDuplicateName(shift);
        }
    }
}
