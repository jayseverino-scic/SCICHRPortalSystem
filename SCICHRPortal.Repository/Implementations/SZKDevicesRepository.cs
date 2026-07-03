using Microsoft.EntityFrameworkCore;
using System.Data;
using SCICHRPortal.Data.DTOs;
using SCICHRPortal.Data.TimekeepingTables;
using SCICHRPortal.Repository.Interfaces;
using SCICHRPortal.Utility.Extensions;


namespace SCICHRPortal.Repository.Implementations
{
    public class SZKDevicesRepository : Repository, ISZKDevicesRepository
    {
        public SZKDevicesRepository(ApplicationContext context, XscribeContext xscribeContext, TimekeepingContext timekeepingContext)
    : base(context, xscribeContext, timekeepingContext)
        {
        }

        public async Task<Tuple<IEnumerable<SZKDevices>, int>> FilterAsync(int pageNumber, int pageSize, string searchKeyword)
        {
            var devices = TimekeepingContext.ZKDevices!
              .Where(e => e.IsDeleted == false);

            if (!String.IsNullOrWhiteSpace(searchKeyword))
            {
                devices = devices
                    .Where(e =>
                        e.Name!.ToLower().Contains(searchKeyword.ToLower()));
            }

            var total = devices.Count();

            devices = devices
                .OrderByDescending(e => e.Id)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize);

            return new Tuple<IEnumerable<SZKDevices>, int>(await devices.ToListAsync(), total);
        }

        public async Task<IEnumerable<SZKDevices>> GetAllAsync()
        {
            var devices = await TimekeepingContext.ZKDevices!.Where(s => !s.IsDeleted)
              .ToListAsync();
            return devices;
        }

        public async Task<SZKDevices> GetAsync(Guid id)
        {
            var device = await TimekeepingContext.ZKDevices!
                    .SingleOrDefaultAsync(s => s.Id == id && !s.IsDeleted);
            return device!;
        }
    }
}
