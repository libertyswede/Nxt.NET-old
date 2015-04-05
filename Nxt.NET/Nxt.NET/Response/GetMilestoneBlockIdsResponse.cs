using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace Nxt.NET.Response
{
    public interface IGetMilestoneBlockIdsResponse
    {
        IReadOnlyCollection<long> MilestonBlockIds { get; set; }
        bool IsLast { get; set; }
    }

    public class GetMilestoneBlockIdsResponse : IGetMilestoneBlockIdsResponse
    {
        public IReadOnlyCollection<long> MilestonBlockIds { get; set; }
        public bool IsLast { get; set; }
        public GetMilestoneBlockIdsResponse(string json)
        {
            Parse(json);
        }

        private void Parse(string json)
        {
            var milesStoneBlockIds = new List<ulong>();

            var token = JObject.Parse(json);
            var milestoneToken = token.SelectToken("milestoneBlockIds");
            if (token.SelectToken("last", false) != null)
                IsLast = Boolean.Parse((string)token.SelectToken("last"));
            var currentId = milestoneToken.FirstOrDefault();

            while (currentId != null)
            {
                milesStoneBlockIds.Add((ulong) currentId);
                currentId = currentId.Next;
            }


            MilestonBlockIds = new ReadOnlyCollection<long>(milesStoneBlockIds.Select(ids => (long)ids).ToList());
        }
    }
}