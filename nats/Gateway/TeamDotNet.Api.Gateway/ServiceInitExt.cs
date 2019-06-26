using HelloWorld.Shared;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TeamDotNet.JsonRpc.Nats;

namespace TeamDotNet.Api.Gateway
{
    public static class ServiceInitExt
    {
        /// <summary>
        /// Adds the nats.
        /// </summary>
        /// <param name="services">The services.</param>
        /// <param name="connectionString">The connection string.</param>
        /// <returns></returns>
        public static IServiceCollection AddNats(this IServiceCollection services, string connectionString)
        {
            services.AddSingleton<IJsonRpcServer>(c => new JsonRpcServer(connectionString));
            return services;
        }

        /// <summary>
        /// Adds the Hello world service.
        /// </summary>
        /// <param name="services">The services.</param>
        /// <returns></returns>
        public static IServiceCollection AddHelloWorldService(this IServiceCollection services)
        {
            services.AddSingleton<IHelloWorldService>(p => new HelloWorldServiceProxy(p.GetService<IJsonRpcServer>()));
            return services;
        }
    }
}
