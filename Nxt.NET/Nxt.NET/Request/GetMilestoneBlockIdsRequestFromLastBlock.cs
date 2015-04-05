using Newtonsoft.Json;

namespace Nxt.NET.Request
{
    public class GetMilestoneBlockIdsRequestFromLastBlock : GetMilestoneBlockIdsRequestBase
    {
        [JsonConverter(typeof(AddQuoteConverter))]
        [JsonProperty(PropertyName = "lastBlockId")]
        public ulong LastBlockId { get; set; }

        public GetMilestoneBlockIdsRequestFromLastBlock(long lastBlockId)
        {
            LastBlockId = (ulong) lastBlockId;
        }
    }
}