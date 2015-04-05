using System.Numerics;
using Newtonsoft.Json.Linq;

namespace Nxt.NET.Response
{
    public interface IGetCumulativeDifficultyResponse
    {
        int? BlockchainHeight { get; set; }
        BigInteger? CumulativeDifficulty { get; set; }
    }

    public class GetCumulativeDifficultyResponse : IGetCumulativeDifficultyResponse
    {
        public int? BlockchainHeight { get; set; }
        public BigInteger? CumulativeDifficulty { get; set; }

        public GetCumulativeDifficultyResponse(string json)
        {
            Parse(json);
        }

        private void Parse(string json)
        {
            var token = JObject.Parse(json);

            CumulativeDifficulty = BigInteger.Parse((string) token.SelectToken("cumulativeDifficulty"));
            BlockchainHeight = (int?) token.SelectToken("blockchainHeight", false);
        }
    }
}