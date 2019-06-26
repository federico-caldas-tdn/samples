using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using TeamDotNet.JsonRpc.Nats;

namespace HelloWorld.Shared
{
    public class HelloWorldProxy 
    {
        private readonly IHelloWorldService _service;

        public HelloWorldProxy(IHelloWorldService service)
        {
            _service = service;
        }

        public async Task<JsonRpcResponse> SayHello(JsonRpcRequest request)
        {
            var result = await _service.SayHello();
            return new JsonRpcResponse
            {
                Id = request.Id,
                Result = JsonConvert.SerializeObject(result)
            };
        }
    }
}
