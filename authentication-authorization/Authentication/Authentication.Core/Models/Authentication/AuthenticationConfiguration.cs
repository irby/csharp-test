using Microsoft.IdentityModel.Tokens;

namespace Authentication.Core.Models.Authentication
{
    public class AuthenticationConfiguration
    {
        public SigningCredentials SigningCredentials { get; set; }
    }
}