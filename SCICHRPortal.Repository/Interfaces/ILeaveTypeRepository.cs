using SCICHRPortal.Core.Interfaces;
using SCICHRPortal.Data.DTOs;
using SCICHRPortal.Data.Entities.Metadatas;
using SCICHRPortal.Utility.Interface;

namespace SCICHRPortal.Repository.Interfaces
{
    public interface ILeaveTypeRepository : IRepository,
        IScopedService,
         IInserter<LeaveType>,
         IRetriever<LeaveType, int>,
         IListRetriever<LeaveType>
    {
        Task<bool> DeleteAsync(int typeId);
        Task<bool> UpdateAsync(LeaveType leaveType);
        Task<Tuple<IEnumerable<LeaveType>, int>> FilterAsync(int pageNumber, int pageSize, string searchKeyword);
        Task<DuplicateMessage> HasDuplicateName(LeaveType leaveType);
    }
}
