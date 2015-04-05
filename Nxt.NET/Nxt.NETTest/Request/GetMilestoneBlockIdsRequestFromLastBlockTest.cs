using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nxt.NET.Request;

namespace Nxt.NETTest.Request
{
    [TestClass]
    public class GetMilestoneBlockIdsRequestFromLastBlockTest : AddQuoteConverterTest
    {
        [TestMethod]
        public void SerializingToJsonShouldQuoteLongValues()
        {
            SetupTest();

            var request = new GetMilestoneBlockIdsRequestFromLastBlock(Id);

            AssertTest("lastBlockId", request);
        }
    }
}
