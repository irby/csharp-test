using System.Collections.Generic;
using System.Threading.Tasks;
using Authentication.Core.Enums;
using Authentication.Core.Exceptions;
using Authentication.Core.Extensions;
using Authentication.Core.Models.Domain.Accounts;
using Authentication.Core.Utilities;
using Authentication.Services.Domain;
using Authentication.Services.Tests.Domain.Shared;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace Authentication.Services.Tests.Domain
{
    public class UserServiceTests : DomainServiceTestBase<UserService>
    {
        public override void Init(IServiceCollection services)
        {
        }

        [Test]
        public async Task UpdateUserPasswordAsync_WhenUserIsMakingRequestToUpdatePassword_AllowsUpdateAsync()
        {
            var oldPassword = "hello123";
            var newPassword = "FooBarBaz1!";
            
            var user = new User
            {
                Email = "jane.doe@test.com",
                HashedPassword = HashUtil.HashPassword(oldPassword, HashUtil.GetSalt())
            };
            user.SetCreatedAndEnabled();
            await Db.Users.AddAsync(user);
            await Db.SaveChangesAsync();

            CurrentUserId = user.Id;

            await Service.UpdatePasswordAsync(user.Email, oldPassword, newPassword);
            Assert.IsTrue(HashUtil.Validate(newPassword, user.HashedPassword));
        }
        
        [Test]
        public async Task UpdateUserPasswordAsync_WhenUserMakingRequestToUpdatePasswordIsNotUserAndHasPermissionToUpdatePassword_AllowsUpdateAsync()
        {
            var currentUser = new User()
            {
                Email = "hello@test.com",
                UserPermissions = new List<UserPermission>()
                {
                    new()
                    {
                        Permission = Permission.CanUpdateUserPassword,
                        IsEnabled = true
                    }
                }
            };
            
            var oldPassword = "hello123";
            var newPassword = "FooBarBaz1!";
            
            var user = new User
            {
                Email = "jane.doe@test.com",
                HashedPassword = HashUtil.HashPassword(oldPassword, HashUtil.GetSalt())
            };

            currentUser.SetCreatedAndEnabled();
            user.SetCreatedAndEnabled();
            await Db.Users.AddAsync(currentUser);
            await Db.Users.AddAsync(user);
            await Db.SaveChangesAsync();

            CurrentUserId = currentUser.Id;

            await Service.UpdatePasswordAsync(user.Email, oldPassword, newPassword);
            Assert.IsTrue(HashUtil.Validate(newPassword, user.HashedPassword));
            Assert.AreEqual(currentUser.Id, user.ModifiedBy.Value);
        }
        
        [Test]
        public async Task UpdateUserPasswordAsync_WhenUserMakingRequestToUpdatePasswordIsNotUserAndDoesNotHavePermissionToUpdatePassword_ThrowsUnauthorizedExceptionAsync()
        {
            var currentUser = new User()
            {
                Email = "hello@test.com"
            };
            
            var oldPassword = "hello123";
            var newPassword = "FooBarBaz1!";
            
            var user = new User
            {
                Email = "jane.doe@test.com",
                HashedPassword = HashUtil.HashPassword(oldPassword, HashUtil.GetSalt())
            };

            currentUser.SetCreatedAndEnabled();
            user.SetCreatedAndEnabled();
            await Db.Users.AddAsync(currentUser);
            await Db.Users.AddAsync(user);
            await Db.SaveChangesAsync();

            CurrentUserId = currentUser.Id;

            var exception = Assert.ThrowsAsync<NotAuthorizedException>(() => Service.UpdatePasswordAsync(user.Email, oldPassword, newPassword));
            Assert.IsTrue(HashUtil.Validate(oldPassword, user.HashedPassword));
        }
        
        [Test]
        public async Task UpdateUserPasswordAsync_WhenNewPasswordIsNotValidPassword_ThrowsBadRequestExceptionAsync()
        {
            var oldPassword = "hello123";
            var newPassword = "foobar";
            
            var user = new User
            {
                Email = "jane.doe@test.com",
                HashedPassword = HashUtil.HashPassword(oldPassword, HashUtil.GetSalt())
            };
            user.SetCreatedAndEnabled();
            await Db.Users.AddAsync(user);
            await Db.SaveChangesAsync();

            CurrentUserId = user.Id;

            var exception = Assert.ThrowsAsync<BadRequestException>(() => Service.UpdatePasswordAsync(user.Email, oldPassword, newPassword));
            Assert.IsTrue(HashUtil.Validate(oldPassword, user.HashedPassword));
            Assert.AreEqual(ErrorCode.InvalidPassword, exception.ErrorCode);
        }
        
        [Test]
        public async Task UpdateUserPasswordAsync_WhenOldPasswordAndNewPasswordMatch_ThrowsBadRequestExceptionAsync()
        {
            var oldPassword = "hello123";
            var newPassword = "hello123";
            
            var user = new User
            {
                Email = "jane.doe@test.com",
                HashedPassword = HashUtil.HashPassword(oldPassword, HashUtil.GetSalt())
            };
            user.SetCreatedAndEnabled();
            await Db.Users.AddAsync(user);
            await Db.SaveChangesAsync();

            CurrentUserId = user.Id;

            var exception = Assert.ThrowsAsync<BadRequestException>(() => Service.UpdatePasswordAsync(user.Email, oldPassword, newPassword));
            Assert.IsTrue(HashUtil.Validate(oldPassword, user.HashedPassword));
            Assert.AreEqual(ErrorCode.NewPasswordEqualsOldPassword, exception.ErrorCode);
        }
        
        [Test]
        public async Task UpdateUserPasswordAsync_WhenOldPasswordDoesNotMatchUserPassword_ThrowsUnprocessableEntityAsync()
        {
            var oldPassword = "hello123";
            var newPassword = "FooBarBaz1!";
            
            var user = new User
            {
                Email = "jane.doe@test.com",
                HashedPassword = HashUtil.HashPassword(oldPassword, HashUtil.GetSalt())
            };
            user.SetCreatedAndEnabled();
            await Db.Users.AddAsync(user);
            await Db.SaveChangesAsync();

            CurrentUserId = user.Id;

            var exception = Assert.ThrowsAsync<UnprocessableEntityException>(() => Service.UpdatePasswordAsync(user.Email, "otherPassword", newPassword));
            Assert.IsTrue(HashUtil.Validate(oldPassword, user.HashedPassword));
            Assert.AreEqual(ErrorCode.IncorrectPassword, exception.ErrorCode);
        }
        
        [Test]
        public async Task UpdateUserPasswordAsync_WhenUserIsDisabled_ThrowsUnprocessableEntityAsync()
        {
            var currentUser = new User()
            {
                Email = "hello@test.com",
                UserPermissions = new List<UserPermission>()
                {
                    new()
                    {
                        Permission = Permission.CanUpdateUserPassword,
                        IsEnabled = true
                    }
                }
            };
            
            var oldPassword = "hello123";
            var newPassword = "FooBarBaz1!";
            
            var user = new User
            {
                Email = "jane.doe@test.com",
                HashedPassword = HashUtil.HashPassword(oldPassword, HashUtil.GetSalt())
            };
            currentUser.SetCreatedAndEnabled();
            await Db.Users.AddAsync(user);
            await Db.Users.AddAsync(currentUser);
            await Db.SaveChangesAsync();

            CurrentUserId = currentUser.Id;

            var exception = Assert.ThrowsAsync<UnprocessableEntityException>(() => Service.UpdatePasswordAsync(user.Email, oldPassword, newPassword));
            Assert.IsTrue(HashUtil.Validate(oldPassword, user.HashedPassword));
            Assert.AreEqual(ErrorCode.AccountDisabled, exception.ErrorCode);
        }
    }
}