using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nxt.NET.Response;

namespace Nxt.NETTest.Response
{
    [TestClass]
    public class GetNextBlockIdsResponseTest
    {
        private const string NoBlockIds = "{\"nextBlockIds\":[]}";
        private const string OneBlockId = "{\"nextBlockIds\":[\"6556228577102711328\"]}";

        [TestMethod]
        public void ShouldHaveZeroNextBlockIds()
        {
            var response = new GetNextBlockIdsResponse(NoBlockIds);

            Assert.AreEqual(0, response.NextBlockIds.Count);
        }

        [TestMethod]
        public void ShouldHaveOneNextBlockId()
        {
            var response = new GetNextBlockIdsResponse(OneBlockId);

            Assert.AreEqual(1, response.NextBlockIds.Count);
        }
    }
}
