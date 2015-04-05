using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Nxt.NET.Transaction;

namespace Nxt.NETTest.Transaction
{
    [TestClass]
    public class UnconfirmedTransactionApplierTest
    {
        private UnconfirmedTransactionApplier _unconfirmedTransactionApplier;
        private Mock<ITransaction> _transactionMock;

        [TestInitialize]
        public void TestInit()
        {
            _transactionMock = new Mock<ITransaction>();
            _transactionMock.Setup(t => t.ApplyUnconfirmed()).Returns(true);

            _unconfirmedTransactionApplier = new UnconfirmedTransactionApplier();
        }

        [TestMethod]
        public void ApplyUnconfirmedTransactionShouldCallApplyUnconfirmed()
        {
            var actual = _unconfirmedTransactionApplier.ApplyUnconfirmedTransaction(_transactionMock.Object);

            _transactionMock.Verify(t => t.ApplyUnconfirmed());
            Assert.IsTrue(actual);
        }

        [TestMethod]
        public void DisposeShouldUndoUnconfirmed()
        {
            var transaction2Mock = new Mock<ITransaction>();
            transaction2Mock.Setup(t => t.ApplyUnconfirmed()).Returns(false);
            _unconfirmedTransactionApplier.ApplyUnconfirmedTransaction(_transactionMock.Object);
            _unconfirmedTransactionApplier.ApplyUnconfirmedTransaction(transaction2Mock.Object);

            _unconfirmedTransactionApplier.Dispose();

            _transactionMock.Verify(t => t.UndoUnconfirmed(), Times.Once);
            transaction2Mock.Verify(t => t.UndoUnconfirmed(), Times.Never);
        }
    }
}
