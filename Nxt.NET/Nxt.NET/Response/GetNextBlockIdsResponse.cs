using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace Nxt.NET.Response
{
    public interface IGetNextBlockIdsResponse
    {
        IReadOnlyCollection<long> NextBlockIds { get; set; }
    }

    public class GetNextBlockIdsResponse : IGetNextBlockIdsResponse
    {
        public IReadOnlyCollection<long> NextBlockIds { get; set; }

        public GetNextBlockIdsResponse(string json)
        {
            Parse(json);
        }

        private void Parse(string json)
        {
            var blockIds = new List<ulong>();

            var token = JObject.Parse(json).SelectToken("nextBlockIds");
            var currentId = token.FirstOrDefault();

            while (currentId != null)
            {
                blockIds.Add((ulong) currentId);
                currentId = currentId.Next;
            }

            NextBlockIds = new ReadOnlyCollection<long>(blockIds.Select(id => (long) id).ToList());
        }
    }
}