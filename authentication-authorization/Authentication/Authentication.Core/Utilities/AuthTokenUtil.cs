using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Authentication.Core.Models.Authentication;
using Authentication.Core.Models.Dto;
using Microsoft.IdentityModel.Tokens;

namespace Authentication.Core.Utilities
{
    public static class AuthTokenUtil
    {
        public static AuthenticationToken CreateToken(UserResponseDto user, SigningCredentials credentials)
        {
            var expirationDate = DateTime.UtcNow.AddHours(5);
            var handler = new JwtSecurityTokenHandler();
            var descriptor = new SecurityTokenDescriptor()
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Email, user.Email)
                }),
                Expires = expirationDate,
                SigningCredentials = credentials
            };
            var securityToken = handler.CreateToken(descriptor);
            return new AuthenticationToken()
            {
                Token = handler.WriteToken(securityToken),
                Expiration = expirationDate,
                User = user
            };
        }
    }
}