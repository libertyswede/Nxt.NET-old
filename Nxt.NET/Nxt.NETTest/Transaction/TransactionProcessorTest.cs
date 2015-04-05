using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Nxt.NET;
using Nxt.NET.Block;
using Nxt.NET.Transaction;

namespace Nxt.NETTest.Transaction
{
    [TestClass]
    public class TransactionProcessorTest
    {
        private TransactionProcessor _transactionProcessor;
        private Mock<ITransactionFactory> _transactionFactory;
        private Mock<IGenesis> _genesis;
        private Mock<ITransactionVerifyer> _transactionVerifyer;

        [TestInitialize]
        public void TestInit()
        {
            _transactionFactory = new Mock<ITransactionFactory>();
            _genesis = new Mock<IGenesis>();
            _transactionVerifyer = new Mock<ITransactionVerifyer>();

            _transactionProcessor = new TransactionProcessor(_transactionFactory.Object, _genesis.Object,
                _transactionVerifyer.Object);
        }

        [TestMethod]
        public void GetGenesisTransactionsShouldReturnGenesisTransactions()
        {
            var expected = new List<ITransaction>();
            _genesis.Setup(g => g.GetGenesisTransactions(It.Is<ITransactionFactory>(tf => tf == _transactionFactory.Object)))
                .Returns(expected);

            var actual = _transactionProcessor.GetGenesisTransactions();

            Assert.AreSame(expected, actual);
        }

        [TestMethod]
        public void ApplyTransactionsShouldCallApplyOnAllTransactions()
        {
            var transaction1 = new Mock<ITransaction>();
            var transaction2 = new Mock<ITransaction>();
            var block = new Mock<IBlock>();
            block.SetupGet(b => b.Transactions)
                .Returns(
                    new ReadOnlyCollection<ITransaction>(new List<ITransaction>
                    {
                        transaction1.Object,
                        transaction2.Object
                    }));

            _transactionProcessor.ApplyTransactions(block.Object);

            transaction1.Verify(t => t.Apply());
            transaction2.Verify(t => t.Apply());
        }

        [TestMethod]
        public async Task VerifyTransactions()
        {
            var block = new Mock<IBlock>();

            await _transactionProcessor.VerifyTransactions(block.Object);

            _transactionVerifyer.Verify(v => v.VerifyTransactions(block.Object));
        }
    }
}
