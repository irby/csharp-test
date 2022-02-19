using System;

namespace Authentication.Core.Interfaces.Authentication
{
    public interface IApplicationUserResolver
    {
        Guid GetUserId();
    }
}