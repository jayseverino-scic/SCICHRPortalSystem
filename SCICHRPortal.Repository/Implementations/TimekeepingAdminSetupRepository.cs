using Microsoft.EntityFrameworkCore;
using System.Data;
using SCICHRPortal.Data.DTOs;
using SCICHRPortal.Data.Entities.Metadatas;
using SCICHRPortal.Repository.Interfaces;
namespace SCICHRPortal.Repository.Implementations
{
    public class TimekeepingAdminSetupRepository : Repository, ITimekeepingAdminSetupRepository
    {
        public TimekeepingAdminSetupRepository(ApplicationContext context)
: base(context)
        {
        }

        public async Task<TimekeepingAdminSetup> GetAsync(int id)
        {
            var TimekeepingAdminSetup = await Context.TimekeepingAdminSetup!
                    .SingleOrDefaultAsync(s => s.SetupId == id && !s.Deleted);
            return TimekeepingAdminSetup!;
        }

        public async Task<TimekeepingAdminSetup> GetFirstOrDefault()
        {
            var TimeKeepingAdminSetup = await Context.TimekeepingAdminSetup!.FirstOrDefaultAsync(s => !s.Deleted);
            return TimeKeepingAdminSetup!;
        }
        public async Task<bool> UpdateAsync(TimekeepingAdminSetup TimekeepingAdminSetup)
        {
            var record = Context.Update(TimekeepingAdminSetup);
            if (record is null)
                return false;

            await Context.SaveChangesAsync();
            return true;
        }
    }
}
