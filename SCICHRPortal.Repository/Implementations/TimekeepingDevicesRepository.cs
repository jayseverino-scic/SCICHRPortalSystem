using Microsoft.EntityFrameworkCore;
using System.Data;
using SCICHRPortal.Data.DTOs;
using SCICHRPortal.Data.Entities;
using SCICHRPortal.Data.Entities.Metadatas;
using SCICHRPortal.Repository.Interfaces;
using SCICHRPortal.Utility.Extensions;

namespace SCICHRPortal.Repository.Implementations
{
    public class TimekeepingDevicesRepository : Repository, ITimekeepingDevicesRepository
    {
        public TimekeepingDevicesRepository(ApplicationContext context)
    : base(context)
        {
        }
        public async Task<bool> DeleteAsync(int Id)
        {
            var TimekeepingDevices = await Context.TimekeepingDevices!
                        .SingleOrDefaultAsync(s => s.Id == Id && !s.Deleted);
            if (TimekeepingDevices == null)
                return false;

            TimekeepingDevices.Deleted = true;
            await Context.SaveChangesAsync();
            return true;
        }

        public async Task<Tuple<IEnumerable<TimekeepingDevices>, int>> FilterAsync(int pageNumber, int pageSize, string searchKeyword)
        {
            var TimekeepingDevicess = Context.TimekeepingDevices!
              .Where(e => e.Deleted == false);

            if (!String.IsNullOrWhiteSpace(searchKeyword))
            {
                TimekeepingDevicess = TimekeepingDevicess
                    .Where(e =>
                        e.Name!.ToLower().Contains(searchKeyword.ToLower()));
            }

            var total = TimekeepingDevicess.Count();

            TimekeepingDevicess = TimekeepingDevicess
                .OrderByDescending(e => e.Id)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize);

            return new Tuple<IEnumerable<TimekeepingDevices>, int>(await TimekeepingDevicess.ToListAsync(), total);
        }

        public async Task<IEnumerable<TimekeepingDevices>> GetAllAsync()
        {
            var TimekeepingDevicess = await Context.TimekeepingDevices!.Where(s => !s.Deleted)
              .ToListAsync();
            return TimekeepingDevicess;
        }

        public async Task<TimekeepingDevices> GetAsync(int id)
        {
            var TimekeepingDevices = await Context.TimekeepingDevices!
                    .SingleOrDefaultAsync(s => s.Id == id && !s.Deleted);
            return TimekeepingDevices!;
        }

        public async Task<DuplicateMessage> HasDuplicateName(TimekeepingDevices TimekeepingDevices)
        {
            DuplicateMessage message = new();
            var title = TimekeepingDevices.Name!.ToLower().StringSplitThenJoin();
            var announcementMessage = TimekeepingDevices.Name!.ToLower().StringSplitThenJoin();
            var TimekeepingDevicess = await Context.TimekeepingDevices!
               .Where(r => r.Deleted == false).ToListAsync();

            var duplicatedTitle = TimekeepingDevicess.Any(t => t.Name!.ToLower().StringSplitThenJoin() == title);
            var duplicatedMessage = TimekeepingDevicess.Any(t => announcementMessage.ToLower() == t.Name!.ToLower().StringSplitThenJoin());
            var duplicatedDate = TimekeepingDevicess.Any(t => t.CreatedAt.Date == DateTime.Now.Date);

            if (duplicatedDate && duplicatedTitle)
            {
                message.Message = "TimekeepingDevices Name Duplicated";
            }
            else if (duplicatedDate && duplicatedMessage)
            {
                message.Message = "TimekeepingDevices Name Duplicated";
            }

            message.IsDuplicated = (duplicatedTitle || duplicatedMessage) && duplicatedDate;
            return message;
        }

        public async Task InsertAsync(TimekeepingDevices entity)
        {
            await Context.TimekeepingDevices!.AddAsync(entity);
            await Context.SaveChangesAsync();
        }

        public async Task<bool> UpdateAsync(TimekeepingDevices TimekeepingDevices)
        {
            var record = Context.Update(TimekeepingDevices);
            if (record is null)
                return false;

            await Context.SaveChangesAsync();
            return true;
        }
    }
}
