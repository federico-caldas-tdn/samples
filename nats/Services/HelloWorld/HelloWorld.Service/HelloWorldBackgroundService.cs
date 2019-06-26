using System;
using System.Collections.Generic;
using System.Text;
using HelloWorld.Shared;
using Microsoft.Extensions.Logging;
using TeamDotNet.JsonRpc.Nats;

namespace HelloWorld.Service
{
    public class HelloWorldBackgroundService : JsonRpcSubscriptionBackgroundService
    {
        public HelloWorldBackgroundService(string natsUrl,
            IHelloWorldService service, ILoggerProvider logger) : base(natsUrl,
            "helloworldservice", logger, wrapper =>
            {
                var proxy = new HelloWorldProxy(service);
                wrapper.RegisterMethod(HelloWorldMethods.SayHello, p => proxy.SayHello(p));
            })
        {

        }
    }
}
