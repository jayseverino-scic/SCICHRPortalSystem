using SCICHRPortal.Core.Interfaces;
using SCICHRPortal.Data.DTOs;
using SCICHRPortal.Data.Entities;
using SCICHRPortal.Data.Entities.Metadatas;
using SCICHRPortal.Repository.Interfaces;
using SCICHRPortal.Service.Interfaces;

namespace SCICHRPortal.Service.Implementations
{
    public class LeaveTypeService : ILeaveTypeService
    {
        private ILeaveTypeRepository LeaveTypeRepository { get; }

        public LeaveTypeService(ILeaveTypeRepository leaveTypeRepository)
        {
            LeaveTypeRepository = leaveTypeRepository;
        }

        public Task<IEnumerable<LeaveType>> FilterAsync(string filter)
        {
            throw new NotImplementedException();
        }

        public Task<LeaveType> GetDuplicateAsync(LeaveType leaveType)
        {
            throw new NotImplementedException();
        }

        public Task<bool> HasDuplicateNameAsync(string name)
        {
            throw new NotImplementedException();
        }

        public async Task InsertAsync(LeaveType entity)
        {
            await LeaveTypeRepository.InsertAsync(entity);
        }

        public async Task<bool> UpdateAsync(LeaveType entity)
        {
            return await LeaveTypeRepository.UpdateAsync(entity);
        }

        public async Task<IEnumerable<LeaveType>> GetAllAsync()
        {
            return await LeaveTypeRepository.GetAllAsync();
        }

        public async Task<LeaveType> GetAsync(int id)
        {
            return await LeaveTypeRepository.GetAsync(id);
        }

        public async Task<bool> DeleteAsync(int typeId)
        {
            return await LeaveTypeRepository.DeleteAsync(typeId);
        }

        public async Task<Tuple<IEnumerable<LeaveType>, int>> FilterAsync(int pageNumber, int pageSize, string searchKeyword)
        {
            return await LeaveTypeRepository.FilterAsync(pageNumber, pageSize, searchKeyword);
        }

        public async Task<DuplicateMessage> HasDuplicateName(LeaveType leaveType)
        {
            return await LeaveTypeRepository.HasDuplicateName(leaveType);
        }
    }
}
