using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using TeamDotNet.JsonRpc.Nats;

namespace HelloWorld.Shared
{
    public class HelloWorldServiceProxy : IHelloWorldService
    {
        private readonly IJsonRpcServer server;

        public HelloWorldServiceProxy(IJsonRpcServer server)
        {
            this.server = server;
        }

        public async Task<string> SayHello()
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            //parameters.Add(UserSettingsParams.category, JsonConvert.SerializeObject(category));
            //parameters.Add(UserSettingsParams.item, JsonConvert.SerializeObject(item));
            JsonRpcRequest request = new JsonRpcRequest
            {
                //Base64BearerToken = user.ToBase64String(),
                Method = HelloWorldMethods.SayHello,
                Id = Guid.NewGuid().ToString(),
                Parameters = parameters
            };
            return await this.Execute<string>(request);
        }

        private async Task<T> Execute<T>(JsonRpcRequest request)
        {
            var resp = await this.server.ServeAsync(request);
            if (resp.Error != null)
            {
                throw new Exception(resp.Error.Message, new Exception(resp.Error.Data));
            }

            return JsonConvert.DeserializeObject<T>(resp.Result);
        }

        private async Task Execute(JsonRpcRequest request)
        {
            var resp = await this.server.ServeAsync(request);
            if (resp.Error != null)
            {
                throw new Exception(resp.Error.Message, new Exception(resp.Error.Data));
            }
        }

    }
}
