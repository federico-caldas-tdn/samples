using System;
using System.Text;
using System.Threading.Tasks;
using MyNatsClient;
using Newtonsoft.Json;

namespace TeamDotNet.JsonRpc.Nats
{
    /// <summary>
    /// Implementation of Json-Rpc service over Nats transport
    /// </summary>
    public class JsonRpcServer:IJsonRpcServer,IDisposable
    {
        private readonly int _timeout;
        private bool _disposed;
        private readonly ConnectionInfo _connectionInfo;
        /// <summary>
        /// Initializes a new instance of the <see cref="JsonRpcServer"/> class.
        /// </summary>
        /// <param name="url">The URL.</param>
        /// <param name="timeout">The timeout.</param>
        public JsonRpcServer(string url, int timeout = 10000)
        {
            Uri uri=new Uri(url);
            _connectionInfo = new ConnectionInfo(uri.Host,uri.Port==-1?null:(int?)uri.Port);
            _timeout = timeout;
        }

        /// <summary>
        /// Serves the specified request.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public JsonRpcResponse Serve(JsonRpcRequest request)
        {
            return ServeAsync(request).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Serves the asynchronous.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns></returns>
        public async Task<JsonRpcResponse> ServeAsync(JsonRpcRequest request)
        {
            if(_disposed)
                throw new ObjectDisposedException(typeof(JsonRpcServer).Name);
            using (var client=new NatsClient(_connectionInfo))
            {
                try
                {
                    client.Connect();
                    var response = await client.RequestAsync(request.Method, EncodeRequest(request),_timeout);
                    var data = DecodeResponse(response.Payload);
                    return data;
                }
                catch (NatsRequestTimedOutException e)
                {
                    throw new JsonRpcException(ErrorCode.InternalError, e.Message);
                }
                catch (Exception ex)
                {
                    throw new JsonRpcException(ErrorCode.InternalError, ex.Message);
                }
            }
        }

        /// <summary>
        /// Decodes the response.
        /// </summary>
        /// <param name="responseData">The response data.</param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        private JsonRpcResponse DecodeResponse(byte[] responseData)
        {
            string jsonStr = Encoding.UTF8.GetString(responseData);
            return JsonConvert.DeserializeObject<JsonRpcResponse>(jsonStr);
        }

        /// <summary>
        /// Encodes the request as Base64 string.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        private byte[] EncodeRequest(JsonRpcRequest request)
        {
            return Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(request));
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
        /// Finalizes an instance of the <see cref="NatsJsonRpcServer"/> class.
        /// </summary>
        ~JsonRpcServer()
        {
            Dispose(false);
        }
        protected virtual void Dispose(bool disposing)
        {
            if (disposing && !_disposed) 
            {
                _disposed = true;
            }

        }

    }
}
