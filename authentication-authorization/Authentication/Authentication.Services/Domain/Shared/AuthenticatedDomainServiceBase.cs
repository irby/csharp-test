using System.Threading.Tasks;
using Authentication.Core.Models.Domain.Accounts;
using Authentication.Data;
using Microsoft.Extensions.Logging;

namespace Authentication.Services.Domain.Shared
{
    public class AuthenticatedDomainServiceBase<T> : DomainServiceBase<T>
    {
        private readonly UserAccountService _userAccountService;
        public AuthenticatedDomainServiceBase(AppContext context, ILoggerFactory loggerFactory, UserAccountService userAccountService) : base(context, loggerFactory)
        {
            _userAccountService = userAccountService;
        }

        public async Task<User> GetCurrentUserAsync() => await _userAccountService.GetCurrentUserAsync();
    }
}