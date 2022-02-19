using System;
using System.Runtime.Serialization;
using Authentication.Core.Enums;

namespace Authentication.Core.Exceptions
{
    public class NotAuthenticatedException : ServiceExceptionBase
    {
        public override int ResponseCode => 401;
        public override string DefaultMessage => "You are not authenticated";

        public NotAuthenticatedException() { }

        public NotAuthenticatedException(string message) : base(message) { }

        public NotAuthenticatedException(SerializationInfo info, StreamingContext context) : base(info, context) { }

        public NotAuthenticatedException(string message, Exception innerException) : base(message, innerException) { }

        public NotAuthenticatedException(ErrorCode errorCode) : base(errorCode) { }
    }
}