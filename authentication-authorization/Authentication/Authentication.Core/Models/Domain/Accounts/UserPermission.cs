using System;
using Authentication.Core.Enums;

namespace Authentication.Core.Models.Domain.Accounts
{
    public class UserPermission : ActivatableEntity
    {
        public Guid UserId { get; set; }
        public virtual User User { get; set; }
        public Permission Permission { get; set; }
    }
}