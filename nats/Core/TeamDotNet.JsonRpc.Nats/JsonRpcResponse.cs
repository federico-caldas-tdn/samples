using Newtonsoft.Json;

namespace TeamDotNet.JsonRpc.Nats
{
    public class JsonRpcResponse
    {
        [JsonProperty("jsonrpc")]
        public string Version { get; }
        [JsonProperty("id")]
        public string Id { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, PropertyName = "result")]
        public string Result { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, PropertyName = "error")]
        public JsonRpcError Error { get; set; }
    }
}
