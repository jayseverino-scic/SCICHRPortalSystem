using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SCICHRPortal.Data;
using SCICHRPortal.Data.Entities;
using SCICHRPortal.Data.Entities.Metadatas;
using SCICHRPortal.Data.Repositories.Interfaces;
using SCICHRPortal.Repository;
using SCICHRPortal.Repository.Implementations;
using SCICHRPortal.Service.Interfaces;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace SCICHRPortal.Service.Implementations
{
    public class BiometricsBulkService : IBiometricsBulkRepository
    {
        private IBiometricsBulkRepository BiometricsBulkRepository { get; }

        private BiometricsBulkService(IBiometricsBulkRepository biometricsBulkRepository)
        {
            BiometricsBulkRepository = biometricsBulkRepository;
        }
        public async Task BulkInsertAsync(DataTable dataTable)
        {
            await BiometricsBulkRepository.BulkInsertAsync(dataTable);
        }
        public async Task BulkInsertBiometricsLogsAsync(List<BiometricsLog> logs)
        {
            await BiometricsBulkRepository.BulkInsertBiometricsLogsAsync(logs);
        }

        public async Task<BulkImportResult> BulkInsertWithResultAsync(List<BiometricsLog> logs)
        {
            return await BiometricsBulkRepository.BulkInsertWithResultAsync(logs);
        }

        public async Task<BulkImportResult> BulkInsertWithProgressAsync(
            List<BiometricsLog> logs,
            IProgress<BulkProgress> progress)
        {
            return await BiometricsBulkRepository.BulkInsertWithProgressAsync(logs, progress);
        }
    }
}