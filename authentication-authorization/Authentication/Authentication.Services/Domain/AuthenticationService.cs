using System;
using System.Linq;
using System.Threading.Tasks;
using Authentication.Core.Enums;
using Authentication.Core.Exceptions;
using Authentication.Core.Extensions;
using Authentication.Core.Models.Domain.Accounts;
using Authentication.Core.Models.Domain.Auditing;
using Authentication.Core.Models.Dto;
using Authentication.Core.Utilities;
using Authentication.Services.Domain.Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using AppContext = Authentication.Data.AppContext;
using ModelValidator = Authentication.Core.Utilities.ModelValidator;

namespace Authentication.Services.Domain
{
    public sealed class AuthenticationService : DomainServiceBase<AuthenticationService>
    {
        private const int NUMBER_OF_ALLOWED_LOGIN_ATTEMPTS = 5;
        
        public AuthenticationService(AppContext context, ILoggerFactory loggerFactory) : base(context, loggerFactory)
        {
        }

        public async Task<UserResponseDto> LoginUserAsync(string username, string password)
        {
            var user = await Db.Users.Where(p => p.Email.Trim().ToLower() == username.Trim().ToLower())
                .Include(p => p.UserPermissions)
                .FirstOrDefaultAsync() ?? throw new UnprocessableEntityException(ErrorCode.UsernamePasswordNotValid);

            var auditRecord = new UserLoginAuditRecord()
            {
                UserId = user.Id
            };
            auditRecord.SetCreated();
            await Db.AddAsync(auditRecord);

            if (!user.IsEnabled)
            {
                var errorCode = ErrorCode.AccountDisabled;
                auditRecord.ErrorCode = errorCode;
                await Db.SaveChangesAsync();
                throw new UnprocessableEntityException(errorCode);
            }

            if (user.NumberOfLoginFailures >= NUMBER_OF_ALLOWED_LOGIN_ATTEMPTS)
            {
                var errorCode = ErrorCode.AccountLocked;
                auditRecord.ErrorCode = errorCode;
                await Db.SaveChangesAsync();
                throw new UnprocessableEntityException(errorCode);
            }

            if (!HashUtil.Validate(password, user.HashedPassword))
            {
                var errorCode = ErrorCode.UsernamePasswordNotValid;
                auditRecord.ErrorCode = errorCode;
                user.NumberOfLoginFailures += 1;
                await Db.SaveChangesAsync();
                throw new UnprocessableEntityException(errorCode);
            }

            auditRecord.IsSuccess = true;
            user.NumberOfLoginFailures = 0;
            await Db.SaveChangesAsync();

            var rolePermissions = await Db.RolePermissions.Where(p => p.Role == user.Role && p.IsEnabled)
                .Select(p => p.Permission).ToListAsync();

            return new UserResponseDto()
            {
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                UserRole = user.Role,
                Permissions = rolePermissions
                    .Union(user.UserPermissions.Select(p => p.Permission))
                    .ToList()
            };
        }

        public async Task<UserResponseDto> CreateUserAsync(CreateUserDto createUserDto)
        {
            ModelValidator.Validate(true, createUserDto.Email, createUserDto.Password, createUserDto.FirstName, createUserDto.LastName);
            var user = await Db.Users.Where(p => p.Email.Trim().ToLower() == createUserDto.Email.Trim().ToLower())
                .Include(p => p.UserPermissions)
                .FirstOrDefaultAsync();

            if (user != null)
                throw new UnprocessableEntityException(ErrorCode.UserAlreadyExists);

            user = new User()
            {
                Email = createUserDto.Email.Trim().ToLower(),
                FirstName = createUserDto.FirstName.Trim().ToLower(),
                LastName = createUserDto.LastName.Trim().ToLower(),
                HashedPassword = HashUtil.HashPassword(createUserDto.Password, HashUtil.GetSalt()),
                Role = Role.User
            };
            
            user.SetCreatedAndEnabled();

            await Db.SaveChangesAsync();
            
            var rolePermissions = await Db.RolePermissions.Where(p => p.Role == user.Role && p.IsEnabled)
                .Select(p => p.Permission).ToListAsync();
            
            return new UserResponseDto()
            {
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                UserRole = user.Role,
                Permissions = rolePermissions
                    .Union(user.UserPermissions.Select(p => p.Permission))
                    .ToList()
            };
        }
    }
}