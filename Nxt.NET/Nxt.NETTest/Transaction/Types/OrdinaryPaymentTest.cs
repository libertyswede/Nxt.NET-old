using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Nxt.NET;
using Nxt.NET.Transaction;
using Nxt.NET.Transaction.Types;

namespace Nxt.NETTest.Transaction.Types
{
    [TestClass]
    public class OrdinaryPaymentTest
    {
        private OrdinaryPayment _ordinaryPayment;
        private Mock<ITransaction> _transactionMock;
        private Mock<IAccount> _senderAccountMock;
        private Mock<IAccount> _recipientAccountMock;
        private Mock<IAccountBalance> _recipientAccountBalanceMock;

        [TestInitialize]
        public void TestInit()
        {
            _transactionMock = new Mock<ITransaction>();
            _senderAccountMock = new Mock<IAccount>();
            _recipientAccountMock = new Mock<IAccount>();
            _recipientAccountBalanceMock = new Mock<IAccountBalance>();
            _senderAccountMock.SetupGet(a => a.Balance).Returns(new Mock<IAccountBalance>().Object);
            _recipientAccountMock.SetupGet(a => a.Balance).Returns(_recipientAccountBalanceMock.Object);

            _ordinaryPayment = new OrdinaryPayment(new Mock<IConfiguration>().Object);
        }

        [TestMethod]
        [ExpectedException(typeof(NxtValidationException))]
        public void ValidateAttachmentShouldFailWhenAmountIsZero()
        {
            _transactionMock.SetupGet(t => t.AmountNQT).Returns(0);

            _ordinaryPayment.ValidateAttachment(_transactionMock.Object);
        }

        [TestMethod]
        [ExpectedException(typeof(NxtValidationException))]
        public void ValidateAttachmentShouldFailWhenAmountIsTooLarge()
        {
            _transactionMock.SetupGet(t => t.AmountNQT).Returns(Constants.MaxBalanceNqt + 1);

            _ordinaryPayment.ValidateAttachment(_transactionMock.Object);
        }

        [TestMethod]
        public void ApplyUnconfirmedShouldReturnTrue()
        {
            var actual = _ordinaryPayment.ApplyUnconfirmed(_transactionMock.Object, _senderAccountMock.Object);
            
            Assert.IsTrue(actual);
        }

        [TestMethod]
        public void ApplyAttachmentShouldUpdateUnconfirmedBalance()
        {
            _transactionMock.SetupGet(t => t.AmountNQT).Returns(42);

            _ordinaryPayment.Apply(_transactionMock.Object, _senderAccountMock.Object, _recipientAccountMock.Object);

            _recipientAccountBalanceMock.Verify(b => b.AddToBalanceAndUnconfirmedBalanceNQT(42));
        }

        [TestMethod]
        public void UndoAttachmentShouldUpdateUnconfirmedBalance()
        {
            _transactionMock.SetupGet(t => t.AmountNQT).Returns(42);

            _ordinaryPayment.Undo(_transactionMock.Object, _senderAccountMock.Object, _recipientAccountMock.Object);

            _recipientAccountBalanceMock.Verify(b => b.AddToBalanceAndUnconfirmedBalanceNQT(-42));
        }
    }
}
