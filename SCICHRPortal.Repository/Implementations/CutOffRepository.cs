using Microsoft.EntityFrameworkCore;
using System.Data;
using SCICHRPortal.Data.DTOs;
using SCICHRPortal.Data.Entities;
using SCICHRPortal.Data.Entities.Metadatas;
using SCICHRPortal.Repository.Interfaces;
using SCICHRPortal.Utility.Extensions;

namespace SCICHRPortal.Repository.Implementations
{
    public class CutOffRepository : Repository, ICutOffRepository
    {
        public CutOffRepository(ApplicationContext context)
    : base(context)
        {
        }
        public async Task<bool> DeleteAsync(int cutOffId)
        {
            var cutOff = await Context.CutOff!
                        .SingleOrDefaultAsync(s => s.CutOffId == cutOffId && !s.Deleted);
            if (cutOff == null)
                return false;

            cutOff.Deleted = true;
            await Context.SaveChangesAsync();
            return true;
        }

        public async Task<Tuple<IEnumerable<CutOff>, int>> FilterAsync(int pageNumber, int pageSize, string searchKeyword)
        {
            var cutOffs = Context.CutOff!
              .Where(e => e.Deleted == false);


            var total = cutOffs.Count();

            cutOffs = cutOffs
                .OrderByDescending(e => e.CutOffId)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize);

            return new Tuple<IEnumerable<CutOff>, int>(await cutOffs.ToListAsync(), total);
        }

        public async Task<IEnumerable<CutOff>> GetAllAsync()
        {
            var cutOffs = await Context.CutOff!.Where(s => !s.Deleted)
              .ToListAsync();
            return cutOffs;
        }

        public async Task<CutOff> GetAsync(int id)
        {
            var cutOff = await Context.CutOff!
                    .SingleOrDefaultAsync(s => s.CutOffId == id && !s.Deleted);
            return cutOff!;
        }

        public async Task<DuplicateMessage> HasDuplicateName(CutOff cutOff)
        {
            DuplicateMessage message = new();
            bool conflictStartAndEnd = false;
            var cutOffs = await Context.CutOff!
               .Where(r => r.Deleted == false).ToListAsync();

            var conflictCutOffStart = cutOffs.Any(c => c.StartDate >= cutOff.StartDate && c.EndDate <= cutOff.StartDate);
            var conflictCutOffEnd = cutOffs.Any(c => c.StartDate >= cutOff.EndDate && c.EndDate <= cutOff.EndDate);
            if (conflictCutOffStart)
            {
                message.Message = "Conflict Cut Off Start Date with other Cut Off Dates";
            }
            if (conflictCutOffEnd)
            {
                message.Message = "Conflict Cut Off End Date with other Cut Off Dates";
            }
            if (cutOff.StartDate == cutOff.EndDate || cutOff.StartDate > cutOff.EndDate)
            {
                message.Message = "Start and End Date are conflict. Either they have equal values or Start Date is greater than End Date.";
                conflictStartAndEnd = true;
            }
            else
            {
                conflictStartAndEnd = false;
            }

            message.IsDuplicated = (conflictCutOffStart || conflictCutOffEnd || conflictStartAndEnd);
            return message;
        }

        public async Task InsertAsync(CutOff entity)
        {
            await Context.CutOff!.AddAsync(entity);
            await Context.SaveChangesAsync();
        }

        public async Task<bool> UpdateAsync(CutOff cutOff)
        {
            var record = Context.Update(cutOff);
            if (record is null)
                return false;

            await Context.SaveChangesAsync();
            return true;
        }
    }
}
