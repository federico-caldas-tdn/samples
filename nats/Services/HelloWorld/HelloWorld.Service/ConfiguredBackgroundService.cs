using HelloWorld.Shared;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Serilog;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace HelloWorld.Service
{
    public class ConfiguredBackgroundService : HelloWorldBackgroundService
    {
        public ConfiguredBackgroundService(IConfiguration config,
            IHelloWorldService service, ILoggerProvider logger) :
            base(config.GetSection("ConnectionStrings")["natsConnection"], service, logger)
        {
        }

        /// <summary>
        /// Executes the asynchronous.
        /// </summary>
        /// <param name="stoppingToken">The stopping token.</param>
        /// <returns></returns>
        protected async override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Log.Logger.Information("Starting the service...");
            await base.ExecuteAsync(stoppingToken);
            Log.Logger.Information("The service started.");
        }

        /// <summary>
        /// Triggered when the application host is performing a graceful shutdown.
        /// </summary>
        /// <param name="cancellationToken">Indicates that the shutdown process should no longer be graceful.</param>
        /// <returns></returns>
        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            Log.Logger.Information("Stopping the service...");
            await base.StopAsync(cancellationToken);
            Log.Logger.Information("The service stopped.");
        }
    }

}
