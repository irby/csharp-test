using System;

namespace Authentication.Core.Interfaces
{
    public interface ICreatableEntity
    {
        public DateTimeOffset? CreatedOn { get; set; }
        public Guid? CreatedBy { get; set; }
    }
}