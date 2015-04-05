using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nxt.NET.Request;

namespace Nxt.NETTest.Request
{
    [TestClass]
    public class GetMilestoneBlockIdsRequestFromMilestoneBlockTest : AddQuoteConverterTest
    {
        [TestMethod]
        public void SerializingToJsonShouldQuoteLongValues()
        {
            SetupTest();

            var request = new GetMilestoneBlockIdsRequestFromMilestoneBlock(Id);

            AssertTest("lastMilestoneBlockId", request);
        }
    }
}
