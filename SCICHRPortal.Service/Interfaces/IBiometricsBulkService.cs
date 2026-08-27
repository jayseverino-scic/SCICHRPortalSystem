using SCICHRPortal.Data.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SCICHRPortal.Service.Interfaces
{
    public interface IBiometricsBulkService
    {
        /// <summary>
        /// Bulk insert biometrics logs
        /// </summary>
        Task BulkInsertBiometricsLogsAsync(List<BiometricsLog> logs);

        /// <summary>
        /// Bulk insert with transaction support
        /// </summary>
        Task BulkInsertWithTransactionAsync(List<BiometricsLog> logs);

        /// <summary>
        /// Bulk insert with result tracking
        /// </summary>
        Task<BulkImportResult> BulkInsertWithResultAsync(List<BiometricsLog> logs);

        /// <summary>
        /// Bulk insert with progress reporting
        /// </summary>
        Task<BulkImportResult> BulkInsertWithProgressAsync(List<BiometricsLog> logs, IProgress<BulkProgress> progress);
    }

    public class BulkImportResult
    {
        public int TotalInserted { get; set; }
        public int TotalFailed { get; set; }
        public long ElapsedMilliseconds { get; set; }
        public int BatchCount { get; set; }
        public List<string> Errors { get; set; } = new List<string>();
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
    }

    public class BulkProgress
    {
        public int ProcessedRows { get; set; }
        public int TotalRows { get; set; }
        public int PercentageComplete => TotalRows > 0 ? (int)((double)ProcessedRows / TotalRows * 100) : 0;
        public string Status { get; set; } = string.Empty;
    }
}