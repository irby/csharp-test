using System.Linq;
using System.Threading.Tasks;
using Authentication.Core.Enums;
using Authentication.Core.Exceptions;
using Authentication.Core.Extensions;
using Authentication.Core.Models.Domain.Accounts;
using Authentication.Core.Models.Dto;
using Authentication.Core.Utilities;
using Authentication.Data;
using Authentication.Services.Domain.Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Authentication.Services.Domain
{
    public class UserService : AuthenticatedDomainServiceBase<UserService>
    {
        public UserService(AppContext context, ILoggerFactory loggerFactory, UserAccountService userAccountService) : base(context, loggerFactory, userAccountService)
        {
        }
        
        public async Task<UserResponseDto> CreateUserAsync(CreateUserDto createUserDto)
        {
            ModelValidator.Validate(true, createUserDto.Email, createUserDto.Password, createUserDto.FirstName, createUserDto.LastName);

            if (!CredentialValidationUtil.IsValidPassword(createUserDto.Password))
            {
                throw new BadRequestException(ErrorCode.InvalidPassword);
            }
            
            if (!CredentialValidationUtil.IsValidEmail(createUserDto.Email))
            {
                throw new BadRequestException(ErrorCode.InvalidEmail);
            }
            
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
                HashedPassword = HashUtil.HashPassword(createUserDto.Password),
                Role = Role.User
            };
            
            user.SetCreatedAndEnabled();

            await Db.Users.AddAsync(user);
            await Db.SaveChangesAsync();
            
            var rolePermissions = await Db.RolePermissions.Where(p => p.Role == user.Role && p.IsEnabled)
                .Select(p => p.Permission).ToListAsync();
            
            return new UserResponseDto()
            {
                Id = user.Id,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                UserRole = user.Role,
                Permissions = rolePermissions
                    .Union(user.UserPermissions.Select(p => p.Permission))
                    .ToList()
            };
        }

        public async Task UpdatePasswordAsync(string username, string currentPassword, string newPassword)
        {
            ModelValidator.Validate(true, username, currentPassword, newPassword);

            if (currentPassword == newPassword)
            {
                throw new BadRequestException(ErrorCode.NewPasswordEqualsOldPassword);
            }

            if (!CredentialValidationUtil.IsValidPassword(newPassword))
            {
                throw new BadRequestException(ErrorCode.InvalidPassword);
            }
            
            var currentUser = await GetCurrentUserAsync();

            var user = await Db.Users.FirstOrDefaultAsync(p => p.Email == username.Trim().ToLower()) ??
                       throw new UnprocessableEntityException(ErrorCode.AccountNotFound);

            if (user.Id != currentUser.Id && !currentUser.HasPermissions(Permission.CanUpdateUserPassword))
            {
                throw new NotAuthorizedException();
            }
            
            if (!user.IsEnabled)
            {
                throw new UnprocessableEntityException(ErrorCode.AccountDisabled);
            }

            if (!HashUtil.Validate(currentPassword, user.HashedPassword))
            {
                throw new UnprocessableEntityException(ErrorCode.IncorrectPassword);
            }

            user.HashedPassword = HashUtil.HashPassword(newPassword);
            user.SetModified(currentUser.Id, true);

            await Db.SaveChangesAsync();
        }
    }
}