using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace Nxt.NET.Response
{
    public interface IGetPeersResponse
    {
        IReadOnlyCollection<string> PeerAddresses { get; }
    }

    public class GetPeersResponse : IGetPeersResponse
    {
        public IReadOnlyCollection<string> PeerAddresses { get; private set; }

        public GetPeersResponse(string json)
        {
            Parse(json);
        }

        private void Parse(string json)
        {
            var peerAddresses = new List<string>();
            var token = JObject.Parse(json);
            var peers = token.SelectToken("peers", false);
            peers.ToList().ForEach(p => peerAddresses.Add(p.ToString()));
            PeerAddresses = new ReadOnlyCollection<string>(peerAddresses);
        }
    }
}
