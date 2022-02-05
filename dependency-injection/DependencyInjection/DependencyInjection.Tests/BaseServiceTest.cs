using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DependencyInjection.Tests
{
    public abstract class BaseServiceTest<T> where T : BaseService<T>
    {
        private readonly IServiceCollection _serviceCollection;
        protected IServiceProvider ServiceProvider { get; private set; }
        protected T Service => ServiceProvider.GetService<T>();

        protected BaseServiceTest()
        {
            _serviceCollection = new ServiceCollection();
        }

        [TestInitialize]
        public void BaseInit()
        {
            _serviceCollection.TryAddTransient<T>();
            _serviceCollection.AddLogging();
            
            Init(_serviceCollection);

            ServiceProvider = _serviceCollection.BuildServiceProvider();
        }

        public abstract void Init(IServiceCollection services);
    }
}