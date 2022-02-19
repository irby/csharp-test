using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Authentication.Core.Enums;
using Authentication.Core.Exceptions;
using Authentication.Core.Extensions;
using Authentication.Core.Models.Domain.Accounts;
using Authentication.Core.Models.Dto;
using Authentication.Core.Utilities;
using Authentication.Services.Domain.Admin;
using Authentication.Services.Tests.Domain.Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace Authentication.Services.Tests.Domain.Admin
{
    [TestFixture]
    public class AdminUserServiceTests : DomainServiceTestBase<AdminUserService>
    {
        public override void Init(IServiceCollection services)
        {
        }

        [Test]
        public async Task GetUsersAsync_WhenCurrentUserHasPermissionToListUsers_ReturnsUsersAsync()
        {
            var currentUser = new User()
            {
                UserPermissions = new List<UserPermission>()
                {
                    new ()
                    {
                        Permission = Permission.CanListUsers,
                        IsEnabled = true
                    }
                }
            };
            currentUser.SetCreatedAndEnabled();
            Db.Users.Add(currentUser);
            await Db.SaveChangesAsync();

            CurrentUserId = currentUser.Id;

            var currentCount = await Db.Users.CountAsync();

            var result = await Service.GetUsersAsync();
            Assert.AreEqual(currentCount, result.Count);
        }
        
        [Test]
        public async Task GetUsersAsync_WhenCurrentUserCanListUserPermissionDisabled_ThrowsExceptionAsync()
        {
            var currentUser = new User()
            {
                UserPermissions = new List<UserPermission>()
                {
                    new ()
                    {
                        Permission = Permission.CanListUsers,
                        IsEnabled = false
                    }
                }
            };
            currentUser.SetCreatedAndEnabled();
            Db.Users.Add(currentUser);
            await Db.SaveChangesAsync();

            CurrentUserId = currentUser.Id;

            Assert.ThrowsAsync<NotAuthorizedException>(() => Service.GetUsersAsync());
        }
        
        [Test]
        public async Task GetUsersAsync_WhenCurrentUserMissingListUserPermission_ThrowsExceptionAsync()
        {
            var currentUser = new User()
            {
                UserPermissions = new List<UserPermission>()
                {
                }
            };
            currentUser.SetCreatedAndEnabled();
            Db.Users.Add(currentUser);
            await Db.SaveChangesAsync();

            CurrentUserId = currentUser.Id;

            Assert.ThrowsAsync<NotAuthorizedException>(() => Service.GetUsersAsync());
        }
        
        [Test]
        public async Task GetUserAsync_WhenUserExists_ReturnsUserAsync()
        {
            var currentUser = new User()
            {
                UserPermissions = new List<UserPermission>()
                {
                    new ()
                    {
                        Permission = Permission.CanViewUser,
                        IsEnabled = true
                    }
                }
            };
            currentUser.SetCreatedAndEnabled();

            var user = new User()
            {
            };
            user.SetCreatedAndEnabled();
            
            Db.Users.Add(currentUser);
            Db.Users.Add(user);
            await Db.SaveChangesAsync();

            CurrentUserId = currentUser.Id;

            var result = await Service.GetUserAsync(user.Id);
            Assert.NotNull(result);
        }
        
        [Test]
        public async Task GetUserAsync_WhenUserIsNotEnabled_ReturnsUserAsync()
        {
            var currentUser = new User()
            {
                UserPermissions = new List<UserPermission>()
                {
                    new ()
                    {
                        Permission = Permission.CanViewUser,
                        IsEnabled = true
                    }
                }
            };
            currentUser.SetCreatedAndEnabled();

            var user = new User()
            {
                IsEnabled = false
            };

            Db.Users.Add(currentUser);
            Db.Users.Add(user);
            await Db.SaveChangesAsync();

            CurrentUserId = currentUser.Id;

            var result = await Service.GetUserAsync(user.Id);
            Assert.NotNull(result);
        }
        
        [Test]
        public async Task GetUserAsync_WhenUserDoesNotExist_ThrowsUnprocessableEntityExceptionAsync()
        {
            var currentUser = new User()
            {
                UserPermissions = new List<UserPermission>()
                {
                    new ()
                    {
                        Permission = Permission.CanViewUser,
                        IsEnabled = true
                    }
                }
            };
            currentUser.SetCreatedAndEnabled();

            Db.Users.Add(currentUser);
            await Db.SaveChangesAsync();

            CurrentUserId = currentUser.Id;

            var exception = Assert.ThrowsAsync<UnprocessableEntityException>(() => Service.GetUserAsync(Guid.Empty));
            Assert.AreEqual(ErrorCode.AccountNotFound, exception!.ErrorCode);
        }
        
        [Test]
        public async Task GetUserAsync_WhenCurrentUserMissingViewUserPermission_ThrowsNotAuthorizedExceptionAsync()
        {
            var currentUser = new User()
            {
                UserPermissions = new List<UserPermission>()
                {
                    new ()
                    {
                    }
                }
            };
            currentUser.SetCreatedAndEnabled();

            Db.Users.Add(currentUser);
            await Db.SaveChangesAsync();

            CurrentUserId = currentUser.Id;

            Assert.ThrowsAsync<NotAuthorizedException>(() => Service.GetUserAsync(Guid.Empty));
        }
        
        [Test]
        public async Task UpdateUserAsync_WhenUserExists_UpdatesUserAsync()
        {
            var currentUser = new User()
            {
                UserPermissions = new List<UserPermission>()
                {
                    new ()
                    {
                        Permission = Permission.CanUpdateUser,
                        IsEnabled = true
                    }
                }
            };
            currentUser.SetCreatedAndEnabled();

            var user = new User()
            {
                Email = "john.doe@test.com",
                FirstName = "John",
                LastName = "Doe",
                IsEnabled = false,
                Role = Role.Admin,
                UserPermissions = new List<UserPermission>()
                {
                    new ()
                    {
                        Permission = Permission.CanListUsers,
                        IsEnabled = true
                    },
                    new ()
                    {
                        Permission = Permission.CanViewUser,
                        IsEnabled = false
                    },
                    new ()
                    {
                        Permission = Permission.CanUpdateUser,
                        IsEnabled = true
                    }
                }
            };

            Db.Users.Add(currentUser);
            Db.Users.Add(user);
            await Db.SaveChangesAsync();

            CurrentUserId = currentUser.Id;

            var dto = new UserUpdateDto()
            {
                Id = user.Id,
                Email = "jim.dean@test.com",
                FirstName = "Jim",
                LastName = "Dean",
                IsEnabled = true,
                UserRole = Role.Moderator,
                UserPermissions = new List<UserPermission>()
                {
                    new ()
                    {
                        Permission = Permission.CanListUsers,
                        IsEnabled = true
                    },
                    new ()
                    {
                        Permission = Permission.CanViewUser,
                        IsEnabled = true
                    },
                    new ()
                    {
                        Permission = Permission.CanCreateUser,
                        IsEnabled = true
                    }
                }
            };

            var result = await Service.UpdateUserAsync(dto);
            Assert.NotNull(result);
            Assert.AreEqual("jim.dean@test.com", user.Email);
            Assert.AreEqual("Jim", user.FirstName);
            Assert.AreEqual("Dean", user.LastName);
            Assert.AreEqual(true, user.IsEnabled);
            Assert.AreEqual(Role.Moderator, user.Role);
            Assert.AreEqual(4, user.UserPermissions.Count);
            Assert.AreEqual(currentUser.Id, user.ModifiedBy);
            Assert.NotNull(user.ModifiedOn);
            
            Assert.NotNull(user.UserPermissions.FirstOrDefault(p => p.Permission == Permission.CanListUsers && p.IsEnabled));
            Assert.NotNull(user.UserPermissions.FirstOrDefault(p => p.Permission == Permission.CanViewUser && p.IsEnabled));
            Assert.NotNull(user.UserPermissions.FirstOrDefault(p => p.Permission == Permission.CanCreateUser && p.IsEnabled));
            
            Assert.NotNull(user.UserPermissions.FirstOrDefault(p => p.Permission == Permission.CanUpdateUser && !p.IsEnabled));
        }
        
        [Test]
        public async Task UpdateUserAsync_WhenNewPermissionGivenIsPartOfUsersRole_DontCreatePermissionRecord()
        {
            var currentUser = new User()
            {
                UserPermissions = new List<UserPermission>()
                {
                    new ()
                    {
                        Permission = Permission.CanUpdateUser,
                        IsEnabled = true
                    }
                }
            };
            currentUser.SetCreatedAndEnabled();

            var user = new User()
            {
                Email = "john.doe@test.com",
                FirstName = "John",
                LastName = "Doe",
                IsEnabled = false,
                Role = Role.User
            };

            Db.Users.Add(currentUser);
            Db.Users.Add(user);
            await Db.SaveChangesAsync();

            CurrentUserId = currentUser.Id;

            var dto = new UserUpdateDto()
            {
                Id = user.Id,
                Email = "jim.dean@test.com",
                FirstName = "Jim",
                LastName = "Dean",
                IsEnabled = true,
                UserRole = Role.Admin,
                UserPermissions = new List<UserPermission>()
                {
                    new ()
                    {
                        Permission = Permission.CanViewUser,
                        IsEnabled = true
                    }
                }
            };

            var result = await Service.UpdateUserAsync(dto);
            Assert.NotNull(result);
            Assert.IsNull(user.UserPermissions.FirstOrDefault(p => p.Permission == Permission.CanViewUser));
        }
        
        [Test]
        public async Task UpdateUserAsync_WhenPermissionIsTakenAwayFromRoleChange_RemovesPermission()
        {
            var currentUser = new User()
            {
                UserPermissions = new List<UserPermission>()
                {
                    new ()
                    {
                        Permission = Permission.CanUpdateUser,
                        IsEnabled = true
                    }
                }
            };
            currentUser.SetCreatedAndEnabled();

            var user = new User()
            {
                Email = "john.doe@test.com",
                FirstName = "John",
                LastName = "Doe",
                IsEnabled = false,
                Role = Role.Admin,
                UserPermissions = new List<UserPermission>()
                {
                    new UserPermission()
                    {
                        Permission = Permission.CanViewUser,
                        IsEnabled = true
                    }
                }
            };

            Db.Users.Add(currentUser);
            Db.Users.Add(user);
            await Db.SaveChangesAsync();

            CurrentUserId = currentUser.Id;

            var dto = new UserUpdateDto()
            {
                Id = user.Id,
                Email = "jim.dean@test.com",
                FirstName = "Jim",
                LastName = "Dean",
                IsEnabled = true,
                UserRole = Role.User
            };

            var result = await Service.UpdateUserAsync(dto);
            Assert.NotNull(result);
            Assert.IsNull(user.UserPermissions.FirstOrDefault(p => p.Permission == Permission.CanViewUser && p.IsEnabled));
        }
        
        [Test]
        public async Task UpdateUserAsync_WhenPermissionIsGivenToUser_OnlyModifiesRecordsThatHaveChanged()
        {
            var currentUser = new User()
            {
                UserPermissions = new List<UserPermission>()
                {
                    new ()
                    {
                        Permission = Permission.CanUpdateUser,
                        IsEnabled = true
                    }
                }
            };
            currentUser.SetCreatedAndEnabled();

            var user = new User()
            {
                Email = "john.doe@test.com",
                FirstName = "John",
                LastName = "Doe",
                IsEnabled = false,
                Role = Role.User,
                UserPermissions = new List<UserPermission>()
                {
                    new UserPermission()
                    {
                        Permission = Permission.CanViewUser,
                        IsEnabled = true
                    },
                    new UserPermission()
                    {
                        Permission = Permission.CanUpdateUser,
                        IsEnabled = false
                    }
                }
            };

            Db.Users.Add(currentUser);
            Db.Users.Add(user);
            await Db.SaveChangesAsync();

            CurrentUserId = currentUser.Id;

            var dto = new UserUpdateDto()
            {
                Id = user.Id,
                Email = "jim.dean@test.com",
                FirstName = "Jim",
                LastName = "Dean",
                IsEnabled = true,
                UserRole = Role.User,
                UserPermissions = new List<UserPermission>()
                {
                    new UserPermission()
                    {
                        Permission = Permission.CanViewUser,
                        IsEnabled = true
                    },
                    new UserPermission()
                    {
                        Permission = Permission.CanUpdateUser,
                        IsEnabled = true
                    }
                }
                
            };

            var result = await Service.UpdateUserAsync(dto);
            Assert.NotNull(result);
            Assert.IsNull(user.UserPermissions.First(p => p.Permission == Permission.CanViewUser).ModifiedOn);
            Assert.IsTrue(user.UserPermissions.First(p => p.Permission == Permission.CanViewUser).IsEnabled);
            Assert.IsNotNull(user.UserPermissions.First(p => p.Permission == Permission.CanUpdateUser).ModifiedOn);
            Assert.IsTrue(user.UserPermissions.First(p => p.Permission == Permission.CanUpdateUser).IsEnabled);
        }
        
        [Test]
        public async Task UpdateUserAsync_WhenPassedAnUpdate_DoesNotUpdatePassword()
        {
            var currentUser = new User()
            {
                UserPermissions = new List<UserPermission>()
                {
                    new ()
                    {
                        Permission = Permission.CanUpdateUser,
                        IsEnabled = true
                    }
                }
            };
            currentUser.SetCreatedAndEnabled();

            var user = new User()
            {
                Email = "john.doe@test.com",
                FirstName = "John",
                LastName = "Doe",
                IsEnabled = false,
                Role = Role.User,
                HashedPassword = HashUtil.HashPassword("hello123")
            };

            Db.Users.Add(currentUser);
            Db.Users.Add(user);
            await Db.SaveChangesAsync();

            CurrentUserId = currentUser.Id;

            var dto = new UserUpdateDto()
            {
                Id = user.Id,
                Email = "jim.dean@test.com",
                FirstName = "Jim",
                LastName = "Dean",
                IsEnabled = true,
                UserRole = Role.User
                
            };

            var beforePassword = user.HashedPassword;

            var result = await Service.UpdateUserAsync(dto);
            Assert.True(user.HashedPassword == beforePassword);
        }
        
        [Test]
        public async Task UpdateUserAsync_WhenUserDoesNotExist_ThrowsUnprocessableEntityExceptionAsync()
        {
            var currentUser = new User()
            {
                UserPermissions = new List<UserPermission>()
                {
                    new ()
                    {
                        Permission = Permission.CanUpdateUser,
                        IsEnabled = true
                    }
                }
            };
            currentUser.SetCreatedAndEnabled();

            Db.Users.Add(currentUser);
            await Db.SaveChangesAsync();

            CurrentUserId = currentUser.Id;

            var dto = new UserUpdateDto()
            {
                Id = Guid.Empty,
                Email = "jim.dean@test.com",
                FirstName = "Jim",
                LastName = "Dean",
                IsEnabled = true,
                UserRole = Role.Moderator,
                UserPermissions = new List<UserPermission>()
                {
                    new ()
                    {
                        Permission = Permission.CanListUsers,
                        IsEnabled = true
                    },
                    new ()
                    {
                        Permission = Permission.CanViewUser,
                        IsEnabled = true
                    },
                    new ()
                    {
                        Permission = Permission.CanCreateUser,
                        IsEnabled = true
                    }
                }
            };

            var exception = Assert.ThrowsAsync<UnprocessableEntityException>(() => Service.UpdateUserAsync(dto));
            Assert.AreEqual(ErrorCode.AccountNotFound, exception!.ErrorCode);
        }
    }
}