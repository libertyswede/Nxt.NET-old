using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nxt.NET.Response;

namespace Nxt.NETTest.Response
{
    [TestClass]
    public class GetPeersResponseTest
    {
        [TestMethod]
        public void NewGetPeersResponseShouldParseJsonSuccessfully()
        {
            var addresses = new List<string> {"test1", "test2"};
            var json = string.Format("{{peers:[\"{0}\",\"{1}\"]}}", addresses[0], addresses[1]);
            var response = new GetPeersResponse(json);

            Assert.AreEqual(addresses.Count, response.PeerAddresses.Count);
            addresses.ForEach(a => CollectionAssert.Contains(response.PeerAddresses.ToList(), a));
        }
    }
}
