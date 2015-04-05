using Nxt.NET.Response;

namespace Nxt.NET.Request
{
    public class GetCumulativeDifficultyRequest : BaseRequest
    {
        public GetCumulativeDifficultyRequest() 
            : base("getCumulativeDifficulty")
        {
        }

        public override object ParseResponse(string json)
        {
            return new GetCumulativeDifficultyResponse(json);
        }
    }
}
