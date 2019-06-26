using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace TeamDotNet.JsonRpc.Nats
{
    [JsonObject(MemberSerialization.OptIn)]
    public class JsonRpcRequest
    {
        [JsonProperty("jsonrpc")]
        public string Version { get; }
        [JsonProperty("method")]
        public string Method { get; set; }
        [JsonProperty("id")]
        public string Id { get; set; }
        [JsonProperty("token")]
        public string Base64BearerToken { get; set; }
        [JsonProperty("params")]
        public Dictionary<string, string> Parameters { get; set; }
    }
}
