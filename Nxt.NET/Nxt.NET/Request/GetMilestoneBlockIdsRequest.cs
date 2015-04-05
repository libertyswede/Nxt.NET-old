using Nxt.NET.Response;

namespace Nxt.NET.Request
{
    public abstract class GetMilestoneBlockIdsRequestBase : BaseRequest
    {
        protected GetMilestoneBlockIdsRequestBase()
            : base("getMilestoneBlockIds")
        {
        }

        public override object ParseResponse(string json)
        {
            return new GetMilestoneBlockIdsResponse(json);
        }
    }
}