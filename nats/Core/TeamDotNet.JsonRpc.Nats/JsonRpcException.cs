using System;

namespace TeamDotNet.JsonRpc.Nats
{
    public class JsonRpcException : Exception
    {
        public JsonRpcException(ErrorCode code)
        {
            Code = code;
        }
        public JsonRpcException(ErrorCode code, string message):base(message)
        {
            Code = code;
        }

        public JsonRpcException(ErrorCode code, string message, Exception innerException) : base(message,innerException)
        {
            Code = code;
        }

        public ErrorCode Code { get; }
    }
}
