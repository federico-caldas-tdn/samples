using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace TeamDotNet.JsonRpc.Nats
{
    public class JsonRpcError
    {
        [JsonProperty("code")]
        public int Code { get; set; }
        [JsonProperty("message")]
        public string Message { get; set; }
        [JsonProperty("data")]
        public string Data { get; set; }
    }
}
