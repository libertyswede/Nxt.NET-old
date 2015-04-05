using Newtonsoft.Json;
using Nxt.NET.Response;

namespace Nxt.NET.Request
{
    public class GetNextBlockIdsRequest : BaseRequest
    {
        [JsonProperty(PropertyName = "blockId")]
        [JsonConverter(typeof(AddQuoteConverter))]
        public ulong BlockId { get; set; }

        public GetNextBlockIdsRequest(long blockId)
            : base("getNextBlockIds")
        {
            BlockId = (ulong) blockId;
        }

        public override object ParseResponse(string json)
        {
            return new GetNextBlockIdsResponse(json);
        }
    }
}
