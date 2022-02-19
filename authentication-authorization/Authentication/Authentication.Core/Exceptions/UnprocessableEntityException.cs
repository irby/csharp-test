using System;
using System.Runtime.Serialization;
using Authentication.Core.Enums;

namespace Authentication.Core.Exceptions
{
    public class UnprocessableEntityException : ServiceExceptionBase
    {
        public override int ResponseCode => 422;
        public override string DefaultMessage => "The server was not able to process this request as submitted";

        public UnprocessableEntityException() { }

        public UnprocessableEntityException(string message) : base(message) { }

        public UnprocessableEntityException(SerializationInfo info, StreamingContext context) : base(info, context) { }

        public UnprocessableEntityException(string message, Exception innerException) : base(message, innerException) { }

        public UnprocessableEntityException(ErrorCode errorCode) : base(errorCode) { }
    }
}