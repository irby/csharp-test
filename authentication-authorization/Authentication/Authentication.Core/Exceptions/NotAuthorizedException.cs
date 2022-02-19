using System;
using System.Runtime.Serialization;
using Authentication.Core.Enums;

namespace Authentication.Core.Exceptions
{
    public class NotAuthorizedException : ServiceExceptionBase
    {
        public override int ResponseCode => 403;
        public override string DefaultMessage => "You are not authorized to access the specified resource";

        public NotAuthorizedException() { }

        public NotAuthorizedException(string message) : base(message) { }

        public NotAuthorizedException(SerializationInfo info, StreamingContext context) : base(info, context) { }

        public NotAuthorizedException(string message, Exception innerException) : base(message, innerException) { }

        public NotAuthorizedException(ErrorCode errorCode) : base(errorCode) { }
    }
}