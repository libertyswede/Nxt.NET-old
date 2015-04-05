using Newtonsoft.Json;
using Nxt.NET.Response;

namespace Nxt.NET.Request
{
    public class GetInfoRequest : BaseRequest
    {
        [JsonProperty(PropertyName = "application")]
        public string Application { get { return Constants.ApplicationName; } }

        [JsonProperty(PropertyName = "version")]
        public string Version { get { return Constants.ApplicationVersion; } }

        [JsonProperty(PropertyName = "platform")]
        public string Platform { get { return Constants.ApplicationPlatform; } }

        [JsonProperty(PropertyName = "shareAddress")]
        public bool ShareAddress { get { return true; } }

        public GetInfoRequest()
            : base("getInfo")
        {
        }

        public override object ParseResponse(string json)
        {
            return new GetInfoResponse(json);
        }
    }
}
