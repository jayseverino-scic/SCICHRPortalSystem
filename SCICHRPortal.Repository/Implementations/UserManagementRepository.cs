using System.Data;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using SCICHRPortal.Data.DTOs;
using SCICHRPortal.Data.Entities;
using SCICHRPortal.Repository.Interfaces;
using SCICHRPortal.Utility.HttpContext.Interfaces;

namespace SCICHRPortal.Repository.Implementations;

public sealed class UserManagementRepository : Repository, IUserManagementRepository
{
    private IHttpContextService? RequestMetadata { get; }
    public UserManagementRepository(ApplicationContext context, XscribeContext xscribeContext, TimekeepingContext timekeepingContext,
        IHttpContextService? requestMetadata = null)
        : base(context, xscribeContext, timekeepingContext) { RequestMetadata = requestMetadata; }

    private static readonly Expression<Func<User, UserRow>> Row = user => new UserRow
    {
        UserId = user.UserId, FirstName = user.FirstName, MiddleName = user.MiddleName,
        LastName = user.LastName, Email = user.Email, ContactNumber = user.ContactNumber,
        IsApproved = user.IsApproved, Active = user.Active, Locked = user.Locked,
        Roles = user.UserRoles!.Where(link => !link.Deleted && !link.Role!.Deleted)
            .OrderBy(link => link.RoleId).Select(link => new UserRoleOption(link.RoleId, link.Role!.Name!)).ToList()
    };

    public async Task<UserPage> GetPageAsync(UserPageQuery query, CancellationToken cancellationToken)
    {
        if (!query.IsValid()) throw new ArgumentException("Invalid user paging options.", nameof(query));
        var users = Context.User!.AsNoTracking().Where(user => !user.Deleted);
        var total = await users.CountAsync(cancellationToken);
        if (query.Approval is "approved" or "pending")
            users = users.Where(user => user.IsApproved == (query.Approval == "approved"));
        if (query.State is "active" or "inactive")
            users = users.Where(user => user.Active == (query.State == "active"));
        var search = query.Search?.Trim().ToLowerInvariant();
        if (!string.IsNullOrEmpty(search))
            users = users.Where(user => user.FirstName!.ToLower().Contains(search) || user.LastName!.ToLower().Contains(search)
                || user.Email!.ToLower().Contains(search) || (user.MiddleName != null && user.MiddleName.ToLower().Contains(search)));
        var filtered = await users.CountAsync(cancellationToken);
        var data = await users.OrderByDescending(user => user.UserId).Skip(query.Start).Take(query.Length)
            .Select(Row).ToListAsync(cancellationToken);
        return new(query.Draw, total, filtered, data);
    }

    public Task<UserRow?> GetDetailsAsync(int id, CancellationToken cancellationToken) =>
        Context.User!.AsNoTracking().Where(user => user.UserId == id && !user.Deleted).Select(Row).SingleOrDefaultAsync(cancellationToken);

    public async Task<IReadOnlyList<UserRoleOption>> GetRolesAsync(CancellationToken cancellationToken) =>
        await Context.Role!.AsNoTracking().Where(role => !role.Deleted).OrderBy(role => role.Name)
            .Select(role => new UserRoleOption(role.RoleId, role.Name!)).ToListAsync(cancellationToken);

    public Task<User?> GetAccountAsync(int id, CancellationToken cancellationToken) =>
        Context.User!.AsNoTracking().Include(user => user.UserRoles!).ThenInclude(link => link.Role)
            .SingleOrDefaultAsync(user => user.UserId == id && !user.Deleted, cancellationToken);

    public Task<User?> GetAccountByEmailAsync(string normalizedEmail, CancellationToken cancellationToken) =>
        Context.User!.AsNoTracking().Include(user => user.UserRoles!).ThenInclude(link => link.Role)
            .SingleOrDefaultAsync(user => EF.Property<string>(user, "NormalizedEmail") == normalizedEmail && !user.Deleted, cancellationToken);

    public Task<bool> AnyAccountAsync(Expression<Func<User, bool>> predicate, CancellationToken cancellationToken) =>
        Context.User!.AsNoTracking().AnyAsync(predicate, cancellationToken);

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

    public async Task SaveAccountAsync(User account, string actor, CancellationToken cancellationToken)
    {
        if (account.UserId == 0)
        {
            Context.User!.Add(account);
        }
        else
        {
            var record = await Context.User!.SingleAsync(user => user.UserId == account.UserId, cancellationToken);
            // Authorization filters/other reads may have tracked an older copy in this request.
            await Context.Entry(record).ReloadAsync(cancellationToken);
            record.FirstName = account.FirstName;
            record.MiddleName = account.MiddleName;
            record.LastName = account.LastName;
            record.Email = account.Email;
            record.ContactNumber = account.ContactNumber;
            record.IsApproved = account.IsApproved;
            record.Active = account.Active;
            record.Locked = account.Locked;
            record.Deleted = account.Deleted;
            record.Password = account.Password;
            record.Salt = account.Salt;
            record.IsPasswordChanged = account.IsPasswordChanged;
            record.LoginAttempts = account.LoginAttempts;
            record.UpdatedAt = account.UpdatedAt;
            record.UpdatedBy = account.UpdatedBy;

            var records = await Context.UserRole!.Where(link => link.UserId == account.UserId).ToListAsync(cancellationToken);
            foreach (var assignment in account.UserRoles ?? [])
            {
                if (assignment.UserRoleId == 0)
                {
                    Context.UserRole!.Add(new UserRole { User = record, RoleId = assignment.RoleId,
                        Deleted = assignment.Deleted, CreatedAt = assignment.CreatedAt, CreatedBy = assignment.CreatedBy });
                }
                else
                {
                    var link = records.Single(item => item.UserRoleId == assignment.UserRoleId);
                    link.Deleted = assignment.Deleted;
                    link.UpdatedAt = assignment.UpdatedAt;
                    link.UpdatedBy = assignment.UpdatedBy;
                }
            }
        }
        cancellationToken.ThrowIfCancellationRequested();
        await Context.SaveChangesAsync(new ImportAudit(actor,
            string.IsNullOrWhiteSpace(RequestMetadata?.GetReferrer()) ? "User management" : RequestMetadata!.GetReferrer()!,
            string.IsNullOrWhiteSpace(RequestMetadata?.GetSystem()) ? "HR Portal" : RequestMetadata!.GetSystem()!));
    }

    private static bool IsConflict(Exception exception)
    {
        for (Exception? current = exception; current != null; current = current.InnerException)
            if (current is PostgresException { SqlState: "23505" or "40001" or "40P01" or "23503" } || current is DbUpdateConcurrencyException) return true;
        return false;
    }
}
