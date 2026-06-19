using SCICHRPortal.Core.Interfaces;
using SCICHRPortal.Data.XscribeTables;
using SCICHRPortal.Repository.Interfaces;
using SCICHRPortal.Utility.Interface;
using System;
using System.Collections.Generic;
using System.Text;

namespace SCICHRPortal.Service.Interfaces
{
    public interface ICompanyBranchService : 
        IScopedService,
         IRetriever<Company_Branch, int>,
         IListRetriever<Company_Branch>
    {
        Task<Tuple<IEnumerable<Company_Branch>, int>> FilterAsync(int pageNumber, int pageSize, string searchKeyword);
        Task<IEnumerable<Company_Branch>> GetBranches();
    }
}
