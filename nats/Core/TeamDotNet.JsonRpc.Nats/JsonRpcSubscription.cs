using System;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MyNatsClient;
using MyNatsClient.Extensions;
using Newtonsoft.Json;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace TeamDotNet.JsonRpc.Nats
{
    /// <summary>
    /// The subscription that listens on Nats topic and sends response to the message originator
    /// </summary>
    public class JsonRpcSubscription : IDisposable
    {
        private readonly IJsonRpcServer _server;
        private bool _disposed;
        private ConnectionInfo _cnInfo;
        private NatsClient _client;
        private ISubscription _subscription;
        private readonly SubscriptionInfo _sInfo;
        private readonly ILogger _logger;
        /// <summary>
        /// Initializes a new instance of the <see cref="JsonRpcSubscription" /> class.
        /// </summary>
        /// <param name="channel">The channel.</param>
        /// <param name="group">The subscription group</param>
        /// <param name="server">The server.</param>
        /// <param name="withReplyMessage">If set to false, no reply message will be sent back to the subscription thorugh NATS. Use false for subscriptions that do not have to send a response, just process whatever is in the queue. Default is true. </param>
        public JsonRpcSubscription(string channel, string group, IJsonRpcServer server, ILogger logger = null)
        {
            _server = server;
            _server = server;
            _sInfo = new SubscriptionInfo(channel, group);
            _logger = logger;
        }

        /// <summary>
        /// Subscribes to the specified url.
        /// </summary>
        /// <param name="url">The connection url.</param>
        public void Subscribe(string url)
        {
            var uri = new Uri(url);
            _cnInfo = new ConnectionInfo(uri.Host,uri.Port==-1?null:(int?)uri.Port);
            _client = new NatsClient(_cnInfo);
        }
        /// <summary>
        /// Starts the subscription
        /// </summary>
        /// <summary>
        /// Starts the subscription
        /// </summary>
        public async Task Start()
        {
            if(_disposed)
                throw new ObjectDisposedException($"{typeof(JsonRpcSubscription).FullName}@{this.GetHashCode()}");
            if(!_client.IsConnected)
                _client.Connect();
            if (_subscription != null)
            {
                await _client.UnsubAsync(_subscription);
                _subscription = null;
            }
            _subscription=await _client.SubAsync(_sInfo, stream => stream.Subscribe(msg =>
            {             
                try
                {
                    byte[] resp = OnMessage(msg.Payload);
                    _client.Pub(msg.ReplyTo,resp);
                }
                catch (Exception ex)
                {
                    _logger?.LogError(ex, "Error publishing NATS message");
                }
            }, error => {                
                _logger?.LogError(error, "Fatal error in NATS subscription handler");
            }));
        }

        private byte[] OnMessage(byte[] data)
        {
            string jsonStr = Encoding.UTF8.GetString(data);
            var req = JsonConvert.DeserializeObject<JsonRpcRequest>(jsonStr);
            JsonRpcResponse res = null;
            try
            {
                res = _server.Serve(req);
            }
            catch (Exception ex)
            {
                res = new JsonRpcResponse
                {
                    Id = req.Id,
                    Error = new JsonRpcError
                    {
                        Code = (int)ErrorCode.InternalError,
                        Message = ex.Message
                    }
                };
            }
            string jsonResp = JsonConvert.SerializeObject(res);
            byte[] resData= Encoding.UTF8.GetBytes(jsonResp);
            return resData;
        }
        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Finalizes an instance of the <see cref="NatsJsonRpcSubscription"/> class.
        /// </summary>
        ~JsonRpcSubscription()
        {
            Dispose(false);
        }
        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing && !_disposed)
            {
                if (_client != null && _client.IsConnected)
                {
                    if (_subscription != null)
                    {
                        _client.Unsub(_subscription);
                        _subscription = null;
                    }
                    _client.Disconnect();
                   
                }
                _disposed = true;
            }
        }
    }
}
