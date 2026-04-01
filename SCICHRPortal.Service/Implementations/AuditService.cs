using SCICHRPortal.Data.Entities;
using SCICHRPortal.Repository.Interfaces;
using SCICHRPortal.Service.Interfaces;

namespace SCICHRPortal.Service.Implementations
{
    public class AuditService : IAuditService
    {
        public IAuditRepository AuditRepository { get; }

        public AuditService(IAuditRepository auditRepository)
        {
            AuditRepository = auditRepository;
        }

        public async Task<Tuple<IEnumerable<Audit>, int>> FilterAsync(int pageNumber, int pageSize, string searchKeyword, string system)
        {
            return await AuditRepository.FilterAsync(pageNumber, pageSize, searchKeyword, system);
        }

        public async Task InsertAsync(Audit entity)
        {
            await AuditRepository.InsertAsync(entity);
        }

        public async Task<bool> HasRecord(string tableName, string type, DateTime dateTime)
        {
            return await AuditRepository.HasRecord(tableName, type, dateTime);
        }
    }
}
