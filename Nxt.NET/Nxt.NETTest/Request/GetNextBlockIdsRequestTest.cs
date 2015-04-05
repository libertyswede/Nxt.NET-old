using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nxt.NET.Request;

namespace Nxt.NETTest.Request
{
    [TestClass]
    public class GetNextBlockIdsRequestTest : AddQuoteConverterTest
    {
        [TestMethod]
        public void SerializingToJsonShouldQuoteLongValues()
        {
            SetupTest();

            var request = new GetNextBlockIdsRequest(Id);

            AssertTest("blockId", request);
        }
    }
}
