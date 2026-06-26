using SCICHRPortal.Core.Interfaces;
using SCICHRPortal.Data.DTOs;
using SCICHRPortal.Data.XscribeTables;
using SCICHRPortal.Utility.Interface;

namespace SCICHRPortal.Service.Interfaces
{
    public interface IXDepartmentService :
        IScopedService,
         IRetriever<XDepartment, int>,
         IListRetriever<XDepartment>
    {
        Task<Tuple<IEnumerable<XDepartment>, int>> FilterAsync(int pageNumber, int pageSize, string searchKeyword);
    }
}
