using System;
using System.Linq;
using System.Threading.Tasks;
using Authentication.Core.Enums;
using Authentication.Core.Exceptions;
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
    public class AuthenticationServiceTests : DomainServiceTestBase<AuthenticationService>
    {
        public override void Init(IServiceCollection services)
        {
        }

        [Test]
        public async Task LoginUserAsync_WhenUsernameAndPasswordAreValid_ReturnsUserAsync()
        {
            var user = new User()
            {
                Email = "jane.doe@test.com",
                HashedPassword = HashUtil.HashPassword("hello123"),
                IsEnabled = true
            };
            Db.Users.Add(user);
            await Db.SaveChangesAsync();

            var result = await Service.LoginUserAsync("jane.doe@test.com", "hello123");
            Assert.NotNull(result);
            Assert.AreEqual("jane.doe@test.com", result.Email);
            Assert.AreEqual(0, user.NumberOfLoginFailures);
        }
        
        [Test]
        public async Task LoginUserAsync_WhenUsernameAndPasswordAreValid_ResetsNumberOfLoginFailures()
        {
            var user = new User()
            {
                Email = "jane.doe@test.com",
                HashedPassword = HashUtil.HashPassword("hello123"),
                IsEnabled = true,
                NumberOfLoginFailures = 4
            };
            Db.Users.Add(user);
            await Db.SaveChangesAsync();

            await Service.LoginUserAsync("jane.doe@test.com", "hello123");
            Assert.AreEqual(0, user.NumberOfLoginFailures);
        }
        
        [Test]
        public async Task LoginUserAsync_WhenUsernameAndPasswordAreValid_CreatesSuccessfulLoginAuditRecord()
        {
            var user = new User()
            {
                Email = "jane.doe@test.com",
                HashedPassword = HashUtil.HashPassword("hello123"),
                IsEnabled = true
            };
            Db.Users.Add(user);
            await Db.SaveChangesAsync();

            await Service.LoginUserAsync("jane.doe@test.com", "hello123");
            var auditRecords =  await Db.UserLoginAuditRecords.Where(p => p.UserId == user.Id).ToListAsync();
            
            Assert.AreEqual(1, auditRecords.Count);
            Assert.AreEqual(true, auditRecords.First().IsSuccess);
        }
        
        [Test]
        public async Task LoginUserAsync_WhenUsernameIsValidButPasswordIsInvalid_ThrowsUnprocessableEntityAsync()
        {
            var user = new User()
            {
                Email = "jane.doe@test.com",
                HashedPassword = HashUtil.HashPassword("hello123"),
                IsEnabled = true
            };
            Db.Users.Add(user);
            await Db.SaveChangesAsync();

            var exception = Assert.ThrowsAsync<UnprocessableEntityException>(() => Service.LoginUserAsync("jane.doe@test.com", "Hello123"));
            Assert.AreEqual(ErrorCode.UsernamePasswordNotValid, exception.ErrorCode);
            
            var auditRecords =  await Db.UserLoginAuditRecords.Where(p => p.UserId == user.Id).ToListAsync();
            
            Assert.AreEqual(1, auditRecords.Count);
            Assert.AreEqual(false, auditRecords.First().IsSuccess);
            Assert.AreEqual(ErrorCode.UsernamePasswordNotValid, auditRecords.First().ErrorCode);
            
            Assert.AreEqual(1, user.NumberOfLoginFailures);
        }
        
        [Test]
        public async Task LoginUserAsync_WhenUsernameDoesNotExist_ThrowsUnprocessableEntityAsync()
        {
            var exception = Assert.ThrowsAsync<UnprocessableEntityException>(() => Service.LoginUserAsync("bob.doe@test.com", "Hello123"));
            Assert.AreEqual(ErrorCode.UsernamePasswordNotValid, exception.ErrorCode);
            
            var auditRecords =  await Db.UserLoginAuditRecords.ToListAsync();
            
            Assert.AreEqual(0, auditRecords.Count);
        }
        
        [Test]
        public async Task LoginUserAsync_WhenUserIsDisabled_ThrowsUnprocessableEntityAsync()
        {
            var user = new User()
            {
                Email = "jane.doe@test.com",
                HashedPassword = HashUtil.HashPassword("hello123"),
                IsEnabled = false
            };
            Db.Users.Add(user);
            await Db.SaveChangesAsync();

            var exception = Assert.ThrowsAsync<UnprocessableEntityException>(() => Service.LoginUserAsync("jane.doe@test.com", "Hello123"));
            Assert.AreEqual(ErrorCode.AccountDisabled, exception.ErrorCode);
            
            var auditRecords =  await Db.UserLoginAuditRecords.Where(p => p.UserId == user.Id).ToListAsync();
            
            Assert.AreEqual(1, auditRecords.Count);
            Assert.AreEqual(false, auditRecords.First().IsSuccess);
            Assert.AreEqual(ErrorCode.AccountDisabled, auditRecords.First().ErrorCode);
        }
        
        [Test]
        public async Task LoginUserAsync_WhenUserLoginAttemptsExceedsLimit_ThrowsUnprocessableEntityAsync()
        {
            var user = new User()
            {
                Email = "jane.doe@test.com",
                HashedPassword = HashUtil.HashPassword("hello123"),
                IsEnabled = true,
                NumberOfLoginFailures = 5
            };
            Db.Users.Add(user);
            await Db.SaveChangesAsync();

            var exception = Assert.ThrowsAsync<UnprocessableEntityException>(() => Service.LoginUserAsync("jane.doe@test.com", "Hello123"));
            Assert.AreEqual(ErrorCode.AccountLocked, exception.ErrorCode);
            
            var auditRecords =  await Db.UserLoginAuditRecords.Where(p => p.UserId == user.Id).ToListAsync();
            
            Assert.AreEqual(1, auditRecords.Count);
            Assert.AreEqual(false, auditRecords.First().IsSuccess);
            Assert.AreEqual(ErrorCode.AccountLocked, auditRecords.First().ErrorCode);
        }
    }
}