using SCICHRPortal.Core.Interfaces;
using SCICHRPortal.Data.Entities;
using SCICHRPortal.Data.Entities.Metadatas;
using SCICHRPortal.Utility.Interface;
namespace SCICHRPortal.Service.Interfaces
{
    public interface ITimekeepingAdminSetupService :
        IScopedService,
        IRetriever<TimekeepingAdminSetup, int>
    {
        Task<bool> UpdateAsync(TimekeepingAdminSetup timekeepingAdminSetup);
    }
}
