using System;
using System.Collections.Generic;
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
            ModelValidator.Validate(true, username, password);
            
            var user = await Db.Users.Where(p => p.Email.Trim().ToLower() == username.Trim().ToLower())
                .Include(p => p.UserPermissions)
                .FirstOrDefaultAsync() ?? throw new UnprocessableEntityException(ErrorCode.UsernamePasswordNotValid);

            var auditRecord = new UserLoginAuditRecord()
            {
                UserId = user.Id
            };
            auditRecord.SetCreated();
            await Db.UserLoginAuditRecords.AddAsync(auditRecord);

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

            var rolePermissions = new List<Permission>();

            if (user.Role == Role.SuperAdmin)
            {
                rolePermissions.AddRange(Enum.GetValues(typeof(Permission)).Cast<Permission>());
            }
            else
            {
                rolePermissions.AddRange(await Db.RolePermissions.Where(p => p.Role == user.Role && p.IsEnabled)
                    .Select(p => p.Permission).ToListAsync());
            }

            return new UserResponseDto()
            {
                Id = user.Id,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                UserRole = user.Role,
                IsEnabled = user.IsEnabled,
                Permissions = rolePermissions
                    .Union(user.UserPermissions.Select(p => p.Permission))
                    .ToList()
            };
        }

        
    }
}