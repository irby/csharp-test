using System;

namespace Authentication.Core.Interfaces
{
    public interface IIdentifiableEntity
    {
        public Guid Id { get; set; }
    }
}