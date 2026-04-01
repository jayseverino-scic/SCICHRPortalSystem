using SCICHRPortal.Data.Entities;
using SCICHRPortal.Service.Interfaces;
using SCICHRPortal.Utility.Cryptography.Interfaces;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Models = SCICHRPortal.Utility.Cryptography.Models;
using SCICHRPortal.Utility.Cryptography.Models;

namespace SCICHRPortal.Service.Implementations
{
    public class AccessTokenService : IAccessTokenService
    {
        private IJsonWebTokenGenerator JsonWebTokenGenerator { get; }
        //private IRefreshTokenRepository RefreshTokenRepository { get; }

        public AccessTokenService(IJsonWebTokenGenerator jsonWebTokenGenerator)
        {
            JsonWebTokenGenerator = jsonWebTokenGenerator;
        }

        public async Task<AccessToken> 
            Generate(User user, IEnumerable<Role> roles)
        {
            var accessToken = JsonWebTokenGenerator.GenerateToken(user, roles.Select(ur => ur.Name!).ToArray());
            //var refreshTokenRecord = await RefreshTokenRepository.GetByUserIdAsync(userId);

            //if (refreshTokenRecord != null)
            //{
            //    refreshTokenRecord.Token = accessToken.RefreshToken;
            //    await RefreshTokenRepository.UpdateAsync(refreshTokenRecord);
            //}
            //else
            //{
            //    await RefreshTokenRepository.InsertAsync(new RefreshToken
            //    {
            //        UserId = userId,
            //        Token = accessToken.RefreshToken
            //    });
            //}

            return accessToken!;
        }
    }
}
