using System;
using System.Collections.Generic;
using System.Text;

namespace TeamDotNet.JsonRpc.Nats
{
    public enum ErrorCode
    {
        ParseError = -32700,
        InternalError = -32603,
        InvalidParam = -32602,
        MethodNotFound = -32601,
        InvalidRequest = -32600,
        ServerError = -32000
    }
}
