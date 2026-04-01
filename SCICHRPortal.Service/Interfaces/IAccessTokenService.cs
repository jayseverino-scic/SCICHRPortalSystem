
using SCICHRPortal.Data.Entities;
using SCICHRPortal.Utility.Cryptography.Models;
using SCICHRPortal.Utility.Interface;

namespace SCICHRPortal.Service.Interfaces
{
    public interface IAccessTokenService : IScopedService
    {
        Task<AccessToken> Generate(User user, IEnumerable<Role> roles);
    }
}
