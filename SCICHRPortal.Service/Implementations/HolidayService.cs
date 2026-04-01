using SCICHRPortal.Core.Interfaces;
using SCICHRPortal.Data.DTOs;
using SCICHRPortal.Data.Entities;
using SCICHRPortal.Data.Entities.Metadatas;
using SCICHRPortal.Repository.Interfaces;
using SCICHRPortal.Service.Interfaces;

namespace SCICHRPortal.Service.Implementations
{
    public class HolidayService : IHolidayService
    {
        private IHolidayRepository HolidayRepository { get; }

        public HolidayService(IHolidayRepository holidayRepository)
        {
            HolidayRepository = holidayRepository;
        }

        public Task<IEnumerable<Holiday>> FilterAsync(string filter)
        {
            throw new NotImplementedException();
        }

        public Task<Holiday> GetDuplicateAsync(Holiday holiday)
        {
            throw new NotImplementedException();
        }

        public Task<bool> HasDuplicateNameAsync(string name)
        {
            throw new NotImplementedException();
        }

        public async Task InsertAsync(Holiday entity)
        {
            await HolidayRepository.InsertAsync(entity);
        }

        public async Task<bool> UpdateAsync(Holiday entity)
        {
            return await HolidayRepository.UpdateAsync(entity);
        }

        public async Task<IEnumerable<Holiday>> GetAllAsync()
        {
            return await HolidayRepository.GetAllAsync();
        }

        public async Task<Holiday> GetAsync(int id)
        {
            return await HolidayRepository.GetAsync(id);
        }

        public async Task<bool> DeleteAsync(int holidayId)
        {
            return await HolidayRepository.DeleteAsync(holidayId);
        }

        public async Task<Tuple<IEnumerable<Holiday>, int>> FilterAsync(int pageNumber, int pageSize, string searchKeyword)
        {
            return await HolidayRepository.FilterAsync(pageNumber, pageSize, searchKeyword);
        }

        public async Task<DuplicateMessage> HasDuplicateName(Holiday holiday)
        {
            return await HolidayRepository.HasDuplicateName(holiday);
        }
    }
}
