using Microsoft.EntityFrameworkCore;
using System.Data;
using SCICHRPortal.Data.DTOs;
using SCICHRPortal.Data.Entities;
using SCICHRPortal.Data.Entities.Metadatas;
using SCICHRPortal.Repository.Interfaces;
using SCICHRPortal.Utility.Extensions;


namespace SCICHRPortal.Repository.Implementations
{
    public class PositionRepository : Repository, IPositionRepository
    {
        public PositionRepository(ApplicationContext context)
    : base(context)
        {
        }
        public async Task<bool> DeleteAsync(int categoryId)
        {
            var position = await Context.Position!
                        .SingleOrDefaultAsync(s => s.PositionId == categoryId && !s.Deleted);
            if (position == null)
                return false;

            position.Deleted = true;
            await Context.SaveChangesAsync();
            return true;
        }

        public async Task<Tuple<IEnumerable<Position>, int>> FilterAsync(int pageNumber, int pageSize, string searchKeyword)
        {
            var positions = Context.Position!
              .Where(e => e.Deleted == false);

            if (!String.IsNullOrWhiteSpace(searchKeyword))
            {
                positions = positions
                    .Where(e =>
                        e.PositionName!.ToLower().Contains(searchKeyword.ToLower()));
            }

            var total = positions.Count();

            positions = positions
                .OrderByDescending(e => e.PositionId)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize);

            return new Tuple<IEnumerable<Position>, int>(await positions.ToListAsync(), total);
        }

        public async Task<IEnumerable<Position>> GetAllAsync()
        {
            var positions = await Context.Position!.Where(s => !s.Deleted)
              .ToListAsync();
            return positions;
        }

        public async Task<Position> GetAsync(int id)
        {
            var position = await Context.Position!
                    .SingleOrDefaultAsync(s => s.PositionId == id && !s.Deleted);
            return position!;
        }

        public async Task<DuplicateMessage> HasDuplicateName(Position position)
        {
            DuplicateMessage message = new();
            var title = position.PositionName!.ToLower().StringSplitThenJoin();
            var announcementMessage = position.PositionName!.ToLower().StringSplitThenJoin();
            var positions = await Context.Position!
               .Where(r => r.Deleted == false).ToListAsync();

            var duplicatedTitle = positions.Any(t => t.PositionName!.ToLower().StringSplitThenJoin() == title);
            var duplicatedMessage = positions.Any(t => announcementMessage.ToLower() == t.PositionName!.ToLower().StringSplitThenJoin());
            var duplicatedDate = positions.Any(t => t.CreatedAt.Date == DateTime.Now.Date);

            if (duplicatedDate && duplicatedTitle)
            {
                message.Message = "Position Name Duplicated";
            }
            else if (duplicatedDate && duplicatedMessage)
            {
                message.Message = "Position Name Duplicated";
            }

            message.IsDuplicated = (duplicatedTitle || duplicatedMessage) && duplicatedDate;
            return message;
        }

        public async Task InsertAsync(Position entity)
        {
            await Context.Position!.AddAsync(entity);
            await Context.SaveChangesAsync();
        }

        public async Task<bool> UpdateAsync(Position position)
        {
            var record = Context.Update(position);
            if (record is null)
                return false;

            await Context.SaveChangesAsync();
            return true;
        }
    }
}
