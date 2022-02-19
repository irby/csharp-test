using Authentication.Core.Interfaces;

namespace Authentication.Core.Models.Domain
{
    public class ActivatableEntity : DomainModelBase, IActivatableEntity
    {
        public bool IsEnabled { get; set; }
    }
}