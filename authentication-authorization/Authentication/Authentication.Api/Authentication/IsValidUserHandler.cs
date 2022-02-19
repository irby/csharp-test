using System.Threading.Tasks;
using Authentication.Services.Domain;
using Microsoft.AspNetCore.Authorization;

namespace Authentication.Api.Authentication
{
    public class IsValidUserHandler : AuthorizationHandler<IsValidUserRequirement>
    {
        private readonly UserAccountService _userAccountService;

        public IsValidUserHandler(UserAccountService userAccountService)
        {
            _userAccountService = userAccountService;
        }

        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, IsValidUserRequirement requirement)
        {
            await _userAccountService.GetCurrentUserAsync();
            context.Succeed(requirement);
        }
    }
}