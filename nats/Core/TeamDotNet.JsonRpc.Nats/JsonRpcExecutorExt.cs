using System;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace TeamDotNet.JsonRpc.Nats
{
    /// <summary>
    ///  Execution extension
    /// </summary>
    public static class JsonRpcExecutorExt
    {
        /// <summary>
        /// Executes the specified req.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="server">The server.</param>
        /// <param name="req">The req.</param>
        /// <returns></returns>
        /// <exception cref="Exception">
        /// </exception>
        public static async Task<T> Execute<T>(this IJsonRpcServer server, JsonRpcRequest req)
        {
            var resp = await server.ServeAsync(req);
            if (resp.HasError())
            {
                throw new Exception(resp.Error.Message, new Exception(resp.Error.Data));
            }
            return JsonConvert.DeserializeObject<T>(resp.Result);
        }
    }
}