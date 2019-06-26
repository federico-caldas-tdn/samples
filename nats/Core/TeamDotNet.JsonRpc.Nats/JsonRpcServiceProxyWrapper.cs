using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Serilog;

namespace TeamDotNet.JsonRpc.Nats
{
    /// <summary>
    /// Proxy wrapper that create "Pseudo" json-rpc server
    /// </summary>
    public class JsonRpcServiceProxyWrapper : IJsonRpcServer
    {
        private readonly static ConcurrentDictionary<string, Func<JsonRpcRequest, Task<JsonRpcResponse>>>
            MethodMap = new ConcurrentDictionary<string, Func<JsonRpcRequest,  Task<JsonRpcResponse>>>();
        /// <summary>
        /// Initializes a new instance of the <see cref="JsonRpcServiceProxy"/> class.
        /// </summary>
        /// <param name="appService">The application service.</param>
        public JsonRpcServiceProxyWrapper()
        {
        }

        public void RegisterMethod(string methodName, Func<JsonRpcRequest, Task<JsonRpcResponse>> method)
        {
            if (!MethodMap.TryGetValue(methodName, out Func<JsonRpcRequest, Task<JsonRpcResponse>> m))
                MethodMap.TryAdd(methodName, method);
            else
                MethodMap.TryUpdate(methodName, method, m);
        }

        /// <summary>
        /// Unregisters the method.
        /// </summary>
        /// <param name="methodName">Name of the method.</param>
        /// <returns></returns>
        public bool UnregisterMethod(string methodName)
        {
            return MethodMap.TryRemove(methodName, out Func<JsonRpcRequest, Task<JsonRpcResponse>> method);
        }

        public JsonRpcResponse Serve(JsonRpcRequest request)
        {
            return ServeAsync(request).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Serves the asynchronous.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public async Task<JsonRpcResponse> ServeAsync(JsonRpcRequest request)
        {
            Log.Information($"Request for method {request?.Method} received.");
            if (request == null)
            {
                return await Task.FromResult(new JsonRpcResponse
                {
                    Error = new JsonRpcError
                    {
                        Code = (int)ErrorCode.InvalidRequest
                    }
                });
            }

            if (string.IsNullOrEmpty(request.Method))
            {
                return await Task.FromResult(new JsonRpcResponse
                {
                    Id = request.Id,
                    Error = new JsonRpcError
                    {
                        Code = (int)ErrorCode.MethodNotFound,
                        Message = "Method name is not provided"
                    }
                });
            }

            if (!MethodMap.TryGetValue(request.Method, out Func<JsonRpcRequest, Task<JsonRpcResponse>> f))
            {
                Log.Information($"Method {request?.Method} not found.");
                return await Task.FromResult(new JsonRpcResponse
                {
                    Id = request.Id,
                    Error = new JsonRpcError
                    {
                        Code = (int)ErrorCode.MethodNotFound,
                        Message = $"Method {request.Method} not found."
                    }
                });
            }

            try
            {
                return await f(request);
            }
            catch (Exception ex)
            {
                return await Task.FromResult(new JsonRpcResponse
                {
                    Id = request.Id,
                    Error = new JsonRpcError
                    {
                        Code = (int)ErrorCode.ServerError,
                        Message = ex.Message,
                        Data = ex.StackTrace
                    }
                });

            }
        }

        /// <summary>
        /// Determines whether the specified m1 has method.
        /// </summary>
        /// <param name="methodName">Name of the method.</param>
        /// <returns></returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public bool HasMethod(string methodName)
        {
            return MethodMap.TryGetValue(methodName, out Func<JsonRpcRequest, Task<JsonRpcResponse>> m);
        }
        /// <summary>
        /// Gets all registered methods.
        /// </summary>
        /// <value>
        /// The methods.
        /// </value>
        public IReadOnlyList<string> Methods => MethodMap.Keys.ToList().AsReadOnly();
    }
}