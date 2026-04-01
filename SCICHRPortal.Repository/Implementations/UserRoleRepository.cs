using Microsoft.EntityFrameworkCore;
using SCICHRPortal.Data.Entities;
using SCICHRPortal.Repository.Interfaces;

namespace SCICHRPortal.Repository.Implementations
{
    public class UserRoleRepository : Repository, IUserRoleRepository
    {
        public UserRoleRepository(ApplicationContext context) : base(context)
        {
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var userRole = await Context.UserRole!
                       .SingleOrDefaultAsync(s => s.UserRoleId == id && !s.Deleted);
            if (userRole == null)
                return false;

            userRole.Deleted = true;
            await Context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UnDeleteAsync(int id)
        {
            var userRole = await Context.UserRole!
                       .SingleOrDefaultAsync(s => s.UserRoleId == id);
            if (userRole == null)
                return false;

            userRole.Deleted = false;
            await Context.SaveChangesAsync();
            return true;
        }

        public async Task<UserRole> GetAsync(int id)
        {
            var userRoles = await Context.UserRole!
            .Where(x => x.UserRoleId == id).FirstOrDefaultAsync();

            return userRoles!;
        }

        public async Task<List<UserRole>> GetByRoleIdAsync(int roleId)
        {
            var userRoles = await Context.UserRole!.AsNoTracking()
                .Include(x => x.User).Where(x => !x.Deleted)
                .Where(x => x.RoleId == roleId && !x.Deleted).ToListAsync();

            return userRoles;
        }

        public async Task<List<UserRole>> GetByUserAsync(int id)
        {
            var userRoles = await Context.UserRole!
            .Where(x => x.UserId == id && !x.Deleted).ToListAsync();

            return userRoles!;
        }

        public async Task<UserRole> GetByUserIdAndRoleIdAsync(int userId, int roleId)
        {
            var userRoles = await Context.UserRole!
              .Where(x => x.RoleId == roleId && x.UserId == userId).FirstOrDefaultAsync();

            return userRoles!;
        }

        public async Task<UserRole> GetByUserIdAsync(int id)
        {
            var userRole =  await Context.UserRole!
                .FirstOrDefaultAsync(u =>
                u.UserId == id &&
                u.Deleted == false
                );
            return userRole!;
        }

        public async Task InsertAsync(UserRole entity)
        {
            await Context.UserRole!.AddAsync(entity);
        }

        public async Task<bool> UpdateAsync(UserRole userRole)
        {
            var record = Context.Update(userRole);
            if (record is null)
                return false;

            await Context.SaveChangesAsync();
            return true;
        }
    }
}
