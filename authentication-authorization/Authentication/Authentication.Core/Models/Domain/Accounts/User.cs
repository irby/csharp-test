using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;
using Authentication.Core.Enums;
using Authentication.Core.Models.Domain;

namespace Authentication.Core.Models.Domain.Accounts
{
    public class User : ActivatableEntity
    {
        [NotNull]
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string HashedPassword { get; set; }
        public Role Role { get; set; }
        public virtual ICollection<UserPermission> UserPermissions { get; set; } = new List<UserPermission>();
        [NotMapped]
        public ICollection<Permission> Permissions { get; set; }
        public int NumberOfLoginFailures { get; set; }
    }
}