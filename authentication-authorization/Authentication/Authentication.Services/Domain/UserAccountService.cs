using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Authentication.Core.Enums;
using Authentication.Core.Exceptions;
using Authentication.Core.Extensions;
using Authentication.Core.Interfaces.Authentication;
using Authentication.Core.Models.Domain.Accounts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using AppContext = Authentication.Data.AppContext;

namespace Authentication.Services.Domain
{
    public sealed class UserAccountService
    {
        private readonly AppContext _db;
        private readonly IDistributedCache _cache;
        private readonly ILogger<UserAccountService> _log;
        private readonly IApplicationUserResolver _userResolver;
        
        public UserAccountService(AppContext context, IDistributedCache cache, ILoggerFactory loggerFactory, IApplicationUserResolver userResolver)
        {
            _db = context;
            _cache = cache;
            _log = loggerFactory.CreateLogger<UserAccountService>();
            _userResolver = userResolver;
        }
        
        public async Task<User> GetCurrentUserAsync()
        {
            var user = await GetUserAsync(_userResolver.GetUserId());

            if (!user.IsEnabled)
            {
                throw new NotAuthorizedException(ErrorCode.AccountDisabled);
            }

            return user;
        }
        
        public async Task<User> GetUserAsync(Guid id)
        {
            _log.LogDebug($"Checking cache for user '{id}'");
            User user = null;

            try
            {
                user = await _cache.GetAsync<User>(id.ToString());
            } catch (Exception ex)
            {
                _log.LogError(ex, "Issue retrieving user from cache");
            }
            

            if (user == null)
            {
                _log.LogDebug($"User '{id}' not found in cache");

                user = await _db.Users
                        .Include(p => p.UserPermissions)
                        .FirstOrDefaultAsync(p => p.Id == id)
                       ?? throw new NotAuthorizedException();

                user.Permissions = new List<Permission>();

                if (user.Role == Role.SuperAdmin)
                {
                    var allPermissions = Enum.GetValues(typeof(Permission)).Cast<Permission>().ToList();
                    user.Permissions = allPermissions;
                }
                else
                {
                    var rolePermissions = await _db.RolePermissions.Where(p => p.Role == user.Role).Select(p => p.Permission).ToListAsync();
                    var enabledPermissions = user.UserPermissions.Where(p => p.IsEnabled).Select(p => p.Permission).ToList();
                    var deniedPermissions = user.UserPermissions.Where(p => !p.IsEnabled).Select(p => p.Permission).ToList();
                    var permissions = rolePermissions.Union(enabledPermissions).Where(p => !deniedPermissions.Contains(p)).ToList();
                    user.Permissions = permissions;
                }

                try
                { 
                    await CacheUserAsync(user);
                } 
                catch (Exception ex)
                {
                    _log.LogError(ex, "Error occurred saving user to cache");
                }
            }
            else
            {
                _log.LogDebug($"User '{id}' found in cache");
            }

            return user;
        }
        
        public async Task CacheUserAsync(User user)
        {
            // expire the cached user entry every 5 minutes
            var expiration = DateTimeOffset.UtcNow.AddMinutes(5);

            await _cache.SetAsync(user.Id.ToString(), user, new DistributedCacheEntryOptions { AbsoluteExpiration = expiration });

            _log.LogDebug($"User '{user.Id}' added to cache. Expiration set to {expiration}");
        }
    }
}