using Newtonsoft.Json.Linq;

namespace Nxt.NET.Response
{
    public class ProcessBlockResponse
    {
        public bool Accepted { get; private set; }

        public ProcessBlockResponse(string json)
        {
            Parse(json);
        }

        private void Parse(string json)
        {
            var token = JObject.Parse(json);
            Accepted = (bool)token.SelectToken("accepted");
        }
    }
}
