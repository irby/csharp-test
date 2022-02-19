using System;

namespace Authentication.Core.Interfaces
{
    public interface IUpdatableEntity : ICreatableEntity
    {
        public DateTimeOffset? ModifiedOn { get; set; }
        public Guid? ModifiedBy { get; set; }
    }
}