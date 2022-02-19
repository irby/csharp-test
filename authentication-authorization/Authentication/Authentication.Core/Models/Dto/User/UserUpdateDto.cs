using System;
using System.Collections.Generic;
using Authentication.Core.Enums;
using Authentication.Core.Models.Domain.Accounts;

namespace Authentication.Core.Models.Dto
{
    public class UserUpdateDto
    {
        public Guid Id { get; set; }
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public Role UserRole { get; set; }
        public ICollection<UserPermission> UserPermissions { get; set; } = new List<UserPermission>();
        public bool IsEnabled { get; set; }
    }
}