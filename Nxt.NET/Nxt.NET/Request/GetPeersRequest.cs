using Nxt.NET.Response;

namespace Nxt.NET.Request
{
    public class GetPeersRequest : BaseRequest
    {
        public GetPeersRequest() 
            : base("getPeers")
        {
        }

        public override object ParseResponse(string json)
        {
            return new GetPeersResponse(json);
        }
    }
}
