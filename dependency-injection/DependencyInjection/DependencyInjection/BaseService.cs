using Microsoft.Extensions.Logging;

namespace DependencyInjection
{
    public abstract class BaseService<T>
    {
        protected readonly SharedService Service;
        protected readonly ILogger<T> Log;
        public BaseService(SharedService service, ILoggerFactory loggerFactory)
        {
            Service = service;
            Log = loggerFactory?.CreateLogger<T>();
        }
    }
}