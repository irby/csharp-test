using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DependencyInjection.Tests
{
    [TestClass]
    public class ServiceATest : BaseServiceTest<ServiceA>
    {
        public override void Init(IServiceCollection services)
        {
            services.TryAddTransient<SharedService>();
        }

        [TestMethod]
        public void ServiceA_WhenGetNumber_ReturnsNumber()
        {
            var result = Service.GetServiceValue();
            Assert.AreEqual(10, result);
        }
    }
}