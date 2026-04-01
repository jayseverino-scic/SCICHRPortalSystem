using SCICHRPortal.Utility.Cryptography.Interfaces;

namespace SCICHRPortal.Utility.Cryptography
{
    public class RefreshTokenGenerator : IRefreshTokenGenerator
    {
        public string GenerateToken()
        {
            return Guid.NewGuid().ToString();
        }
    }
}
