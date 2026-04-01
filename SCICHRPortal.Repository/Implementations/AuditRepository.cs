using Microsoft.EntityFrameworkCore;
using SCICHRPortal.Data.Entities;
using SCICHRPortal.Repository.Interfaces;

namespace SCICHRPortal.Repository.Implementations
{
    public class AuditRepository : Repository, IAuditRepository
    {

        public AuditRepository(ApplicationContext context)
            : base(context)
        {
        }

        public async Task<Tuple<IEnumerable<Audit>, int>> FilterAsync(int pageNumber, int pageSize, string searchKeyword, string system)
        {
            var auditLogs = Context.AuditLogs!.Where(e => e.Id != 0);


            if (!String.IsNullOrWhiteSpace(system))
            {
                auditLogs = auditLogs
                    .Where(e =>
                        e.SystemName!.ToLower().Contains(system.ToLower())
                        );
            }

            if (!String.IsNullOrWhiteSpace(searchKeyword))
            {
                auditLogs = auditLogs
                    .Where(e =>
                        e.UserName!.ToLower().Contains(searchKeyword.ToLower())
                        || e.Type!.ToLower().Contains(searchKeyword.ToLower())
                        || e.TableName!.ToLower().Contains(searchKeyword.ToLower())
                        );
            }


            var total = auditLogs.Count();

            auditLogs = auditLogs
                .OrderByDescending(e => e.Id)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize);


            return new Tuple<IEnumerable<Audit>, int>(await auditLogs.ToListAsync(), total);
        }

        public async Task<bool> HasRecord(string tableName, string type, DateTime dateTime)
        {
            return await Context.AuditLogs!
           .AnyAsync(r =>
           r.TableName!.ToLower() == tableName.ToLower() &&
           r.Type!.ToLower() == type.ToLower() &&
           r.DateTime.Date == dateTime.Date);
        }

        public async Task InsertAsync(Audit entity)
        {
            await Context.AuditLogs!.AddAsync(entity);
            await Context.SaveChangesAsync();
        }
    }
}
