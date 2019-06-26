using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace TeamDotNet.JsonRpc.Nats
{
    /// <summary>
    /// Base class for NATS Subscription service
    /// </summary>
    /// <seealso cref="Microsoft.Extensions.Hosting.BackgroundService" />
    public abstract class JsonRpcSubscriptionBackgroundService : BackgroundService
    {
        private readonly string _natsUrl;
        private readonly List<JsonRpcSubscription> _subscriptions = new List<JsonRpcSubscription>();
        private JsonRpcServiceProxyWrapper _wrapper;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new base instance of the <see cref="NatsSubscriptionBackgroundService" /> class.
        /// </summary>
        /// <param name="natsUrl">The nats URL.</param>
        /// <param name="group">The queue group.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="registerMethods">The registrarion action.</param>
        protected JsonRpcSubscriptionBackgroundService(string natsUrl, string group,
                ILoggerProvider logger, Action<JsonRpcServiceProxyWrapper> registerMethods)
        {
            _logger = logger.CreateLogger(MethodBase.GetCurrentMethod().DeclaringType.FullName);
            _natsUrl = natsUrl;

            _wrapper = new JsonRpcServiceProxyWrapper();
            registerMethods(_wrapper);

            foreach (string channel in _wrapper.Methods)
            {
                _logger?.Log(LogLevel.Information, $"Create subscription {channel}");
                _subscriptions.Add(new JsonRpcSubscription(channel, group, _wrapper, _logger));
            }
        }

        /// <summary>
        /// This method is called when the <see cref="T:Microsoft.Extensions.Hosting.IHostedService" /> starts. The implementation should return a task that represents
        /// the lifetime of the long running operation(s) being performed.
        /// </summary>
        /// <param name="stoppingToken">Triggered when <see cref="M:Microsoft.Extensions.Hosting.IHostedService.StopAsync(System.Threading.CancellationToken)" /> is called.</param>
        /// <returns>
        /// A <see cref="T:System.Threading.Tasks.Task" /> that represents the long running operations.
        /// </returns>
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // create subscriptions to the channels
            await Task.Run(() =>
            {
                _subscriptions.ForEach(s =>
                {
                    s.Subscribe(_natsUrl);
                    s.Start().GetAwaiter().GetResult();
                });
                _logger?.Log(LogLevel.Information, "All subscriptions started.");

            });
        }

        public override Task StopAsync(CancellationToken cancellationToken)
        {
            _subscriptions.ForEach(s=>s.Dispose());
            return Task.CompletedTask;
        }
    }
}
