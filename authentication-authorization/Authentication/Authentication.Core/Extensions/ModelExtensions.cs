using System;
using Authentication.Core.Enums;
using Authentication.Core.Interfaces;
using Authentication.Core.Models.Domain;
using Authentication.Core.Models.Domain.Accounts;

namespace Authentication.Core.Extensions
{
    public static class ModelExtensions
    {
        public static void SetCreated(this ICreatableEntity model)
        {
            model.CreatedOn = DateTimeOffset.UtcNow;
        }
        
        public static void SetCreatedAndEnabled(this ActivatableEntity model, Guid? id = null)
        {
            model.CreatedOn = DateTimeOffset.UtcNow;
            model.CreatedBy = id;
            model.IsEnabled = true;
        }
        
        public static void SetModified(this ActivatableEntity model, Guid? id, bool isEnabled)
        {
            model.ModifiedOn = DateTimeOffset.UtcNow;
            model.ModifiedBy = id;
            model.IsEnabled = isEnabled;
        }

        /// <summary>
        /// Returns true if user has all permissions passed in, returns false otherwise
        /// </summary>
        /// <param name="user"></param>
        /// <param name="permissions"></param>
        /// <returns></returns>
        public static bool HasPermissions(this User user, params Permission[] permissions)
        {
            foreach (var permission in permissions)
            {
                if (!user.Permissions.Contains(permission))
                    return false;
            }

            return true;
        }
        
        /// <summary>
        /// Returns true if user has any of the permissions passed in, returns false otherwise
        /// </summary>
        /// <param name="user"></param>
        /// <param name="permissions"></param>
        /// <returns></returns>
        public static bool HasAnyPermissions(this User user, params Permission[] permissions)
        {
            foreach (var permission in permissions)
            {
                if (user.Permissions.Contains(permission))
                    return true;
            }

            return false;
        }
    }
}