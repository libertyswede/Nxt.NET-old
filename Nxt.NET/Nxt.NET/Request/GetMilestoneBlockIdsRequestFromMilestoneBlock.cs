using Newtonsoft.Json;

namespace Nxt.NET.Request
{
    public class GetMilestoneBlockIdsRequestFromMilestoneBlock : GetMilestoneBlockIdsRequestBase
    {
        [JsonConverter(typeof(AddQuoteConverter))]
        [JsonProperty(PropertyName = "lastMilestoneBlockId")]
        public ulong LastMilestoneBlockId { get; set; }

        public GetMilestoneBlockIdsRequestFromMilestoneBlock(long lastMilestoneBlockId)
        {
            LastMilestoneBlockId = (ulong) lastMilestoneBlockId;
        }
    }
}