using Newtonsoft.Json;

namespace Nxt.NET.Request
{
    public interface IRequest
    {
        object ParseResponse(string json);
    }

    public abstract class BaseRequest : IRequest
    {
        [JsonProperty(PropertyName = "protocol")]
        public int Protocol { get { return 1; } }

        [JsonProperty(PropertyName = "requestType")]
        public string RequestType { get; private set; }

        protected BaseRequest(string requestType)
        {
            RequestType = requestType;
        }

        public abstract object ParseResponse(string json);
    }
}