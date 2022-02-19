namespace Authentication.Core.Interfaces
{
    public interface IActivatableEntity
    {
        public bool IsEnabled { get; set; }
    }
}