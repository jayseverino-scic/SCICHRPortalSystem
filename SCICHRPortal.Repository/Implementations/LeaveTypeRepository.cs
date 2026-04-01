using Microsoft.EntityFrameworkCore;
using System.Data;
using SCICHRPortal.Data.DTOs;
using SCICHRPortal.Data.Entities;
using SCICHRPortal.Data.Entities.Metadatas;
using SCICHRPortal.Repository.Interfaces;
using SCICHRPortal.Utility.Extensions;

namespace SCICHRPortal.Repository.Implementations
{
    public class LeaveTypeRepository : Repository, ILeaveTypeRepository
    {
        public LeaveTypeRepository(ApplicationContext context)
    : base(context)
        {
        }
        public async Task<bool> DeleteAsync(int typeId)
        {
            var leaveType = await Context.LeaveType!
                        .SingleOrDefaultAsync(s => s.LeaveTypeId == typeId && !s.Deleted);
            if (leaveType == null)
                return false;

            leaveType.Deleted = true;
            await Context.SaveChangesAsync();
            return true;
        }

        public async Task<Tuple<IEnumerable<LeaveType>, int>> FilterAsync(int pageNumber, int pageSize, string searchKeyword)
        {
            var leaveTypes = Context.LeaveType!
              .Where(e => e.Deleted == false);

            if (!String.IsNullOrWhiteSpace(searchKeyword))
            {
                leaveTypes = leaveTypes
                    .Where(e =>
                        e.LeaveDescription!.ToLower().Contains(searchKeyword.ToLower()));
            }

            var total = leaveTypes.Count();

            leaveTypes = leaveTypes
                .OrderByDescending(e => e.LeaveTypeId)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize);

            return new Tuple<IEnumerable<LeaveType>, int>(await leaveTypes.ToListAsync(), total);
        }

        public async Task<IEnumerable<LeaveType>> GetAllAsync()
        {
            var leaveTypes = await Context.LeaveType!.Where(s => !s.Deleted)
              .ToListAsync();
            return leaveTypes;
        }

        public async Task<LeaveType> GetAsync(int id)
        {
            var leaveType = await Context.LeaveType!
                    .SingleOrDefaultAsync(s => s.LeaveTypeId == id && !s.Deleted);
            return leaveType!;
        }

        public async Task<DuplicateMessage> HasDuplicateName(LeaveType leaveType)
        {
            DuplicateMessage message = new();
            var title = leaveType.LeaveDescription!.ToLower().StringSplitThenJoin();
            var announcementMessage = leaveType.LeaveDescription!.ToLower().StringSplitThenJoin();
            var leaveTypes = await Context.LeaveType!
               .Where(r => r.Deleted == false).ToListAsync();

            var duplicatedTitle = leaveTypes.Any(t => t.LeaveDescription!.ToLower().StringSplitThenJoin() == title);
            var duplicatedMessage = leaveTypes.Any(t => announcementMessage.ToLower() == t.LeaveDescription!.ToLower().StringSplitThenJoin());
            var duplicatedDate = leaveTypes.Any(t => t.CreatedAt.Date == DateTime.Now.Date);

            if (duplicatedDate && duplicatedTitle)
            {
                message.Message = "Leave Type Description Duplicated";
            }
            else if (duplicatedDate && duplicatedMessage)
            {
                message.Message = "Leave Type Description Duplicated";
            }

            message.IsDuplicated = (duplicatedTitle || duplicatedMessage) && duplicatedDate;
            return message;
        }

        public async Task InsertAsync(LeaveType entity)
        {
            await Context.LeaveType!.AddAsync(entity);
            await Context.SaveChangesAsync();
        }

        public async Task<bool> UpdateAsync(LeaveType leaveType)
        {
            var record = Context.Update(leaveType);
            if (record is null)
                return false;

            await Context.SaveChangesAsync();
            return true;
        }
    }
}
