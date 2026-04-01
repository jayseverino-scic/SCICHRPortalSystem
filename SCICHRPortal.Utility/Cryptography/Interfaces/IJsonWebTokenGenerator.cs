using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using SCICHRPortal.Data.Entities;
using SCICHRPortal.Utility.Cryptography.Models;

namespace SCICHRPortal.Utility.Cryptography.Interfaces
{
    public interface IJsonWebTokenGenerator
    {
        AccessToken GenerateToken(User user, string[] roles);

        AccessToken GenerateToken(IEnumerable<Claim> claims);

        ClaimsPrincipal GetClaimsPrincipal(string jsonWebToken, out SecurityToken validationToken);
    }
}
