using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Numerics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Nxt.NET;
using Nxt.NET.Block;
using Nxt.NET.Crypto;
using Nxt.NET.Transaction;
using StructureMap;

namespace Nxt.NETTest.Block
{
    [TestClass]
    public class BlockFactoryTest
    {
        private BlockFactory _blockFactory;
        private Mock<ICryptoFactory> _cryptoFactoryMock;
        private List<ITransaction> _transactions;
        private Mock<ICrypto> _cryptoMock;
        private readonly byte[] _dummyBytes = { 1, 2, 3, 4, 5, 6, 7, 8 };
        private Mock<IBlock> _blockMock;
        private Mock<ITransaction> _transaction1Mock;
        private Mock<ITransaction> _transaction2Mock;
        private Mock<ITransaction> _transaction3Mock;

        [TestInitialize]
        public void TestInit()
        {
            _blockMock = new Mock<IBlock>();
            _blockMock.SetupAllProperties();
            ObjectFactory.Configure(x => x.For<IBlock>().Use(_blockMock.Object));

            _cryptoFactoryMock = new Mock<ICryptoFactory>();
            _cryptoMock = new Mock<ICrypto>();
            _transaction1Mock = new Mock<ITransaction>();
            _transaction2Mock = new Mock<ITransaction>();
            _transaction3Mock = new Mock<ITransaction>();
            _transactions = new List<ITransaction>
            {
                _transaction1Mock.Object,
                _transaction2Mock.Object,
                _transaction3Mock.Object
            };

            _cryptoFactoryMock.Setup(cf => cf.Create()).Returns(_cryptoMock.Object);

            _blockFactory = new BlockFactory(_cryptoFactoryMock.Object);
        }

        [TestMethod]
        [ExpectedException(typeof(NxtException))]
        public void CreateBlockShouldNotAcceptTooManyTransactions()
        {
            var listMock = new Mock<IList<ITransaction>>();
            listMock.SetupGet(l => l.Count).Returns(Constants.MaxNumberOfTransactions + 1);

            _blockFactory.Create(1, 1, 1, 0, 0, 0, null, null, null, null, null, listMock.Object);
        }

        [TestMethod]
        [ExpectedException(typeof(NxtException))]
        public void CreateBlockShouldNotAcceptTooBigPayloadLength()
        {
            _blockFactory.Create(1, 1, 1, 0, 0, Constants.MaxPayloadLength + 1, null, null, null, null, null,
                _transactions);
        }

        [TestMethod]
        public void CreateBlockShouldSetPreviousBlockHashToNullIfVersionIs1()
        {
            _cryptoMock.Setup(c => c.ComputeHash(It.IsAny<byte[]>())).Returns(_dummyBytes);

            var block = _blockFactory.Create(1, 1, 1, 1, 1, 1, _dummyBytes, _dummyBytes, _dummyBytes, _dummyBytes,
                _dummyBytes, _transactions);

            Assert.IsNull(block.PreviousBlockHash);
        }

        [TestMethod]
        public void CreateBlockShouldOrderTransactionsById()
        {
            _cryptoMock.Setup(c => c.ComputeHash(It.IsAny<byte[]>())).Returns(_dummyBytes);
            _transaction1Mock.SetupGet(t => t.Id).Returns(2);
            _transaction2Mock.SetupGet(t => t.Id).Returns(3);
            _transaction3Mock.SetupGet(t => t.Id).Returns(1);

            var block = _blockFactory.Create(1, 1, 1, 1, 1, 1, _dummyBytes, _dummyBytes, _dummyBytes, _dummyBytes,
                _dummyBytes, _transactions);

            var previousTransactionId = 0L;
            foreach (var transaction in block.Transactions)
            {
                Assert.IsTrue(transaction.Id > previousTransactionId);
                previousTransactionId = transaction.Id;
            }
        }

        [TestMethod]
        public void CreateBlockShouldUpdatePayloadHashIfNull()
        {
            var expected = new byte[] {1, 2, 3};
            _cryptoMock.SetupGet(c => c.Hash).Returns(expected);
            _cryptoMock.Setup(c => c.ComputeHash(It.IsAny<byte[]>())).Returns(_dummyBytes);

            var block = _blockFactory.Create(1, 1, 1, 0, 0, 384, null, _dummyBytes, _dummyBytes, _dummyBytes,
                _dummyBytes, _transactions);

            CollectionAssert.AreEqual(expected, block.PayloadHash);
        }

        [TestMethod]
        [ExpectedException(typeof(NxtException))]
        public void CreateBlockShouldNotAcceptEmptyBlockSignature()
        {
            var expected = new byte[] { 1, 2, 3 };
            _cryptoMock.SetupGet(c => c.Hash).Returns(expected);
            _cryptoMock.Setup(c => c.ComputeHash(It.IsAny<byte[]>())).Returns(_dummyBytes);

            var block = _blockFactory.Create(1, 1, 1, 0, 0, 384, _dummyBytes, _dummyBytes, _dummyBytes, new byte[0], 
                _dummyBytes, _transactions);

            CollectionAssert.AreEqual(expected, block.PayloadHash);
        }

        [TestMethod]
        public void CreateBlockShouldSetBlockHash()
        {
            var expected = new byte[] {8, 7, 6, 5, 4, 3, 2, 1};
            _cryptoMock.Setup(c => c.ComputeHash(It.IsAny<byte[]>())).Returns(expected);

            var block = _blockFactory.Create(1, 1, 1, 0, 0, 384, _dummyBytes, _dummyBytes, _dummyBytes, _dummyBytes,
                _dummyBytes, _transactions);

            CollectionAssert.AreEqual(expected, block.BlockHash);
        }

        [TestMethod]
        public void CreateBlockFromRepositoryShouldSetBlockHash()
        {
            var expected = new byte[] { 8, 7, 6, 5, 4, 3, 2, 1 };
            _cryptoMock.Setup(c => c.ComputeHash(It.IsAny<byte[]>())).Returns(expected);
            var transactions = new ReadOnlyCollection<ITransaction>(_transactions);

            var block = _blockFactory.Create(1, 1, 1, 0, 0, 384, _dummyBytes, _dummyBytes, _dummyBytes, _dummyBytes,
                _dummyBytes, transactions, BigInteger.Zero, 0, 0, 0, 0, 0, 0);

            CollectionAssert.AreEqual(expected, block.BlockHash);
        }

        [TestMethod]
        public void CreateBlockShouldSetId()
        {
            var generatorPublicKey = new byte[] {2, 3, 4, 5, 6, 7, 8, 9};
            const long expectedId = 650777868590383874;
            _cryptoMock.Setup(c => c.ComputeHash(It.IsAny<byte[]>())).Returns(generatorPublicKey);

            var block = _blockFactory.Create(1, 1, 1, 0, 0, 384, _dummyBytes, generatorPublicKey, _dummyBytes, _dummyBytes,
                _dummyBytes, _transactions);

            Assert.AreEqual(expectedId, block.Id);
        }
    }
}
