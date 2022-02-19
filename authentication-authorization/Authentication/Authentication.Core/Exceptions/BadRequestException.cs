using System;
using System.Runtime.Serialization;
using Authentication.Core.Enums;

namespace Authentication.Core.Exceptions
{
    public class BadRequestException : ServiceExceptionBase
    {
        public override int ResponseCode => 400;
        public override string DefaultMessage => "The request was not valid";

        public BadRequestException() { }

        public BadRequestException(string message) : base(message) { }

        public BadRequestException(SerializationInfo info, StreamingContext context) : base(info, context) { }

        public BadRequestException(string message, Exception innerException) : base(message, innerException) { }

        public BadRequestException(ErrorCode errorCode) : base(errorCode) { }
    }
}