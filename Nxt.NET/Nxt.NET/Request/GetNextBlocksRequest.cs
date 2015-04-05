using Newtonsoft.Json;
using Nxt.NET.Block;
using Nxt.NET.Response;
using StructureMap;

namespace Nxt.NET.Request
{
    public class GetNextBlocksRequest : BaseRequest
    {
        [JsonProperty(PropertyName = "blockId")]
        [JsonConverter(typeof(AddQuoteConverter))]
        public ulong BlockId { get; set; }

        public GetNextBlocksRequest(long blockId) 
            : base("getNextBlocks")
        {
            BlockId = (ulong) blockId;
        }

        public override object ParseResponse(string json)
        {
            var parser = ObjectFactory.GetInstance<IBlockParser>();
            return new GetNextBlocksResponse(parser, json);
        }
    }
}
