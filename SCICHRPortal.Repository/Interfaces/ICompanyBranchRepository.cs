using SCICHRPortal.Core.Interfaces;
using SCICHRPortal.Data.DTOs;
using SCICHRPortal.Data.Entities.Metadatas;
using SCICHRPortal.Data.XscribeTables;
using SCICHRPortal.Utility.Interface;
using System;
using System.Collections.Generic;
using System.Text;

namespace SCICHRPortal.Repository.Interfaces
{
    public interface ICompanyBranchRepository : IRepository,
        IScopedService,
         IRetriever<XCompany_Branch, int>,
         IListRetriever<XCompany_Branch>
    {
        Task<Tuple<IEnumerable<XCompany_Branch>, int>> FilterAsync(int pageNumber, int pageSize, string searchKeyword);
        Task<IEnumerable<XCompany_Branch>> GetBranches();
    }
}
