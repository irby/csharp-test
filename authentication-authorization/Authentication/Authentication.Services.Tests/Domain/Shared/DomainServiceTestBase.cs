using System;
using Authentication.Core.Exceptions;
using Authentication.Core.Interfaces.Authentication;
using Authentication.Services.Domain;
using Authentication.Services.Domain.Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;
using NUnit.Framework;
using AppContext = Authentication.Data.AppContext;

namespace Authentication.Services.Tests.Domain.Shared
{
    [TestFixture]
    public abstract class DomainServiceTestBase<T> where T : DomainServiceBase<T>
    {
        private readonly IServiceCollection _serviceCollection;
        protected IServiceProvider ServiceProvider { get; private set; }
        protected T Service => ServiceProvider.GetService<T>();
        protected Guid? CurrentUserId = null;
        protected AppContext Db => ServiceProvider.GetService<AppContext>();

        protected DomainServiceTestBase()
        {
            _serviceCollection = new ServiceCollection();
        }

        [SetUp]
        public void BaseInit()
        {
            _serviceCollection.TryAddTransient<T>();
            _serviceCollection.AddLogging();

            var authedUserMock = new Mock<IApplicationUserResolver>();
            authedUserMock.Setup(p => p.GetUserId()).Returns(() => CurrentUserId ?? Guid.Empty);
            _serviceCollection.TryAddSingleton(authedUserMock.Object);
            
            _serviceCollection.TryAddSingleton<UserAccountService>();
            
            _serviceCollection.AddDbContext<AppContext>(options =>
                options.UseInMemoryDatabase(Guid.NewGuid().ToString()));
            _serviceCollection.AddDistributedMemoryCache();

            Init(_serviceCollection);

            ServiceProvider = _serviceCollection.BuildServiceProvider();
        }

        public abstract void Init(IServiceCollection services);
    }
}