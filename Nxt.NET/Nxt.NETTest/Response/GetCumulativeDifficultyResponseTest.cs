using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nxt.NET.Response;

namespace Nxt.NETTest.Response
{
    [TestClass]
    public class GetCumulativeDifficultyResponseTest
    {
        private const string ValidJsonResponse = "{\"blockchainHeight\":1234,\"cumulativeDifficulty\":\"1234\"}";
        private const string InvalidJsonMissingCumulativeDifficulty = "{\"blockchainHeight\":1234}";
        private const string ValidJsonMissingBlockchainHeight = "{\"cumulativeDifficulty\":\"1234\"}";

        [TestMethod]
        public void ShouldParseValidJson()
        {
            var response = new GetCumulativeDifficultyResponse(ValidJsonResponse);

            Assert.AreEqual(1234, response.BlockchainHeight);
            Assert.AreEqual(1234, (int)response.CumulativeDifficulty);
        }

        [TestMethod]
        public void ShouldParseSuccessfullyWithoutBlockchainHeight()
        {
            var response = new GetCumulativeDifficultyResponse(ValidJsonMissingBlockchainHeight);

            Assert.IsNull(response.BlockchainHeight);
            Assert.AreEqual(1234, (int)response.CumulativeDifficulty);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ShouldRequireCumulativeDifficultyInResponse()
        {
            // ReSharper disable once ObjectCreationAsStatement
            new GetCumulativeDifficultyResponse(InvalidJsonMissingCumulativeDifficulty);
        }
    }
}
