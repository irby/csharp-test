using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Authentication.Core.Enums;
using Authentication.Core.Exceptions;
using Authentication.Core.Extensions;
using Authentication.Core.Interfaces;
using Authentication.Core.Models.Domain.Accounts;
using Authentication.Core.Models.Dto;
using Authentication.Core.Utilities;
using Authentication.Services.Domain.Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using AppContext = Authentication.Data.AppContext;

namespace Authentication.Services.Domain.Admin
{
    public class AdminUserService : AuthenticatedDomainServiceBase<AdminUserService>
    {
        public AdminUserService(AppContext context, ILoggerFactory loggerFactory, UserAccountService userAccountService) : base(context, loggerFactory, userAccountService)
        {
        }

        public async Task<ICollection<UserResponseDto>> GetUsersAsync()
        {
            var currentUser = await GetCurrentUserAsync();
            if (!currentUser.HasPermissions(Permission.CanListUsers))
            {
                throw new NotAuthorizedException();
            }

            return await Db.Users.Select(p => new UserResponseDto()
                {
                    Id = p.Id,
                    Email = p.Email,
                    FirstName = p.FirstName,
                    LastName = p.LastName,
                    IsEnabled = p.IsEnabled,
                    UserRole = p.Role
                })
                .ToListAsync();
        }
        
        public async Task<UserResponseDto> GetUserAsync(Guid id)
        {
            var currentUser = await GetCurrentUserAsync();
            if (!currentUser.HasPermissions(Permission.CanViewUser))
            {
                throw new NotAuthorizedException();
            }

            return await Db.Users.Select(p => new UserResponseDto()
                {
                    Id = p.Id,
                    Email = p.Email,
                    FirstName = p.FirstName,
                    LastName = p.LastName,
                    IsEnabled = p.IsEnabled,
                    UserRole = p.Role,
                    LastModifiedOn = p.ModifiedOn
                })
                .Where(p => p.Id == id)
                .FirstOrDefaultAsync() 
                   ?? throw new UnprocessableEntityException(ErrorCode.AccountNotFound);
        }

        public async Task<UserResponseDto> CreateUserAsync(CreateUserDto dto)
        {
            ModelValidator.Validate(true, dto.Email, dto.FirstName, dto.LastName, dto.Password);
            
            var currentUser = await GetCurrentUserAsync();

            if (!currentUser.HasPermissions(Permission.CanCreateUser))
            {
                throw new NotAuthorizedException();
            }

            var existingUser = await Db.Users.FirstOrDefaultAsync(p => p.Email.Trim().ToLower() == dto.Email.Trim().ToLower());
            if (existingUser != null)
            {
                throw new UnprocessableEntityException(ErrorCode.UserAlreadyExists);
            }

            var user = new User
            {
                Email = dto.Email,
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                HashedPassword = HashUtil.HashPassword(dto.Password),
                Role = dto.Role
            };

            var rolePermissions = await Db.RolePermissions.Where(p => p.Role == user.Role && p.IsEnabled)
                .Select(p => p.Permission)
                .ToListAsync();

            foreach (var permission in dto.UserPermissions)
            {
                if (rolePermissions.Contains(permission.Permission))
                    continue;

                if (!permission.IsEnabled)
                    continue;
                
                permission.SetCreatedAndEnabled(currentUser.Id);
                user.UserPermissions.Add(permission);
            }
            
            user.SetCreatedAndEnabled(currentUser.Id);

            await Db.Users.AddAsync(user);
            await Db.SaveChangesAsync();

            return new UserResponseDto()
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                IsEnabled = true,
                LastModifiedOn = DateTimeOffset.UtcNow,
                Permissions = rolePermissions.Union(user.UserPermissions.Select(p => p.Permission)).Distinct().ToList(),
                UserRole = user.Role
            };
        }
        
        public async Task<UserResponseDto> UpdateUserAsync(UserUpdateDto dto)
        {
            var currentUser = await GetCurrentUserAsync();
            if (!currentUser.HasPermissions(Permission.CanUpdateUser))
            {
                throw new NotAuthorizedException();
            }

            ModelValidator.Validate(true, dto.Email, dto.FirstName, dto.LastName, dto.UserPermissions, dto.UserRole);

            var user = await Db.Users.FirstOrDefaultAsync(p => p.Id == dto.Id)
                       ?? throw new UnprocessableEntityException(ErrorCode.AccountNotFound);

            user.Email = dto.Email.Trim().ToLower();
            user.FirstName = dto.FirstName;
            user.LastName = dto.LastName;
            user.Role = dto.UserRole;
            user.IsEnabled = dto.IsEnabled;

            var rolePermissions = await Db.RolePermissions.Where(p => p.Role == user.Role && p.IsEnabled).Select(p => p.Permission).ToListAsync();

            // Iterate through permission on dto. Modify existing permissions and create new permissions if present
            foreach (var permission in dto.UserPermissions)
            {
                var existingPermission = user.UserPermissions.FirstOrDefault(p => p.Permission == permission.Permission);
                if (existingPermission != null && existingPermission.IsEnabled != permission.IsEnabled)
                {
                    existingPermission.SetModified(currentUser.Id, permission.IsEnabled);
                }
                else
                {
                    // If permission is not enabled, don't create a record for it
                    if (!permission.IsEnabled)
                        continue;

                    // If permission is specific to the role and user didn't have it before, don't create a record
                    if (rolePermissions.Contains(permission.Permission))
                        continue;
                    
                    permission.SetCreatedAndEnabled(currentUser.Id);
                    user.UserPermissions.Add(permission);
                }
            }

            // For each permission that not included on DTO that are on user, deactivate
            foreach (var permission in user.UserPermissions)
            {
                var dtoPermissions = dto.UserPermissions.Select(p => p.Permission).ToList();
                if (!dtoPermissions.Contains(permission.Permission))
                {
                    permission.SetModified(currentUser.Id, false);
                }
            }
            
            user.SetModified(currentUser.Id, user.IsEnabled);

            await Db.SaveChangesAsync();
            
            return new UserResponseDto()
            {
                Id = user.Id,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                IsEnabled = user.IsEnabled,
                UserRole = user.Role,
                LastModifiedOn = user.ModifiedOn
            };
        }
    }
}