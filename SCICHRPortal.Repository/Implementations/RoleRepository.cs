using Microsoft.EntityFrameworkCore;
using SCICHRPortal.Data.DTOs;
using SCICHRPortal.Data.Entities;
using SCICHRPortal.Repository.Interfaces;
using SCICHRPortal.Utility.HttpContext.Interfaces;
using System.Data;
using Npgsql;

namespace SCICHRPortal.Repository.Implementations
{
    public class RoleRepository : Repository, IRoleRepository
    {
        private IHttpContextService? RequestMetadata { get; }
        public RoleRepository(ApplicationContext context, XscribeContext xscribeContext, TimekeepingContext timekeepingContext,
            IHttpContextService? requestMetadata = null) : base(context, xscribeContext, timekeepingContext)
        {
            RequestMetadata = requestMetadata;
        }

        public async Task<bool> DeleteAsync(int roleId)
        {
            var role = await Context.Role!.SingleOrDefaultAsync(row => row.RoleId == roleId && !row.Deleted);
            if (role == null) return false;
            role.Deleted = true;
            await Context.SaveChangesAsync();
            return true;
        }

        public Task<bool> HasAssignmentsAsync(int roleId, CancellationToken cancellationToken) =>
            Context.UserRole!.AsNoTracking().AnyAsync(link => link.RoleId == roleId && !link.Deleted, cancellationToken);

        public async Task ExecuteWriteAsync(Func<Task> operation, CancellationToken cancellationToken)
        {
            await using var transaction = await Context.Database.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);
            try
            {
                await operation();
                await transaction.CommitAsync(cancellationToken);
            }
            catch (Exception exception) when (IsConflict(exception))
            {
                Context.ChangeTracker.Clear();
                throw new UserPersistenceConflictException(exception);
            }
        }

        public async Task SaveRoleAsync(Role role, string actor, CancellationToken cancellationToken)
        {
            if (role.RoleId == 0) Context.Role!.Add(role);
            else
            {
                var record = await Context.Role!.SingleAsync(row => row.RoleId == role.RoleId, cancellationToken);
                await Context.Entry(record).ReloadAsync(cancellationToken);
                record.Name = role.Name; record.Description = role.Description; record.Deleted = role.Deleted;
                record.UpdatedAt = role.UpdatedAt; record.UpdatedBy = role.UpdatedBy;
            }
            cancellationToken.ThrowIfCancellationRequested();
            await Context.SaveChangesAsync(new ImportAudit(actor,
                string.IsNullOrWhiteSpace(RequestMetadata?.GetReferrer()) ? "Role management" : RequestMetadata!.GetReferrer()!,
                string.IsNullOrWhiteSpace(RequestMetadata?.GetSystem()) ? "HR Portal" : RequestMetadata!.GetSystem()!));
        }

        private static bool IsConflict(Exception exception)
        {
            for (Exception? current = exception; current != null; current = current.InnerException)
                if (current is PostgresException { SqlState: "23505" or "23503" or "40001" or "40P01" } || current is DbUpdateConcurrencyException) return true;
            return false;
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
            var roles = await Context.Role!.AsNoTracking().Where(r => !r.Deleted).ToListAsync();
            return roles!;
        }

        public async Task<Role> GetAsync(int id)
        {
            var role = await Context.Role!.AsNoTracking()
                 .SingleOrDefaultAsync(r =>
                     r.RoleId == id &&
                     r.Deleted == false);
            return role!;
        }

        public Task<Role> GetDuplicateAsync(Role role)
        {
            throw new NotImplementedException();
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
