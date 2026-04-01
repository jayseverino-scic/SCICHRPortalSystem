using Microsoft.EntityFrameworkCore;
using SCICHRPortal.Data.Entities;
using SCICHRPortal.Repository.Interfaces;

namespace SCICHRPortal.Repository.Implementations
{
    public class UserRepository : Repository, IUserRepository
    {

        public UserRepository(ApplicationContext context)
            : base(context)
        {
        }

        public async Task ApprovedAsync(int id)
        {
            var user = await Context.User!
            .SingleOrDefaultAsync(c => c.UserId == id && !c.Deleted);

            if(user is not null)
            {
                user.IsApproved = true;
                await Context.SaveChangesAsync();
            }
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var user = await Context.User!
            .SingleOrDefaultAsync(c => c.UserId == id && !c.Deleted);

            
            if (user == null)
                return false;

            user.Deleted = true;
            await Context.SaveChangesAsync();
            return true;
        }

        public async Task<Tuple<IEnumerable<User>, int>> FilterApprovedUserAsync(int pageNumber, int pageSize, string searchKeyword)
        {

            var users = Context.User!
                .Include(c => c.UserRoles!.Where(ur => !ur.Deleted))
                .Include(u => u.UserRoles!.Where(ur => !ur.Deleted))
                    .ThenInclude(r => r.Role)
         .Where(e => e.Deleted == false && e.IsApproved);

            if (!String.IsNullOrWhiteSpace(searchKeyword))
            {
                users = users
                    .Where(e =>
                        e.FirstName!.ToLower().Contains(searchKeyword.ToLower())
                        || e.LastName!.ToLower().Contains(searchKeyword.ToLower())
                        || e.Username!.ToLower().Contains(searchKeyword.ToLower())
                        || e.Email!.ToLower().Contains(searchKeyword.ToLower())
                        );
            }

            var total = users.Count();

            users = users
                .OrderByDescending(e => e.UserId)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize);

            return new Tuple<IEnumerable<User>, int>(await users.ToListAsync(), total);
        }

        public async Task<Tuple<IEnumerable<User>, int>> FilterUnApprovedUserAsync(int pageNumber, int pageSize, string searchKeyword)
        {
            var users = Context.User!
           .Where(e => e.Deleted == false && !e.IsApproved);

            if (!String.IsNullOrWhiteSpace(searchKeyword))
            {
                users = users
                    .Where(e =>
                        e.FirstName!.ToLower().Contains(searchKeyword.ToLower())
                        || e.LastName!.ToLower().Contains(searchKeyword.ToLower())
                        || e.Username!.ToLower().Contains(searchKeyword.ToLower())
                        || e.Email!.ToLower().Contains(searchKeyword.ToLower())
                        );
            }

            var total = users.Count();

            users = users
                .OrderByDescending(e => e.UserId)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize);


            return new Tuple<IEnumerable<User>, int>(await users.ToListAsync(), total);
        }

        public async Task<IEnumerable<User>> GetAllAsync()
        {
            return await Context.User!
                .Include(u => u.UserRoles!)
                    .ThenInclude(r => r.Role)
              .Where(u => u.Deleted == false && u.IsApproved)
              .ToListAsync()!;
        }

        public async Task<User> GetAsync(int id)
        {
            var user =  await Context.User!
               .SingleOrDefaultAsync(u =>
                   u.UserId == id &&
                   u.Deleted == false)!;

            return user!;
        }

        public async Task<User> GetByEmailAsync(string email)
        {
            var user = await Context.User!
              .FirstOrDefaultAsync(u =>
                  u.Email!.ToLower() == email.ToLower() &&
                  u.Deleted == false);

            return user!;
        }

        public async Task<List<UserRole>> GetByRoleIdsync(int roleId)
        {
            var users = await Context.UserRole!
              .Include(c => c.Role)
              .Include(c => c.User)
              .Where(e => e.RoleId == roleId && e.User!.IsApproved && !e.Deleted)
              .ToListAsync();

            return users!;
        }

        public async Task<User> GetByUserNameAsync(string username)
        {
            var user =  await Context.User!
              .SingleOrDefaultAsync(u =>
                  u.Username!.ToLower() == username.ToLower() &&
                  u.Deleted == false);

            return user!;
        }

        public async Task<User> GetDuplicateAsync(User user)
        {
            var u = await Context.User!
                .SingleOrDefaultAsync(r =>
                    r.Email!.ToLower() == user.Email!.ToLower() &&
                    r.Deleted! == false);
            return u!;
        }   
        public async Task<User> GetDuplicateEmailAsync(User user)
        {
            var u = await Context.User!
                .FirstOrDefaultAsync(r =>
                    r.Email!.ToLower() == user.Email!.ToLower() &&
                    r.Deleted! == false);
            return u!;
        }

        public async Task<IEnumerable<User>> GetUnApprovedUserAsync()
        {
            return await Context.User!
               .Where(u => u.Deleted == false && !u.IsApproved)
               .ToListAsync()!;
        }

        public async Task<IEnumerable<UserRole>> GetUserRolesAsync(int id)
        {
            return await Context.UserRole!
                .Include(ur => ur.Role)
                .Where(ur =>
                    ur.UserId == id &&
                    ur.Deleted == false)
                .ToListAsync()!;
        }

        public async Task<bool> HasDuplicateNameAsync(string name)
        {
            return await Context.User!
               .AnyAsync(r =>
               r.Username!.ToLower() == name.ToLower()
               && r.Deleted == false);
        }

        public async Task InsertAsync(User entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(User));

            Context.User!.Add(entity);
            await Context.SaveChangesAsync();
        }

        public async Task ResetPassword(User user)
        {
            await Context.SaveChangesAsync();
        }

        public async Task ToggleActiveAsync(bool isActive, int id)
        {
            var user = await Context.User!
            .SingleOrDefaultAsync(c => c.UserId == id && !c.Deleted);

            if (user is not null)
            {
                user.Active = isActive;
                await Context.SaveChangesAsync();
            }
        }

        public async Task UpdateAsync(User entity)
        {
            var record = Context.Update(entity);
            if (record is not null)
                await Context.SaveChangesAsync();
        }
    }
}
