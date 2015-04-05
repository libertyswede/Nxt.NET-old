using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Nxt.NET;
using Nxt.NET.Block;
using Nxt.NET.Crypto;
using Nxt.NET.Transaction;
using Nxt.NET.Transaction.Types;
using StructureMap;

namespace Nxt.NETTest.Transaction
{
    [TestClass]
    public class TransactionByteSerializerTest
    {
        private Mock<IConvert> _convertMock;
        private Mock<IBlockRepository> _blockRepositoryMock;
        private Mock<ITransaction> _transactionMock;
        private TransactionByteSerializer _transactionByteSerializer;
        private Mock<IBlock> _lastBlockMock;

        [TestInitialize]
        public void TestInit()
        {
            _convertMock = new Mock<IConvert>();
            _blockRepositoryMock = new Mock<IBlockRepository>();
            _transactionMock = new Mock<ITransaction>();
            _lastBlockMock = new Mock<IBlock>();

            ObjectFactory.Configure(x => x.For<IAccountContainer>().Use(new Mock<IAccountContainer>().Object));
            ObjectFactory.Configure(x => x.For<IConfiguration>().Use(new Mock<IConfiguration>().Object));

            _lastBlockMock.SetupGet(b => b.Height).Returns(Constants.NQTBlock + 1);
            _blockRepositoryMock.SetupGet(r => r.LastBlock).Returns(_lastBlockMock.Object);
            _transactionMock.SetupGet(t => t.Height).Returns(Constants.NQTBlock + 1);
            _transactionMock.SetupGet(t => t.TransactionType).Returns(new Mock<ITransactionType>().Object);
            _transactionMock.SetupGet(t => t.SenderPublicKey).Returns(new byte[32]);
            _transactionMock.SetupGet(t => t.ReferencedTransactionFullHash).Returns(new byte[32]);
            _transactionMock.SetupGet(t => t.Signature).Returns(new byte[64]);

            _transactionByteSerializer = new TransactionByteSerializer(_convertMock.Object, _blockRepositoryMock.Object);
        }

        [TestMethod]
        public void GetBytesShouldReturnCorrectBytes()
        {
            var expected = new byte[]
            {
                0, 0, 0, 0, 0, 0, 0, 0, 18, 89, 236, 33, 211, 26, 48, 137, 141, 124, 209, 96, 159, 128, 217, 102, 139,
                71, 120, 227, 217, 126, 148, 16, 68, 179, 159, 12, 68, 210, 229, 27, 151, 205, 122, 181, 35, 91, 70, 2,
                134, 143, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 41, 115, 215, 7, 37, 21, 253, 215, 120, 119, 63,
                155, 108, 48, 139, 1, 213, 32, 85, 95, 65, 42, 92, 234, 123, 220, 6, 157, 195, 203, 93, 7, 23, 8, 226,
                65, 57, 129, 254, 42, 164, 152, 11, 72, 190, 108, 17, 113, 99, 139, 181, 123, 110, 107, 119, 231, 67,
                64, 32, 117, 111, 54, 82, 242
            };
            var transaction = CreateGenesisTransaction();

            var actual = _transactionByteSerializer.SerializeBytes(transaction);

            Assert.AreEqual(expected.Length, actual.Length);
            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void SerializeBytesShouldHaveCorrectSizeWhenUsingNQTStyle()
        {
            var actual = _transactionByteSerializer.SerializeBytes(_transactionMock.Object);

            Assert.AreEqual(160, actual.Length);
        }

        [TestMethod]
        public void SerializeBytesShouldHaveCorrectSizeWhenNotUsingNQTStyle()
        {
            _transactionMock.SetupGet(t => t.Height).Returns(Constants.NQTBlock - 1);

            var actual = _transactionByteSerializer.SerializeBytes(_transactionMock.Object);

            Assert.AreEqual(128, actual.Length);
        }

        [TestMethod]
        public void SerializeBytesShouldHaveCorrectTransactionType()
        {
            var transactionTypeMock = new Mock<ITransactionType>();
            transactionTypeMock.Setup(tt => tt.GetTypeByte()).Returns(1);
            transactionTypeMock.Setup(tt => tt.GetSubtypeByte()).Returns(2);
            _transactionMock.SetupGet(t => t.TransactionType).Returns(transactionTypeMock.Object);

            var actual = _transactionByteSerializer.SerializeBytes(_transactionMock.Object);

            Assert.AreEqual(1, actual[0]);
            Assert.AreEqual(2, actual[1]);
        }

        [TestMethod]
        public void SerializeBytesShouldHaveCorrectTimestamp()
        {
            _transactionMock.SetupGet(t => t.Timestamp).Returns(1 + 256*2 + 256*256*3 + 256*256*256*4);

            var actual = _transactionByteSerializer.SerializeBytes(_transactionMock.Object).Skip(2).Take(4).ToArray();

            CollectionAssert.AreEqual(new byte[] {1, 2, 3, 4}, actual);
        }

        [TestMethod]
        public void SerializeBytesShouldHaveCorrectDeadline()
        {
            _transactionMock.SetupGet(t => t.Deadline).Returns(1 + 256 * 2);

            var actual = _transactionByteSerializer.SerializeBytes(_transactionMock.Object).Skip(6).Take(2).ToArray();

            CollectionAssert.AreEqual(new byte[] { 1, 2 }, actual);
        }

        [TestMethod]
        public void SerializeBytesShouldHaveCorrectSenderPublicKey()
        {
            var senderPublicKey = GenerateByteArray(32);
            _transactionMock.SetupGet(t => t.SenderPublicKey).Returns(senderPublicKey);

            var actual = _transactionByteSerializer.SerializeBytes(_transactionMock.Object).Skip(8).Take(32).ToArray();

            CollectionAssert.AreEqual(senderPublicKey, actual);
        }

        [TestMethod]
        public void SerializeBytesShouldHaveCorrectRecipient()
        {
            _transactionMock.SetupGet(t => t.RecipientId).Returns(1 + 256 * 2);

            var actual = _transactionByteSerializer.SerializeBytes(_transactionMock.Object).Skip(40).Take(8).ToArray();

            CollectionAssert.AreEqual(new byte[] { 1, 2, 0, 0, 0, 0, 0, 0 }, actual);
        }
        
        [TestMethod]
        public void SerializeBytesShouldHaveCorrectAmountAndFeeForNQTTransaction()
        {
            _transactionMock.SetupGet(t => t.AmountNQT).Returns(3 + 256*4 + 256*256*5);
            _transactionMock.SetupGet(t => t.FeeNQT).Returns(4 + 256*5 + 256*256*6);

            var actual = _transactionByteSerializer.SerializeBytes(_transactionMock.Object);
            var actualAmount = actual.Skip(48).Take(8).ToArray();
            var actualFee = actual.Skip(56).Take(8).ToArray();

            CollectionAssert.AreEqual(new byte[] { 3, 4, 5, 0, 0, 0, 0, 0 }, actualAmount);
            CollectionAssert.AreEqual(new byte[] { 4, 5, 6, 0, 0, 0, 0, 0 }, actualFee);
        }

        [TestMethod]
        public void SerializeBytesShouldHaveCorrectReferencedTransactionFullHashForNQTTransaction()
        {
            var referencedTransactionFullHash = GenerateByteArray(32);
            _transactionMock.SetupGet(t => t.ReferencedTransactionFullHash).Returns(referencedTransactionFullHash);

            var actual = _transactionByteSerializer.SerializeBytes(_transactionMock.Object).Skip(64).Take(32).ToArray();

            CollectionAssert.AreEqual(referencedTransactionFullHash, actual);
        }

        [TestMethod]
        public void SerializeBytesShouldHaveCorrectNullReferencedTransactionFullHashForNQTTransaction()
        {
            _transactionMock.SetupGet(t => t.ReferencedTransactionFullHash).Returns((byte[]) null);

            var actual = _transactionByteSerializer.SerializeBytes(_transactionMock.Object).Skip(64).Take(32).ToArray();

            CollectionAssert.AreEqual(new Byte[32], actual);
        }

        [TestMethod]
        public void SerializeBytesShouldHaveCorrectAmountAndFeeForNonNQTTransaction()
        {
            _transactionMock.SetupGet(t => t.Height).Returns(Constants.NQTBlock - 1);
            _transactionMock.SetupGet(t => t.AmountNQT).Returns((3 + 256*4 + 256*256*5)*Constants.OneNxt);
            _transactionMock.SetupGet(t => t.FeeNQT).Returns((4 + 256*5 + 256*256*6)*Constants.OneNxt);

            var actual = _transactionByteSerializer.SerializeBytes(_transactionMock.Object);
            var actualAmount = actual.Skip(48).Take(4).ToArray();
            var actualFee = actual.Skip(52).Take(4).ToArray();

            CollectionAssert.AreEqual(new byte[] { 3, 4, 5, 0 }, actualAmount);
            CollectionAssert.AreEqual(new byte[] { 4, 5, 6, 0 }, actualFee);
        }

        [TestMethod]
        public void SerializeBytesShouldHaveCorrectReferencedTransactionIdForNonNQTTransaction()
        {
            _transactionMock.SetupGet(t => t.Height).Returns(Constants.NQTBlock - 1);
            _convertMock.Setup(c => c.FullHashToId(It.IsAny<byte[]>())).Returns(42);
            _transactionMock.SetupGet(t => t.ReferencedTransactionFullHash).Returns(new byte[0]);

            var temp = _transactionByteSerializer.SerializeBytes(_transactionMock.Object);
            var actual = temp.Skip(56).Take(8).ToArray();

            CollectionAssert.AreEqual(new byte[] {42, 0, 0, 0, 0, 0, 0, 0}, actual);
        }

        [TestMethod]
        public void SerializeBytesShouldHaveZeroReferencedTransactionIdForNonNQTTransaction()
        {
            _transactionMock.SetupGet(t => t.Height).Returns(Constants.NQTBlock - 1);
            _transactionMock.SetupGet(t => t.ReferencedTransactionFullHash).Returns((byte[]) null);

            var temp = _transactionByteSerializer.SerializeBytes(_transactionMock.Object);
            var actual = temp.Skip(56).Take(8).ToArray();

            CollectionAssert.AreEqual(new byte[8], actual);
        }

        [TestMethod]
        public void SerializeBytesShouldHaveCorrectSignature()
        {
            var signature = GenerateByteArray(64);
            _transactionMock.SetupGet(t => t.Signature).Returns(signature);

            var actual = _transactionByteSerializer.SerializeBytes(_transactionMock.Object).Skip(96).Take(64).ToArray();

            CollectionAssert.AreEqual(signature, actual);
        }

        [TestMethod]
        public void SerializeBytesShouldHaveZeroSignature()
        {
            _transactionMock.SetupGet(t => t.Signature).Returns((byte[]) null);

            var actual = _transactionByteSerializer.SerializeBytes(_transactionMock.Object).Skip(96).Take(64).ToArray();

            CollectionAssert.AreEqual(new Byte[64], actual);
        }

        [TestMethod]
        public void SerializeBytesShouldSerializeAttachment()
        {
            var expected = new byte[] {1, 2, 3};
            _transactionMock.SetupGet(t => t.Signature).Returns((byte[])null);
            var attachmentMock = new Mock<IAttachment>();
            attachmentMock.Setup(a => a.GetBytes()).Returns(expected);
            _transactionMock.SetupGet(t => t.Attachment).Returns(attachmentMock.Object);

            var actual = _transactionByteSerializer.SerializeBytes(_transactionMock.Object).Skip(160).Take(expected.Length).ToArray();

            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void UseNQTShouldReturnTrue()
        {
            var actual = _transactionByteSerializer.UseNQT(_transactionMock.Object);

            Assert.IsTrue(actual);
        }

        [TestMethod]
        public void UseNQTShouldReturnFalseBecauseTransactionHeightIsTooLow()
        {
            _transactionMock.SetupGet(t => t.Height).Returns(Constants.NQTBlock - 1);

            var actual = _transactionByteSerializer.UseNQT(_transactionMock.Object);

            Assert.IsFalse(actual);
        }

        [TestMethod]
        public void UseNQTShouldReturnFalseBecauseLastBlockHeightIsTooLow()
        {
            _lastBlockMock.SetupGet(b => b.Height).Returns(Constants.NQTBlock - 1);

            var actual = _transactionByteSerializer.UseNQT(_transactionMock.Object);

            Assert.IsFalse(actual);
        }

        private static ITransaction CreateGenesisTransaction()
        {
            ObjectFactory.Configure(x => x.For<ITransaction>().Use<NET.Transaction.Transaction>());
            ObjectFactory.Configure(x => x.For<ITransactionByteSerializer>().Use<TransactionByteSerializer>());
            ObjectFactory.Configure(x => x.For<IConvert>().Use<NET.Convert>());
            ObjectFactory.Configure(x => x.For<IBlockRepository>().Use(new Mock<IBlockRepository>().Object));

            var factory = new TransactionFactory(new CryptoFactory(), new TransactionTypeFactory(), new NET.Convert());

            return factory.Create(0, 0, 0, 0, Genesis.CreatorPublicKey,
                Genesis.GenesisRecipients[0],
                Genesis.GenesisAmounts[0]*Constants.OneNxt, 0, null, Genesis.GenesisSignatures[0]);
        }

        private static byte[] GenerateByteArray(int length)
        {
            var bytes = new byte[length];
            for (var i = 0; i < bytes.Length; i++)
                bytes[i] = (byte)(i * 2);
            return bytes;
        }
    }
}
