using Microsoft.EntityFrameworkCore;
using SCICHRPortal.Data.DTOs;
using SCICHRPortal.Data.Entities;
using SCICHRPortal.Repository.Interfaces;
using SCICHRPortal.Utility.Extensions;
using static System.Collections.Specialized.BitVector32;

namespace SCICHRPortal.Repository.Implementations
{
    public class RoleRepository : Repository, IRoleRepository
    {
        public RoleRepository(ApplicationContext context) : base(context)
        {
        }

        public async Task<bool> DeleteAsync(int roleId)
        {

            var role = await Context.Role!
              .SingleOrDefaultAsync(c => c.RoleId == roleId && !c.Deleted);

            if (role == null)
                return false;

            var isInUsed = await Context.UserRole!.AnyAsync(c => c.RoleId == role.RoleId && c.Deleted == false);
            if (isInUsed)
                return false;

            role.Deleted = true;
            await Context.SaveChangesAsync();
            return true;
        }

        public Task<IEnumerable<Role>> FilterAsync(string filter)
        {
            throw new NotImplementedException();
        }

        public async Task<Tuple<IEnumerable<Role>, int>> FilterAsync(int pageNumber, int pageSize, string searchKeyword)
        {
            var sections = Context.Role!
            .Where(e => e.Deleted == false);

            if (!String.IsNullOrWhiteSpace(searchKeyword))
            {
                sections = sections
                    .Where(e =>
                        e.Description!.ToLower().Contains(searchKeyword.ToLower())
                        || e.Name!.ToLower().Contains(searchKeyword.ToLower()));
            }

            var total = sections.Count();

            sections = sections
                .OrderByDescending(e => e.RoleId)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize);


            return new Tuple<IEnumerable<Role>, int>(await sections.ToListAsync(), total);
        }

        public async Task<IEnumerable<Role>> GetAllAsync()
        {
            var roles = await Context.Role!.Where(r => !r.Deleted).ToListAsync();
            return roles!;
        }

        public async Task<Role> GetAsync(int id)
        {
            var role = await Context.Role!
                 .SingleOrDefaultAsync(r =>
                     r.RoleId == id &&
                     r.Deleted == false);
            return role!;
        }

        public Task<Role> GetDuplicateAsync(Role role)
        {
            throw new NotImplementedException();
        }

        public async Task<DuplicateMessage> HasDuplicateName(Role role)
        {
            DuplicateMessage message = new();
            var description = role.Description!.ToLower().StringSplitThenJoin();
            var roles = await Context.Role!
              .Where(r => r.Deleted == false).ToListAsync();

            var duplicatedName = roles.Any(t =>  t.Name!.ToLower() == role.Name!.ToLower());
            var duplicatedDesc = roles.Any(t => t.Description!.ToLower().StringSplitThenJoin() == description);

            if (duplicatedName)
            {
                message.Message = "Name Duplicated";
            } else if (duplicatedDesc)
            {
                message.Message = "Description Duplicated";
            }

            message.IsDuplicated = duplicatedName || duplicatedDesc;
            return message;
        }

        public Task<bool> HasDuplicateNameAsync(string name)
        {
            throw new NotImplementedException();
        }

        public async Task InsertAsync(Role entity)
        {
            await Context.Role!.AddAsync(entity);
            await Context.SaveChangesAsync();
        }

        public async Task<bool> UpdateAsync(Role entity)
        {
            var record = Context.Update(entity);
            if (record is null)
                return false;

            await Context.SaveChangesAsync();
            return true;
        }
    }
}
