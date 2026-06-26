using SCICHRPortal.Core.Interfaces;
using SCICHRPortal.Data.DTOs;
using SCICHRPortal.Data.XscribeTables;
using SCICHRPortal.Utility.Interface;

namespace SCICHRPortal.Service.Interfaces
{
    public interface IXCompanyPositionService :
        IScopedService,
         IRetriever<XCompany_Position, int>,
         IListRetriever<XCompany_Position>
    {
        Task<Tuple<IEnumerable<XCompany_Position>, int>> FilterAsync(int pageNumber, int pageSize, string searchKeyword);
    }
}
