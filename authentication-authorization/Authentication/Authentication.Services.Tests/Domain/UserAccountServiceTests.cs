using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Authentication.Core.Enums;
using Authentication.Core.Extensions;
using Authentication.Core.Interfaces.Authentication;
using Authentication.Core.Models.Domain.Accounts;
using Authentication.Services.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;
using NUnit.Framework;
using AppContext = Authentication.Data.AppContext;

namespace Authentication.Services.Tests.Domain
{
    public class UserAccountServiceTests
    {
        private readonly IServiceCollection _serviceCollection;
        protected IServiceProvider ServiceProvider { get; private set; }
        protected Guid? CurrentUserId = null;
        protected AppContext Db => ServiceProvider.GetService<AppContext>();
        private UserAccountService Service => ServiceProvider.GetService<UserAccountService>();

        public UserAccountServiceTests()
        {
            _serviceCollection = new ServiceCollection();
        }

        [SetUp]
        public void BaseInit()
        {
            _serviceCollection.TryAddTransient<UserAccountService>();
            _serviceCollection.AddLogging();

            var authedUserMock = new Mock<IApplicationUserResolver>();
            authedUserMock.Setup(p => p.GetUserId()).Returns(() => CurrentUserId ?? Guid.Empty);
            _serviceCollection.TryAddSingleton(authedUserMock.Object);
            
            _serviceCollection.TryAddSingleton<UserAccountService>();
            
            _serviceCollection.AddDbContext<AppContext>(options =>
                options.UseInMemoryDatabase(Guid.NewGuid().ToString()));
            _serviceCollection.AddDistributedMemoryCache();

            ServiceProvider = _serviceCollection.BuildServiceProvider();
        }

        [Test]
        public async Task GetCurrentUserAsync_WhenUserIsSuperAdmin_ReturnsAllPermissionsAsync()
        {
            var currentUser = new User()
            {
                Role = Role.SuperAdmin
            };
            currentUser.SetCreatedAndEnabled();

            Db.Users.Add(currentUser);
            await Db.SaveChangesAsync();

            CurrentUserId = currentUser.Id;

            var allPermissions = Enum.GetValues(typeof(Permission));

            var result = await Service.GetCurrentUserAsync();
            Assert.NotNull(result);
            Assert.AreEqual(Role.SuperAdmin, result.Role);
            Assert.AreEqual(allPermissions.Length, result.Permissions.Count);
        }
        
        [Test]
        public async Task GetCurrentUserAsync_WhenUserIsAdminWithNoDeniedPermissions_ReturnsUserWithAdminPermissionsAsync()
        {
            var currentUser = new User()
            {
                Role = Role.Admin
            };
            currentUser.SetCreatedAndEnabled();

            Db.Users.Add(currentUser);
            await Db.SaveChangesAsync();

            CurrentUserId = currentUser.Id;

            var adminPermissions = await Db.RolePermissions.Where(p => p.Role == Role.Admin).ToListAsync();

            var result = await Service.GetCurrentUserAsync();
            Assert.NotNull(result);
            Assert.AreEqual(Role.Admin, result.Role);
            Assert.AreEqual(adminPermissions.Count, result.Permissions.Count);
        }
        
        [Test]
        public async Task GetCurrentUserAsync_WhenUserIsAdminWithDeniedDeletePermissions_ReturnsUserWithoutDeletePermissionAsync()
        {
            var currentUser = new User()
            {
                Role = Role.Admin,
                UserPermissions = new List<UserPermission>()
                {
                    new ()
                    {
                        Permission = Permission.CanCreateUser,
                        IsEnabled = true
                    },
                    new ()
                    {
                        Permission = Permission.CanDeleteUser,
                        IsEnabled = false
                    }
                }
            };
            currentUser.SetCreatedAndEnabled();

            Db.Users.Add(currentUser);
            await Db.SaveChangesAsync();

            CurrentUserId = currentUser.Id;

            var result = await Service.GetCurrentUserAsync();
            Assert.NotNull(result);
            Assert.AreEqual(Role.Admin, result.Role);
            Assert.True(result.Permissions.Contains(Permission.CanCreateUser));
            Assert.False(result.Permissions.Contains(Permission.CanDeleteUser));
        }
        
        [Test]
        public async Task GetCurrentUserAsync_WhenUserIsRoleUser_ReturnsUserWithOnlyEnabledPermissionsAsync()
        {
            var currentUser = new User()
            {
                Role = Role.User,
                UserPermissions = new List<UserPermission>()
                {
                    new ()
                    {
                        Permission = Permission.CanCreateUser,
                        IsEnabled = true
                    },
                    new ()
                    {
                        Permission = Permission.CanDeleteUser,
                        IsEnabled = false
                    }
                }
            };
            currentUser.SetCreatedAndEnabled();

            Db.Users.Add(currentUser);
            await Db.SaveChangesAsync();

            CurrentUserId = currentUser.Id;

            var result = await Service.GetCurrentUserAsync();
            Assert.NotNull(result);
            Assert.AreEqual(Role.User, result.Role);
            Assert.AreEqual(1, result.Permissions.Count);
            Assert.True(result.Permissions.Contains(Permission.CanCreateUser));
            Assert.False(result.Permissions.Contains(Permission.CanDeleteUser));
        }
    }
}