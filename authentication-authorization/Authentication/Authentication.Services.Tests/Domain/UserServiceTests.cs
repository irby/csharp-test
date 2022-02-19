using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Authentication.Core.Enums;
using Authentication.Core.Exceptions;
using Authentication.Core.Extensions;
using Authentication.Core.Models.Domain.Accounts;
using Authentication.Core.Models.Dto;
using Authentication.Core.Utilities;
using Authentication.Services.Domain;
using Authentication.Services.Tests.Domain.Shared;
using Microsoft.EntityFrameworkCore;
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
        public async Task CreateUserAsync_WhenUserDoesNotExistAndModelIsValid_CreatesUserAsync()
        {
            var createUserDto = new UserCreateDto()
            {
                Email = "jane.doe@test.com",
                FirstName = "Jane",
                LastName = "Doe",
                Password = "HelloWorld1!"
            };
            
            var user = await Service.CreateUserAsync(createUserDto);
            Assert.NotNull(user);
            Assert.AreNotEqual(new Guid(), user.Id);
            Assert.NotNull(await Db.Users.FirstOrDefaultAsync(p => p.Id == user.Id && p.IsEnabled));
        }
        
        [Test]
        public async Task CreateUserAsync_WhenPasswordIsNotValid_ThrowsBadRequestExceptionAsync()
        {
            var createUserDto = new UserCreateDto()
            {
                Email = "jane.doe@test.com",
                FirstName = "Jane",
                LastName = "Doe",
                Password = "helloworld1"
            };

            var exception = Assert.ThrowsAsync<BadRequestException>(() => Service.CreateUserAsync(createUserDto));
            Assert.AreEqual(ErrorCode.InvalidPassword, exception.ErrorCode);
        }
        
        [Test]
        public async Task CreateUserAsync_WhenEmailIsNotValid_ThrowsBadRequestExceptionAsync()
        {
            var createUserDto = new UserCreateDto()
            {
                Email = "jane.doe",
                FirstName = "Jane",
                LastName = "Doe",
                Password = "HelloWorld1!"
            };

            var exception = Assert.ThrowsAsync<BadRequestException>(() => Service.CreateUserAsync(createUserDto));
            Assert.AreEqual(ErrorCode.InvalidEmail, exception.ErrorCode);
        }

        [Test]
        public async Task CreateUserAsync_WhenUserAlreadyExists_ThrowsUnprocessableEntityExceptionAsync()
        {
            Db.Users.Add(new User()
            {
                Email = "jane.doe@test.com"
            });
            await Db.SaveChangesAsync();
            
            var createUserDto = new UserCreateDto()
            {
                Email = "jane.doe@test.com",
                FirstName = "Jane",
                LastName = "Doe",
                Password = "HelloWorld1!"
            };
            
            var exception = Assert.ThrowsAsync<UnprocessableEntityException>(() => Service.CreateUserAsync(createUserDto));
            Assert.AreEqual(ErrorCode.UserAlreadyExists, exception.ErrorCode);
        }

        [Test]
        public async Task UpdateUserPasswordAsync_WhenUserIsMakingRequestToUpdatePassword_AllowsUpdateAsync()
        {
            var oldPassword = "hello123";
            var newPassword = "FooBarBaz1!";
            
            var user = new User
            {
                Email = "jane.doe@test.com",
                HashedPassword = HashUtil.HashPassword(oldPassword)
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
                HashedPassword = HashUtil.HashPassword(oldPassword)
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
                HashedPassword = HashUtil.HashPassword(oldPassword)
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
                HashedPassword = HashUtil.HashPassword(oldPassword)
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
                HashedPassword = HashUtil.HashPassword(oldPassword)
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
                HashedPassword = HashUtil.HashPassword(oldPassword)
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
                HashedPassword = HashUtil.HashPassword(oldPassword)
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