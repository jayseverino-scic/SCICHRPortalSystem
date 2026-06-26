using Microsoft.EntityFrameworkCore;
using SCICHRPortal.Data.DTOs;
using SCICHRPortal.Data.Entities.Metadatas;
using SCICHRPortal.Data.XscribeTables;
using SCICHRPortal.Repository.Interfaces;
using SCICHRPortal.Utility.Extensions;
using System.Data;


namespace SCICHRPortal.Repository.Implementations
{
    public class XCompanyPositionRepository : Repository, IXCompanyPositionRepository
    {
        public XCompanyPositionRepository(ApplicationContext context, XscribeContext xscribeContext, TimekeepingContext timekeepingContext)
    : base(context, xscribeContext, timekeepingContext)
        {
        }

        public async Task<Tuple<IEnumerable<XCompany_Position>, int>> FilterAsync(int pageNumber, int pageSize, string searchKeyword)
        {
            var positions = XscribeContext.Company_Position!
              .Where(e => e.Name != null);

            if (!String.IsNullOrWhiteSpace(searchKeyword))
            {
                positions = positions
                    .Where(e =>
                        e.Name!.ToLower().Contains(searchKeyword.ToLower()));
            }

            var total = positions.Count();

            positions = positions
                .OrderByDescending(e => e.Id)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize);

            return new Tuple<IEnumerable<XCompany_Position>, int>(await positions.ToListAsync(), total);
        }

        public async Task<IEnumerable<XCompany_Position>> GetAllAsync()
        {
            var positions = await XscribeContext.Company_Position!.Where(s => s.Name != null)
              .ToListAsync();
            return positions;
        }

        public async Task<XCompany_Position> GetAsync(int id)
        {
            var position = await XscribeContext.Company_Position!
                    .SingleOrDefaultAsync(s => s.Id == id && s.Name != null);
            return position!;
        }
    }
}
