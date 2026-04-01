using SCICHRPortal.Core.Interfaces;
using SCICHRPortal.Data.Entities.Metadatas;
using SCICHRPortal.Utility.Interface;

namespace SCICHRPortal.Repository.Interfaces
{
    public interface ITimekeepingAdminSetupRepository : IRepository,
        IScopedService,
        IRetriever<TimekeepingAdminSetup, int>
    {
        Task<bool> UpdateAsync(TimekeepingAdminSetup TimekeepingAdminSetup);
    }
}
