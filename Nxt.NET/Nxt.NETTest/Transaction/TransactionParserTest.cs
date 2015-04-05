using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json.Linq;
using Nxt.NET.Transaction;

namespace Nxt.NETTest.Transaction
{
    [TestClass]
    public class TransactionParserTest
    {
        private TransactionParser _transactionParser;
        private Mock<ITransactionFactory> _transactionFactoryMock;

        private const string NoTransactionsJson = "{\"transactions\":[]}";
        private const string OneTransactionJson =
            "{\"transactions\": [{" +
                "\"timestamp\": 857," +
                "\"amountNQT\": 100000000000000," +
                "\"subtype\": 0," +
                "\"feeNQT\": 100000000," +
                "\"senderPublicKey\": \"c729284d595e6cbcac232742b440bf7fe2f910adad46edf7b1540c3741977440\"," +
                "\"type\": 0," +
                "\"deadline\": 1440," +
                "\"signature\": \"5138816a0a25768c23289e704a39d9fede100b9210d3dae370a181d8962ee408a4a5467fe8e3079fb7dd6ade8f4a90e5c5f2ad1d161e03db7062b296cd63c0a6\"," +
                "\"recipient\": \"78880408059\"" +
            "}]}";

        [TestInitialize]
        public void TestInit()
        {
            _transactionFactoryMock = new Mock<ITransactionFactory>();

            _transactionParser = new TransactionParser(_transactionFactoryMock.Object);
        }

        [TestMethod]
        public void ParseTransactionsShouldReturnZeroTransactions()
        {
            var jobject = JObject.Parse(NoTransactionsJson);

            var transactions = _transactionParser.ParseTransactions(jobject.SelectToken("transactions"));

            Assert.AreEqual(0, transactions.Count);
        }

        [TestMethod]
        public void ParseTransactionShouldReturnOneTransaction()
        {
            var jobject = JObject.Parse(OneTransactionJson);

            _transactionParser.ParseTransactions(jobject.SelectToken("transactions"));

            _transactionFactoryMock.Verify(tf =>
                tf.Create(
                    It.Is<byte>(type => type == 0),
                    It.Is<byte>(subType => subType == 0),
                    It.Is<int>(timestamp => timestamp == 857),
                    It.Is<short>(deadline => deadline == 1440),
                    It.Is<byte[]>(senderPublicKey => senderPublicKey.Length == 32 && senderPublicKey[0] == 199),
                    It.Is<long>(recipient => recipient == 78880408059L),
                    It.Is<long>(amount => amount == 100000000000000L),
                    It.Is<long>(amountNQT => amountNQT == 100000000L),
                    It.Is<byte[]>(referencedTransactionFullHash => referencedTransactionFullHash == null),
                    It.Is<byte[]>(signature => signature.Length == 64 && signature[0] == 81),
                    It.Is<JToken>(attachment => attachment == null)));
        }
    }
}
