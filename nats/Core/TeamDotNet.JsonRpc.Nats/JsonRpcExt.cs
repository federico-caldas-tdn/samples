using System;

namespace TeamDotNet.JsonRpc.Nats
{
    /// <summary>
    /// Extensions for Json-Rpc request
    /// </summary>
    public static class JsonRpcExt
    {
        /// <summary>
        /// Determines whether this instance has error.
        /// </summary>
        /// <param name="resp">The resp.</param>
        /// <returns>
        ///   <c>true</c> if the specified resp has error; otherwise, <c>false</c>.
        /// </returns>
        public static bool HasError(this JsonRpcResponse resp)
        {
            return resp?.Error != null;
        }
        /// <summary>
        /// Gets the specified req.
        /// </summary>
        /// <param name="req">The req.</param>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        public static (bool hasValue, string val, JsonRpcResponse response) Get(this JsonRpcRequest req, string name)
        {
            bool hasValue = req.Parameters.TryGetValue(name, out string val);
            return (hasValue: hasValue, val:hasValue?val:string.Empty, response: new JsonRpcResponse
            {
                Id = req.Id,
                Result = string.Empty,
                Error = hasValue?null: new JsonRpcError
                {
                    Code = (int)ErrorCode.InvalidParam,
                    Message = $"Parameter {name} was not provided.",
                    Data=name
                }
            });
        }

        /// <summary>
        /// Gets the specified req.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="req">The req.</param>
        /// <param name="name">The name.</param>
        /// <param name="value">The value.</param>
        /// <param name="convertFunc">The convert function.</param>
        /// <returns></returns>
        
        public static JsonRpcResponse Get<T>(this JsonRpcRequest req, string name, out T value, Func<string, T> convertFunc)
        {
            JsonRpcError error = null;
            value = default(T);
            if (req.Parameters.TryGetValue(name, out string val))
            {
                try
                {
                    value = convertFunc(val);
                }
                catch (Exception e)
                {
                    error = new JsonRpcError
                    {
                        Code = (int) ErrorCode.ParseError,
                        Message = $"{e.Message}",
                        Data = e.StackTrace
                    };
                }
            }
            else
            {
                error = new JsonRpcError
                {
                    Code = (int) ErrorCode.InvalidParam,
                    Message = $"Parameter {name} was not provided.",
                    Data = name
                };
            }
            return new JsonRpcResponse
            {
                Id = req.Id,
                Result = string.Empty,
                Error = error
            };
        }

    }
}