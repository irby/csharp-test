using System;
using System.Linq;
using System.Security.Claims;
using Authentication.Core.Exceptions;
using Authentication.Core.Interfaces.Authentication;
using Microsoft.AspNetCore.Http;

namespace Authentication.Api.Authentication
{
    public class WebApplicationUserResolver : IApplicationUserResolver
    {
        private readonly IHttpContextAccessor _contextAccessor;

        public WebApplicationUserResolver(IHttpContextAccessor contextAccessor)
        {
            _contextAccessor = contextAccessor;
        }

        public Guid? TryGetUserId() =>
            Guid.TryParse(_contextAccessor.HttpContext.User?.Claims
                ?.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier)
                ?.Value ?? "", out var parsedId)
                ? parsedId
                : (Guid?) null;

        public Guid GetUserId() => TryGetUserId() ?? throw new NotAuthenticatedException();
    }
}