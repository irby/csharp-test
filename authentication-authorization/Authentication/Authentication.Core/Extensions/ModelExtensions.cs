using System;
using Authentication.Core.Interfaces;
using Authentication.Core.Models.Domain;

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
    }
}