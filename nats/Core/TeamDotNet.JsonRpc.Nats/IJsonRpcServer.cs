using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace TeamDotNet.JsonRpc.Nats
{
    public interface IJsonRpcServer
    {
        JsonRpcResponse Serve(JsonRpcRequest request);
        Task<JsonRpcResponse> ServeAsync(JsonRpcRequest request);
    }
}
