using HelloWorld.ApplicationService;
using HelloWorld.Shared;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;
using TeamDotNet.JsonRpc.Nats;

namespace HelloWorld.Service
{
    internal static class SetupServicesExt
    {
        /// <summary>
        /// Adds the nats.
        /// </summary>
        /// <param name="services">The services.</param>
        /// <param name="natsConnection">The nats connection.</param>
        public static void AddNats(this IServiceCollection services, string natsConnection)
        {
            services.AddSingleton<IJsonRpcServer>(p => new JsonRpcServer(natsConnection));
        }
        /// <summary>
        /// Adds the services.
        /// </summary>
        /// <param name="services">The services.</param>
        /// <param name="configuration">The configuration.</param>
        public static void AddServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddSingleton<IHelloWorldService>(p =>
                new HelloWorldApplicationService());
        }
    }

}
