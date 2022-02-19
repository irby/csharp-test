using Authentication.Data;
using Microsoft.Extensions.Logging;

namespace Authentication.Services.Domain.Shared
{
    public abstract class DomainServiceBase<T>
    {
        protected readonly AppContext Db;
        protected readonly ILogger<T> Logger;
        
        protected DomainServiceBase(AppContext context, ILoggerFactory loggerFactory)
        {
            Db = context;
            Logger = loggerFactory?.CreateLogger<T>();
        }
    }
}