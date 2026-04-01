using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using SCICHRPortal.Utility;
using SCICHRPortal.Data.Entities;
using SCICHRPortal.Utility.Cryptography.Interfaces;
using SCICHRPortal.Utility.Cryptography.Models;

namespace SCICHRPortal.Utility.Cryptography
{
    public class JsonWebTokenGenerator : IJsonWebTokenGenerator
    {
        private string JWTSecretKey { get; }
        private IRefreshTokenGenerator RefreshTokenGenerator { get; }
        public JsonWebTokenGenerator(string jwtSecretKey, IRefreshTokenGenerator refreshTokenGenerator)
        {
            JWTSecretKey = jwtSecretKey;
            RefreshTokenGenerator = refreshTokenGenerator;
        }
        public AccessToken GenerateToken(User user, string[] roles)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(JWTSecretKey);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Sid, user.UserId.ToString()),
            };

            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            claims.Add(new Claim(ClaimTypes.Name, $"{user.FirstName} {user.LastName}"));

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims.ToArray()),
                Expires = DateTime.UtcNow.AddHours(8),
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            var refreshToken = RefreshTokenGenerator.GenerateToken();

            return new AccessToken
            {
                JsonWebToken = tokenHandler.WriteToken(token),
                RefreshToken = refreshToken
            };
        }

        public AccessToken GenerateToken(IEnumerable<Claim> claims)
        {
            throw new NotImplementedException();
        }

        public ClaimsPrincipal GetClaimsPrincipal(string jsonWebToken, out SecurityToken validationToken)
        {
            throw new NotImplementedException();
        }
    }
}
