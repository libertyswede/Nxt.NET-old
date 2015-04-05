using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Nxt.NET;
using Nxt.NET.Transaction;
using Nxt.NET.Transaction.Types;
using StructureMap;

namespace Nxt.NETTest.Transaction
{
    [TestClass]
    public class TransactionTypeFactoryTest
    {
        private TransactionTypeFactory _transactionTypeFactory;

        [TestInitialize]
        public void TestInit()
        {
            ObjectFactory.Configure(x => x.For<IAliasContainer>().Use(new Mock<IAliasContainer>().Object));
            ObjectFactory.Configure(x => x.For<IConfiguration>().Use(new Mock<IConfiguration>().Object));

            _transactionTypeFactory = new TransactionTypeFactory();
        }

        [TestMethod]
        public void FindTransactionTypeShouldReturnOrdinaryPayment()
        {
            var actual = _transactionTypeFactory.FindTransactionType(0, 0);

            Assert.IsTrue(actual is OrdinaryPayment);
        }

        [TestMethod]
        public void FindTransactionTypeShouldReturnArbitraryMessage()
        {
            var actual = _transactionTypeFactory.FindTransactionType(1, 0);

            Assert.IsTrue(actual is ArbitraryMessage);
        }

        [TestMethod]
        public void FindTransactionTypeShouldReturnAliasAssignment()
        {
            var actual = _transactionTypeFactory.FindTransactionType(1, 1);

            Assert.IsTrue(actual is AliasAssignment);
        }

        [TestMethod]
        [ExpectedException(typeof(NotSupportedException))]
        public void FindTransactionShouldNotAllowFaultyType()
        {
            _transactionTypeFactory.FindTransactionType(2, 0);
        }

        [TestMethod]
        [ExpectedException(typeof(NotSupportedException))]
        public void FindTransactionShouldNotAllowFaultySubType()
        {
            _transactionTypeFactory.FindTransactionType(0, 1);
        }
    }
}
