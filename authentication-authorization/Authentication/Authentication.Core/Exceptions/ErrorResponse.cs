using Authentication.Core.Enums;

namespace Authentication.Core.Exceptions
{
    public class ErrorResponse
    {
        public int StatusCode { get; set; }
        public ErrorCode? ErrorCode { get; set; }
        public string ErrorMessage { get; set; }
    }
}