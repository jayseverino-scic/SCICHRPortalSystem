using SCICHRPortal.Core.Interfaces;
using SCICHRPortal.Data.DTOs;
using SCICHRPortal.Data.Entities.Metadatas;
using SCICHRPortal.Utility.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCICHRPortal.Service.Interfaces
{
    public interface ILeaveTypeService :
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
