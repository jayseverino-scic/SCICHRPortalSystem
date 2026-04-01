using SCICHRPortal.Core.Interfaces;
using SCICHRPortal.Data.Entities;
using SCICHRPortal.Utility.Interface;

namespace SCICHRPortal.Repository.Interfaces
{
    public interface IAuditRepository : IRepository,
        IScopedService,
         IInserter<Audit>
    {
        Task<Tuple<IEnumerable<Audit>, int>> FilterAsync(int pageNumber, int pageSize, string searchKeyword, string system);
        Task<bool> HasRecord(string tableName, string type, DateTime dateTime);

    }
}
