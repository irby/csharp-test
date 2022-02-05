namespace DependencyInjection
{
    public sealed class SharedService
    {
        private const int SecretNumber = 10;
        
        public int GetNumber()
        {
            return SecretNumber;
        }
    }
}