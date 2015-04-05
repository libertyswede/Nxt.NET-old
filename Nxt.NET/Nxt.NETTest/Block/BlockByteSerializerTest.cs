using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Nxt.NET;
using Nxt.NET.Block;
using Nxt.NET.Transaction;

namespace Nxt.NETTest.Block
{
    [TestClass]
    public class BlockByteSerializerTest
    {
        private BlockByteSerializer _blockByteSerializer;

        [TestInitialize]
        public void TestInit()
        {
            _blockByteSerializer = new BlockByteSerializer();
        }

        [TestMethod]
        public void SerializeBytesForGenesisBlockShouldMatchExpectedByteArray()
        {
            var expected = new byte[]
            {
                255, 255, 255, 255, 
                0, 0, 0, 0, 
                0, 0, 0, 0, 0, 0, 0, 0, 
                73, 0, 0, 0, 
                0, 202, 154, 59, 
                0, 0, 0, 0, 
                128, 36, 0, 0, 
                114, 200, 169, 46, 255, 251, 216, 105, 90, 134, 110, 171, 177, 60, 164, 96, 162, 247, 205, 243, 40, 59, 130, 239, 177, 99, 54, 13, 110, 236, 148, 105, 
                18, 89, 236, 33, 211, 26, 48, 137, 141, 124, 209, 96, 159, 128, 217, 102, 139, 71, 120, 227, 217, 126, 148, 16, 68, 179, 159, 12, 68, 210, 229, 27, 
                0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 
                105, 212, 38, 196, 152, 183, 10, 198, 209, 103, 129, 128, 53, 101, 39, 193, 254, 224, 48, 173, 115, 47, 191, 118, 114, 194, 38, 109, 22, 106, 76, 8, 207, 143, 222, 180, 82, 79, 209, 180, 150, 187, 202, 171, 3, 250, 110, 103, 118, 15, 109, 164, 82, 37, 20, 2, 36, 144, 21, 72, 108, 72, 114, 17
            };
            var blockMock = CreateGenesisBlockMock();

            var actual = _blockByteSerializer.SerializeBytes(blockMock.Object);

            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void SerializeBytesForModifiedGenesisBlockShouldMatchExpectedByteArray()
        {
            var expected = new byte[]
            {
                3, 0, 0, 0, 
                0, 0, 0, 0, 
                0, 0, 0, 0, 0, 0, 0, 0, 
                73, 0, 0, 0, 
                0, 0, 138, 93, 120, 69, 99, 1,
                0, 0, 0, 0, 0, 0, 0, 0, 
                128, 36, 0, 0, 
                114, 200, 169, 46, 255, 251, 216, 105, 90, 134, 110, 171, 177, 60, 164, 96, 162, 247, 205, 243, 40, 59, 130, 239, 177, 99, 54, 13, 110, 236, 148, 105, 
                18, 89, 236, 33, 211, 26, 48, 137, 141, 124, 209, 96, 159, 128, 217, 102, 139, 71, 120, 227, 217, 126, 148, 16, 68, 179, 159, 12, 68, 210, 229, 27, 
                0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 
                0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 
                105, 212, 38, 196, 152, 183, 10, 198, 209, 103, 129, 128, 53, 101, 39, 193, 254, 224, 48, 173, 115, 47, 191, 118, 114, 194, 38, 109, 22, 106, 76, 8, 207, 143, 222, 180, 82, 79, 209, 180, 150, 187, 202, 171, 3, 250, 110, 103, 118, 15, 109, 164, 82, 37, 20, 2, 36, 144, 21, 72, 108, 72, 114, 17
            };
            var blockMock = CreateGenesisBlockMock();
            blockMock.SetupGet(b => b.Version).Returns(3);
            blockMock.SetupGet(b => b.PreviousBlockHash).Returns(new byte[32]);

            var actual = _blockByteSerializer.SerializeBytes(blockMock.Object);

            CollectionAssert.AreEqual(expected, actual);
        }

        private static Mock<IBlock> CreateGenesisBlockMock()
        {
            var blockMock = new Mock<IBlock>();
            var transactionsCollectionMock = new Mock<IReadOnlyCollection<ITransaction>>();
            transactionsCollectionMock.SetupGet(c => c.Count).Returns(Genesis.GenesisAmounts.Count());

            var payloadHash = new byte[]
            {
                114, 200, 169, 46, 255, 251, 216, 105, 90, 134, 110, 171, 177, 60, 164, 96, 162, 247, 205, 243, 40, 59,
                130, 239, 177, 99, 54, 13, 110, 236, 148, 105
            };

            blockMock.SetupGet(b => b.Transactions).Returns(transactionsCollectionMock.Object);
            blockMock.SetupGet(b => b.Version).Returns(-1);
            blockMock.SetupGet(b => b.Timestamp).Returns(0);
            blockMock.SetupGet(b => b.PreviousBlockId).Returns((long?)null);
            blockMock.SetupGet(b => b.TotalAmount).Returns(Constants.MaxBalanceNqt);
            blockMock.SetupGet(b => b.TotalFee).Returns(0);
            blockMock.SetupGet(b => b.PayloadLength).Returns(Genesis.GenesisAmounts.Count() * 128);
            blockMock.SetupGet(b => b.PayloadHash).Returns(payloadHash);
            blockMock.SetupGet(b => b.GeneratorPublicKey).Returns(Genesis.CreatorPublicKey);
            blockMock.SetupGet(b => b.GenerationSignature).Returns(new byte[64]);
            blockMock.SetupGet(b => b.BlockSignature).Returns(Genesis.GenesisBlockSignature);

            return blockMock;
        }
    }
}
