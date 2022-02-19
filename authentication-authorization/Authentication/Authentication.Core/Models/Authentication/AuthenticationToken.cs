using System;
using Authentication.Core.Models.Dto;

namespace Authentication.Core.Models.Authentication
{
    public sealed class AuthenticationToken
    {
        public string Token { get; set; }
        public DateTimeOffset Expiration { get; set; }
        public UserResponseDto User { get; set; }
    }
}