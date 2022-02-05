using Microsoft.Extensions.Logging;

namespace DependencyInjection
{
    public class ServiceA : BaseService<ServiceA>
    {
        public ServiceA(SharedService service, ILoggerFactory loggerFactory) : base(service, loggerFactory)
        {
            
        }

        public int GetServiceValue()
        {
            Log.LogInformation("Inside GetServiceValue");
            return Service.GetNumber();
        }
    }
}