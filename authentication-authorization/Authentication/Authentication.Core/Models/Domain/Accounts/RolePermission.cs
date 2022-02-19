using Authentication.Core.Enums;

namespace Authentication.Core.Models.Domain.Accounts
{
    public class RolePermission : ActivatableEntity
    {
        public Role Role { get; set; }
        public Permission Permission { get; set; }
    }
}