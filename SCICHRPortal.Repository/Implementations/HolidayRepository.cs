using Microsoft.EntityFrameworkCore;
using System.Data;
using SCICHRPortal.Data.DTOs;
using SCICHRPortal.Data.Entities;
using SCICHRPortal.Data.Entities.Metadatas;
using SCICHRPortal.Repository.Interfaces;
using SCICHRPortal.Utility.Extensions;

namespace SCICHRPortal.Repository.Implementations
{
    public class HolidayRepository : Repository, IHolidayRepository
    {
        public HolidayRepository(ApplicationContext context)
    : base(context)
        {
        }
        public async Task<bool> DeleteAsync(int holidayId)
        {
            var holiday = await Context.Holiday!
                        .SingleOrDefaultAsync(s => s.HolidayId == holidayId && !s.Deleted);
            if (holiday == null)
                return false;

            holiday.Deleted = true;
            await Context.SaveChangesAsync();
            return true;
        }

        public async Task<Tuple<IEnumerable<Holiday>, int>> FilterAsync(int pageNumber, int pageSize, string searchKeyword)
        {
            var holidays = Context.Holiday!
              .Where(e => e.Deleted == false);

            if (!String.IsNullOrWhiteSpace(searchKeyword))
            {
                holidays = holidays
                    .Where(e =>
                        e.HolidayName!.ToLower().Contains(searchKeyword.ToLower()));
            }

            var total = holidays.Count();

            holidays = holidays
                .OrderByDescending(e => e.HolidayId)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize);

            return new Tuple<IEnumerable<Holiday>, int>(await holidays.ToListAsync(), total);
        }

        public async Task<IEnumerable<Holiday>> GetAllAsync()
        {
            var holidays = await Context.Holiday!.Where(s => !s.Deleted)
              .ToListAsync();
            return holidays;
        }

        public async Task<Holiday> GetAsync(int id)
        {
            var holiday = await Context.Holiday!
                    .SingleOrDefaultAsync(s => s.HolidayId == id && !s.Deleted);
            return holiday!;
        }

        public async Task<DuplicateMessage> HasDuplicateName(Holiday holiday)
        {
            DuplicateMessage message = new();
            var title = holiday.HolidayName!.ToLower().StringSplitThenJoin();
            var announcementMessage = holiday.HolidayName!.ToLower().StringSplitThenJoin();
            var holidays = await Context.Holiday!
               .Where(r => r.Deleted == false).ToListAsync();

            var duplicatedTitle = holidays.Any(t => t.HolidayName!.ToLower().StringSplitThenJoin() == title);
            var duplicatedMessage = holidays.Any(t => announcementMessage.ToLower() == t.HolidayName!.ToLower().StringSplitThenJoin());
            var duplicatedDate = holidays.Any(t => t.CreatedAt.Date == DateTime.Now.Date);

            if (duplicatedDate && duplicatedTitle)
            {
                message.Message = "Holiday Name Duplicated";
            }
            else if (duplicatedDate && duplicatedMessage)
            {
                message.Message = "Holiday Name Duplicated";
            }

            message.IsDuplicated = (duplicatedTitle || duplicatedMessage) && duplicatedDate;
            return message;
        }

        public async Task InsertAsync(Holiday entity)
        {
            await Context.Holiday!.AddAsync(entity);
            await Context.SaveChangesAsync();
        }

        public async Task<bool> UpdateAsync(Holiday holiday)
        {
            var record = Context.Update(holiday);
            if (record is null)
                return false;

            await Context.SaveChangesAsync();
            return true;
        }
    }
}

