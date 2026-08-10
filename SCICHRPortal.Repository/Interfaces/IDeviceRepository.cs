using SCICHRPortal.Core.Interfaces;
using SCICHRPortal.Data.DTOs;
using SCICHRPortal.Data.Entities.Metadatas;
using SCICHRPortal.Utility.Interface;
using System;
using System.Collections.Generic;
using System.Text;

namespace SCICHRPortal.Repository.Interfaces
{
    public interface IDeviceRepository : IRepository,
        IScopedService,
         IInserter<Device>,
         IRetriever<Device, int>,
         IListRetriever<Device>
    {
        Task<bool> DeleteAsync(int deviceId);
        Task<bool> UpdateAsync(Device entity);
        Task<Tuple<IEnumerable<Device>, int>> FilterAsync(int pageNumber, int pageSize, string searchKeyword);
        Task<DuplicateMessage> HasDuplicateName(Device device);
    }
}
