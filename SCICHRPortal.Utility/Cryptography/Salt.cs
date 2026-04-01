using System.Security.Cryptography;

namespace SCICHRPortal.Utility.Cryptography
{
    public class Salt
    {
        public static string Create()
        {
            byte[] randomBytes = new byte[128 / 8];

            using var generator = RandomNumberGenerator.Create();

            generator.GetBytes(randomBytes);

            return Convert.ToBase64String(randomBytes);
        }
    }
}
