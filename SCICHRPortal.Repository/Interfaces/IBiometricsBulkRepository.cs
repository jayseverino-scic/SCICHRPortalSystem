using SCICHRPortal.Data.Entities;
using System.Data;
using System.Threading.Tasks;

namespace SCICHRPortal.Data.Repositories.Interfaces
{
    public interface IBiometricsBulkRepository
    {
        /// <summary>
        /// Bulk insert using SqlBulkCopy
        /// </summary>
        Task BulkInsertAsync(DataTable dataTable);

        /// <summary>
        /// Bulk insert with transaction support
        /// </summary>
        Task BulkInsertWithTransactionAsync(DataTable dataTable);

        /// <summary>
        /// Bulk insert with return count
        /// </summary>
        Task<int> BulkInsertWithReturnCountAsync(DataTable dataTable);

        /// <summary>
        /// Build DataTable from BiometricsLog list
        /// </summary>
        DataTable BuildDataTableFromLogs(List<BiometricsLog> logs);
    }
}