using SCICHRPortal.Data.Entities;
using SCICHRPortal.Data.Entities.Metadatas;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SCICHRPortal.Service.Interfaces
{
    public interface IBiometricsBulkService
    {
        Task BulkInsertBiometricsLogsAsync(List<BiometricsLog> logs);
        Task BulkInsertWithTransactionAsync(List<BiometricsLog> logs);
        Task<BulkImportResult> BulkInsertWithResultAsync(List<BiometricsLog> logs);
        Task<BulkImportResult> BulkInsertWithProgressAsync(List<BiometricsLog> logs, IProgress<BulkProgress> progress);
    }
}