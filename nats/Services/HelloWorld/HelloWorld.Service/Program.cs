using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using System;
using System.Threading.Tasks;

namespace HelloWorld.Service
{
    class Program
    {
        public static async Task Main(string[] args)
        {
            var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            if (environment == null) { environment = EnvironmentName.Production; }

            string configName = $"appsettings.{environment}.json";

            var builder = new HostBuilder()
                .ConfigureAppConfiguration((ctx, confBuilder) =>
                {
                    confBuilder.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
                    confBuilder.AddJsonFile(configName, optional: true);
                    confBuilder.AddEnvironmentVariables();
                    Log.Logger = new LoggerConfiguration()
                        .ReadFrom.Configuration(confBuilder.Build())
                        .Enrich.FromLogContext()
                        .CreateLogger();
                })
                .ConfigureServices((ctx, services) =>
                {
                    services.AddNats(ctx.Configuration.GetSection("ConnectionStrings")["natsConnection"]);
                    services.AddServices(ctx.Configuration);
                    services.AddHostedService<ConfiguredBackgroundService>();

                }).ConfigureLogging((ctx, logBuilder) =>
                {
                    logBuilder.AddSerilog(dispose: true);
                });

            await builder.RunConsoleAsync();
        }

    }

}
