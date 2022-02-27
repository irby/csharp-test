using System.Linq;

namespace SpecFlowCalculator
{
    public class StringBuilder
    {
        public string Value { get; set; }

        public string Reverse()
        {
            return Value == null ? null : new string(Value?.ToCharArray().Reverse().ToArray());
        }
    }
}