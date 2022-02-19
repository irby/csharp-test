using Microsoft.AspNetCore.Authorization;

namespace Authentication.Api.Authentication
{
    public sealed class IsValidUserRequirement : IAuthorizationRequirement
    {
    }
}