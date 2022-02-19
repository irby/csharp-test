using System;
using Authentication.Core.Enums;
using Authentication.Core.Interfaces;
using Authentication.Core.Models.Domain.Accounts;

namespace Authentication.Core.Models.Domain.Auditing
{
    public class UserLoginAuditRecord : IIdentifiableEntity, ICreatableEntity
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public virtual User User { get; set; }
        public bool IsSuccess { get; set; }
        public ErrorCode? ErrorCode { get; set; }
        public DateTimeOffset? CreatedOn { get; set; }
        public Guid? CreatedBy { get; set; }
    }
}