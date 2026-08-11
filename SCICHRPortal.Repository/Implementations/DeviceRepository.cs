using Microsoft.EntityFrameworkCore;
using System.Data;
using SCICHRPortal.Data.DTOs;
using SCICHRPortal.Data.Entities;
using SCICHRPortal.Data.Entities.Metadatas;
using SCICHRPortal.Repository.Interfaces;
using SCICHRPortal.Utility.Extensions;


namespace SCICHRPortal.Repository.Implementations
{
    public class DeviceRepository : Repository, IDeviceRepository
    {
        public DeviceRepository(ApplicationContext context, XscribeContext xscribeContext, TimekeepingContext timekeepingContext)
    : base(context, xscribeContext, timekeepingContext)
        {
        }
        public async Task<bool> DeleteAsync(int id)
        {
            var device = await Context.Device!
                        .SingleOrDefaultAsync(s => s.Id == id && !s.Deleted);
            if (device == null)
                return false;

            device.Deleted = true;
            await Context.SaveChangesAsync();
            return true;
        }

        public async Task<Tuple<IEnumerable<Device>, int>> FilterAsync(int pageNumber, int pageSize, string searchKeyword)
        {
            var devices = Context.Device!
              .Where(e => e.Deleted == false);

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

            return new Tuple<IEnumerable<Device>, int>(await devices.ToListAsync(), total);
        }

        public async Task<IEnumerable<Device>> GetAllAsync()
        {
            var devices = await Context.Device!.Where(s => !s.Deleted)
              .ToListAsync();
            return devices;
        }

        public async Task<Device> GetAsync(int id)
        {
            var device = await Context.Device!
                    .SingleOrDefaultAsync(s => s.Id == id && !s.Deleted);
            return device!;
        }

        public async Task<DuplicateMessage> HasDuplicateName(Device device)
        {
            DuplicateMessage message = new();
            var title = device.Name!.ToLower().StringSplitThenJoin();
            var announcementMessage = device.Name!.ToLower().StringSplitThenJoin();
            var devices = await Context.Device!
               .Where(r => r.Deleted == false).ToListAsync();

            var duplicatedTitle = devices.Any(t => t.Name!.ToLower().StringSplitThenJoin() == title);
            var duplicatedMessage = devices.Any(t => announcementMessage.ToLower() == t.Name!.ToLower().StringSplitThenJoin());
            var duplicatedDate = devices.Any(t => t.CreatedAt.Date == DateTime.Now.Date);

            if (duplicatedDate && duplicatedTitle)
            {
                message.Message = "Device Name Duplicated";
            }
            else if (duplicatedDate && duplicatedMessage)
            {
                message.Message = "Device Name Duplicated";
            }

            message.IsDuplicated = (duplicatedTitle || duplicatedMessage) && duplicatedDate;
            return message;
        }

        public async Task InsertAsync(Device entity)
        {
            await Context.Device!.AddAsync(entity);
            await Context.SaveChangesAsync();
        }

        public async Task<bool> UpdateAsync(Device device)
        {
            var record = Context.Update(device);
            if (record is null)
                return false;

            await Context.SaveChangesAsync();
            return true;
        }
    }
}
