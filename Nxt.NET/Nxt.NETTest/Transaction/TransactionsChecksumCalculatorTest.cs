using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Nxt.NET;
using Nxt.NET.Crypto;
using Nxt.NET.Transaction;

namespace Nxt.NETTest.Transaction
{
    [TestClass]
    public class TransactionsChecksumCalculatorTest
    {
        private Mock<ICrypto> _cryptoMock;
        private Mock<ITransactionRepository> _transactionRepositoryMock;
        private TransactionsChecksumCalculator _transactionsChecksumCalculator;
        private Mock<ITransaction> _transaction1Mock;
        private Mock<ITransaction> _transaction2Mock;

        [TestInitialize]
        public void TestInit()
        {
            var cryptoFactoryMock = new Mock<ICryptoFactory>();
            _cryptoMock = new Mock<ICrypto>();
            _transactionRepositoryMock = new Mock<ITransactionRepository>();
            _transaction1Mock = new Mock<ITransaction>();
            _transaction2Mock = new Mock<ITransaction>();

            _transactionRepositoryMock.Setup(r => r.GetAllTransactions())
                .ReturnsAsync(new List<ITransaction> {_transaction1Mock.Object, _transaction2Mock.Object});
            cryptoFactoryMock.Setup(f => f.Create()).Returns(_cryptoMock.Object);

            _transactionsChecksumCalculator = new TransactionsChecksumCalculator(cryptoFactoryMock.Object,
                _transactionRepositoryMock.Object);
        }

        [TestMethod]
        public async Task TestMethod1()
        {
            var transaction1Bytes = new byte[] {1, 2, 3};
            var transaction2Bytes = new byte[] {3, 2, 1};
            var expected = new byte[] {3, 3, 3};

            _transaction1Mock.Setup(s => s.GetBytes(false)).Returns(transaction1Bytes);
            _transaction2Mock.Setup(s => s.GetBytes(false)).Returns(transaction2Bytes);
            _cryptoMock.SetupGet(c => c.Hash).Returns(expected);

            var actual = await _transactionsChecksumCalculator.CalculateAllTransactionsChecksum();

            CollectionAssert.AreEqual(expected, actual);
            _cryptoMock.Verify(c => c.TransformBlock(It.Is<byte[]>(b => b.SequenceEqual(transaction1Bytes))), Times.Once);
            _cryptoMock.Verify(c => c.TransformFinalBlock(It.Is<byte[]>(b => b.SequenceEqual(transaction2Bytes))), Times.Once);
        }
    }
}
