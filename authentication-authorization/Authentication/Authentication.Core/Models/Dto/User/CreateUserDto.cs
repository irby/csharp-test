using System.Collections.Generic;
using Authentication.Core.Enums;
using Authentication.Core.Models.Domain.Accounts;

namespace Authentication.Core.Models.Dto
{
    public class UserCreateDto
    {
        public string Email { get; set; }
        public string Password { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public Role Role { get; set; }
        public ICollection<UserPermission> UserPermissions { get; set; } = new List<UserPermission>();
    }
}