using Microsoft.EntityFrameworkCore;
using SCICHRPortal.Data.DTOs;
using SCICHRPortal.Data.Entities.Metadatas;
using SCICHRPortal.Data.XscribeTables;
using SCICHRPortal.Repository.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace SCICHRPortal.Repository.Implementations
{
    public class CompanyBranchRepository : Repository, ICompanyBranchRepository
    {
        public CompanyBranchRepository(ApplicationContext context, XscribeContext xscribeContext, TimekeepingContext timekeepingContext)
    : base(context, xscribeContext, timekeepingContext)
        {

        }
        public async Task<Tuple<IEnumerable<XCompany_Branch>, int>> FilterAsync(int pageNumber, int pageSize, string searchKeyword)
        {
            var CompanyBranchs = XscribeContext.Company_Branch!
              .Where(e => e._Deleted == false);

            if (!String.IsNullOrWhiteSpace(searchKeyword))
            {
                CompanyBranchs = CompanyBranchs
                    .Where(e =>
                        e.Name!.ToLower().Contains(searchKeyword.ToLower()));
            }

            var total = CompanyBranchs.Count();

            CompanyBranchs = CompanyBranchs
                .OrderByDescending(e => e.Id)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize);

            return new Tuple<IEnumerable<XCompany_Branch>, int>(await CompanyBranchs.ToListAsync(), total);
        }

        public async Task<IEnumerable<XCompany_Branch>> GetAllAsync()
        {
            var CompanyBranchs = await XscribeContext.Company_Branch!.Where(s => !s._Deleted)
              .ToListAsync();
            return CompanyBranchs;
        }

        public async Task<XCompany_Branch> GetAsync(int id)
        {
            var Company_Branch = await XscribeContext.Company_Branch!.SingleOrDefaultAsync(s => s.Id == id && !s._Deleted);
            return Company_Branch!;
        }
    
        public async Task<IEnumerable<XCompany_Branch>> GetBranches()
        {
            IEnumerable<XCompany_Branch> devices;

            devices = await XscribeContext.Company_Branch!.ToListAsync();

            return devices;
        }
    }
}
