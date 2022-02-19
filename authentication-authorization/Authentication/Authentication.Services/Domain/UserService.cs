using System.Threading.Tasks;
using Authentication.Core.Enums;
using Authentication.Core.Exceptions;
using Authentication.Core.Extensions;
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