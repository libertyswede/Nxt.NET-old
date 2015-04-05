using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json.Linq;
using Nxt.NET;
using Nxt.NET.Response;
using Nxt.NET.Transaction;

namespace Nxt.NETTest.Response
{
    [TestClass]
    public class GetUnconfirmedTransactionsResponseTest
    {
        private List<ITransaction> _expected;

        [TestInitialize]
        public void TestInit()
        {
            _expected = new List<ITransaction>
            {
                new Mock<ITransaction>().Object,
                new Mock<ITransaction>().Object,
                new Mock<ITransaction>().Object
            };
        }

        [TestMethod]
        public void NewGetUnconfirmedTransactionsResponseShouldUseTransactionsFromTransactionProcessor()
        {
            var transactionParserMock = new Mock<ITransactionParser>();
            transactionParserMock.Setup(tp => tp.ParseTransactions(It.IsAny<JToken>()))
                .Returns(_expected);

            var response = new GetUnconfirmedTransactionsResponse(transactionParserMock.Object, "{unconfirmedTransactions:[]}");

            Assert.AreEqual(_expected.Count, response.Transactions.Count);
            _expected.ForEach(t => CollectionAssert.Contains(response.Transactions.ToList(), t));
        }
    }
}
