using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace TeamDotNet.JsonRpc.Nats.Test
{
    public class TestProxy
    {
        public Task<JsonRpcResponse> TestMethod(object obj)
        {
            Console.Write(obj.ToString());
            return Task.FromResult<JsonRpcResponse>(new JsonRpcResponse());
        }
    }
}
