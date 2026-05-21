using SCICHRPortal.Core.Interfaces;
using SCICHRPortal.Data.DTOs;
using SCICHRPortal.Data.Entities.Metadatas;
using SCICHRPortal.Data.TimekeepingTables;
using SCICHRPortal.Utility.Interface;
using System;
using System.Collections.Generic;
using System.Text;

namespace SCICHRPortal.Repository.Interfaces
{
    public interface ITimekeepingDevicesRepository : IRepository,
        IScopedService,
         IInserter<TimekeepingDevices>,
         IRetriever<TimekeepingDevices, int>,
         IListRetriever<TimekeepingDevices>
    {
        Task<bool> DeleteAsync(int Id);
        Task<bool> UpdateAsync(TimekeepingDevices timekeepingDevices);
        Task<Tuple<IEnumerable<TimekeepingDevices>, int>> FilterAsync(int pageNumber, int pageSize, string searchKeyword);
        Task<DuplicateMessage> HasDuplicateName(TimekeepingDevices timekeepingDevices);
        Task<IEnumerable<ZKDevices>> GetDevices();
        Task<TimekeepingDevices> GetBySerialNumber(string?  serialNumber);
    }
}
