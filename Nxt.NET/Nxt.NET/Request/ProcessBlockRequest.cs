using System;
using Nxt.NET.Response;

namespace Nxt.NET.Request
{
    internal class ProcessBlockRequest : BaseRequest
    {
        private readonly Block.Block _block;

        public ProcessBlockRequest(Block.Block block) 
            : base("processBlock")
        {
            _block = block;
            throw new NotImplementedException();
        }

        public override object ParseResponse(string json)
        {
            return new ProcessBlockResponse(json);
        }
    }
}
