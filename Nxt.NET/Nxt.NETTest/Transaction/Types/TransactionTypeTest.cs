using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Nxt.NET;
using Nxt.NET.Transaction;
using Nxt.NET.Transaction.Types;

namespace Nxt.NETTest.Transaction.Types
{
    [TestClass]
    public class TransactionTypeTest
    {
        private TransactionType _transactionType;
        private Mock<IConfiguration> _configurationMock;
        private Mock<ITransaction> _transactionMock;
        private Mock<IAccount> _senderAccountMock;
        private Mock<IAccount> _recipientAccountMock;
        private Mock<IAccountBalance> _senderAccountBalanceMock;
        private Mock<IAccountBalance> _recipientAccountBalanceMock;

        [TestInitialize]
        public void TestInit()
        {
            _configurationMock = new Mock<IConfiguration>();
            _transactionMock = new Mock<ITransaction>();
            _senderAccountMock = new Mock<IAccount>();
            _recipientAccountMock = new Mock<IAccount>();
            _senderAccountBalanceMock = new Mock<IAccountBalance>();
            _recipientAccountBalanceMock = new Mock<IAccountBalance>();

            _senderAccountMock.SetupGet(a => a.Balance).Returns(_senderAccountBalanceMock.Object);
            _recipientAccountMock.SetupGet(a => a.Balance).Returns(_recipientAccountBalanceMock.Object);
            _transactionMock.SetupGet(t => t.AmountNQT).Returns(5);
            _transactionMock.SetupGet(t => t.FeeNQT).Returns(1);

            _transactionType = new OrdinaryPayment(_configurationMock.Object);
        }

        [TestMethod]
        public void ApplyUnconfirmedShouldSucceed()
        {
            _senderAccountBalanceMock.SetupGet(b => b.UnconfirmedBalanceNQT).Returns(10);

            var actual = _transactionType.ApplyUnconfirmed(_transactionMock.Object, _senderAccountMock.Object);

            Assert.IsTrue(actual);
            _senderAccountBalanceMock.Verify(b => b.AddToUnconfirmedBalanceNQT(-6));
        }

        [TestMethod]
        public void ApplyUnconfirmedShouldAddUnconfirmedPoolDeposit()
        {
            _senderAccountBalanceMock.SetupGet(b => b.UnconfirmedBalanceNQT).Returns(100000000000);
            SetupUseUnconfirmedPoolDeposit();

            _transactionType.ApplyUnconfirmed(_transactionMock.Object, _senderAccountMock.Object);

            _senderAccountBalanceMock.Verify(b => b.AddToUnconfirmedBalanceNQT(-10000000006));
        }

        [TestMethod]
        public void ApplyUnconfirmedShouldFailWhenSenderAccountDoesNotHaveEnoughNQT()
        {
            _senderAccountBalanceMock.SetupGet(b => b.UnconfirmedBalanceNQT).Returns(2);

            var actual = _transactionType.ApplyUnconfirmed(_transactionMock.Object, _senderAccountMock.Object);

            Assert.IsFalse(actual);
        }

        [TestMethod]
        public void ApplyShouldUpdateSenderAccountBalance()
        {
            SetupUseUnconfirmedPoolDeposit();

            _transactionType.Apply(_transactionMock.Object, _senderAccountMock.Object, _recipientAccountMock.Object);

            _senderAccountBalanceMock.Verify(b => b.AddToBalanceNQT(-6));
            _senderAccountBalanceMock.Verify(b => b.AddToUnconfirmedBalanceNQT(10000000000));
        }

        [TestMethod]
        public void UndoUnconfirmedShouldUpdateSenderBalance()
        {
            SetupUseUnconfirmedPoolDeposit();

            _transactionType.UndoUnconfirmed(_transactionMock.Object, _senderAccountMock.Object);

            _senderAccountBalanceMock.Verify(b => b.AddToUnconfirmedBalanceNQT(6));
            _senderAccountBalanceMock.Verify(b => b.AddToUnconfirmedBalanceNQT(10000000000));
        }

        private void SetupUseUnconfirmedPoolDeposit()
        {
            _transactionMock.SetupGet(t => t.ReferencedTransactionFullHash).Returns(new byte[] {1});
            _transactionMock.SetupGet(t => t.Timestamp).Returns(Constants.ReferencedTransactionFullHashBlockTimestamp + 1);
        }
    }
}
