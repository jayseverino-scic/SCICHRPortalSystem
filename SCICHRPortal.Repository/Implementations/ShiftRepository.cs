using Microsoft.EntityFrameworkCore;
using System.Data;
using SCICHRPortal.Data.DTOs;
using SCICHRPortal.Data.Entities;
using SCICHRPortal.Data.Entities.Metadatas;
using SCICHRPortal.Repository.Interfaces;
using SCICHRPortal.Utility.Extensions;

namespace SCICHRPortal.Repository.Implementations
{
    public class ShiftRepository : Repository, IShiftRepository
    {
        public ShiftRepository(ApplicationContext context)
    : base(context)
        {
        }
        public async Task<bool> DeleteAsync(int shiftId)
        {
            var shift = await Context.Shift!
                        .SingleOrDefaultAsync(s => s.ShiftId == shiftId && !s.Deleted);
            if (shift == null)
                return false;

            shift.Deleted = true;
            await Context.SaveChangesAsync();
            return true;
        }

        public async Task<Tuple<IEnumerable<Shift>, int>> FilterAsync(int pageNumber, int pageSize, string searchKeyword)
        {
            var shifts = Context.Shift!
              .Where(e => e.Deleted == false);

            if (!String.IsNullOrWhiteSpace(searchKeyword))
            {
                shifts = shifts
                    .Where(e =>
                        e.ShiftName!.ToLower().Contains(searchKeyword.ToLower()));
            }

            var total = shifts.Count();

            shifts = shifts
                .OrderByDescending(e => e.ShiftId)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize);

            return new Tuple<IEnumerable<Shift>, int>(await shifts.ToListAsync(), total);
        }

        public async Task<IEnumerable<Shift>> GetAllAsync()
        {
            var shifts = await Context.Shift!.Where(s => !s.Deleted)
              .ToListAsync();
            return shifts;
        }

        public async Task<Shift> GetAsync(int id)
        {
            var shift = await Context.Shift!
                    .SingleOrDefaultAsync(s => s.ShiftId == id && !s.Deleted);
            return shift!;
        }

        public async Task<DuplicateMessage> HasDuplicateName(Shift shift)
        {
            DuplicateMessage message = new();
            var title = shift.ShiftName!.ToLower().StringSplitThenJoin();
            var announcementMessage = shift.ShiftName!.ToLower().StringSplitThenJoin();
            var shifts = await Context.Shift!
               .Where(r => r.Deleted == false).ToListAsync();

            var duplicatedTitle = shifts.Any(t => t.ShiftName!.ToLower().StringSplitThenJoin() == title);
            var duplicatedMessage = shifts.Any(t => announcementMessage.ToLower() == t.ShiftName!.ToLower().StringSplitThenJoin());
            var duplicatedDate = shifts.Any(t => t.CreatedAt.Date == DateTime.Now.Date);

            var duplicatedShiftTime = shifts.Any(t => t.ShiftStart!.Value.TimeOfDay == shift.ShiftStart!.Value.TimeOfDay && t.ShiftEnd!.Value.TimeOfDay == shift.ShiftEnd!.Value.TimeOfDay);

            if (duplicatedDate && duplicatedTitle)
            {
                message.Message = "Shift Name Duplicated";
            }
            else if (duplicatedDate && duplicatedMessage)
            {
                message.Message = "Shift Name Duplicated";
            }
            else if (duplicatedShiftTime)
            {
                message.Message = "Shift Time Duplicated";
            }
            else
                message.IsDuplicated = (duplicatedTitle || duplicatedMessage) && duplicatedDate && duplicatedShiftTime;
            return message;
        }

        public async Task InsertAsync(Shift entity)
        {
            await Context.Shift!.AddAsync(entity);
            await Context.SaveChangesAsync();
        }

        public async Task<bool> UpdateAsync(Shift shift)
        {
            var record = Context.Update(shift);
            if (record is null)
                return false;

            await Context.SaveChangesAsync();
            return true;
        }
    }
}
