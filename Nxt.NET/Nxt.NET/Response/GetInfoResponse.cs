using Newtonsoft.Json.Linq;

namespace Nxt.NET.Response
{
    public interface IGetInfoResponse
    {
        string Application { get; }
        string Version { get; }
        string Platform { get; }
        bool ShareAddress { get; }
        string Hallmark { get; }
        string AnnouncedAddress { get; }
    }

    public class GetInfoResponse : IGetInfoResponse
    {
        public string Application { get; private set; }
        public string Version { get; private set; }
        public string Platform { get; private set; }
        public bool ShareAddress { get; private set; }
        public string Hallmark { get; private set; }
        public string AnnouncedAddress { get; private set; }

        public GetInfoResponse(string json)
        {
            Parse(json);
        }

        private void Parse(string json)
        {
            var token = JObject.Parse(json);

            Application = (string) token.SelectToken("application", false);
            Version = (string) token.SelectToken("version", false);
            AnnouncedAddress = (string) token.SelectToken("announcedAddress", false);
            Platform = (string) token.SelectToken("platform", false);
            ShareAddress = (bool) token.SelectToken("shareAddress", false);
            Hallmark = (string) token.SelectToken("hallmark", false);
        }
    }
}