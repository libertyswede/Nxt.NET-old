using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Nxt.NET.Block;

namespace Nxt.NETTest.Block
{
    [TestClass]
    public class BlockTest
    {
        private Mock<IBlockByteSerializer> _blockByteSerializerMock;
        private NET.Block.Block _block;

        [TestInitialize]
        public void TestInit()
        {
            _blockByteSerializerMock = new Mock<IBlockByteSerializer>();

            _block = new NET.Block.Block(_blockByteSerializerMock.Object);
        }

        [TestMethod]
        public void GetBytesShouldUseBlockByteSerializer()
        {
            var expected = new byte[] {3, 4, 5};
            _blockByteSerializerMock.Setup(s => s.SerializeBytes(It.IsAny<IBlock>())).Returns(expected);

            var actual = _block.GetBytes();

            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void IsGenesisBlockIsTrue()
        {
            _block.Id = 2680262203532249785L;

            var actual = _block.IsGenesisBlock();

            Assert.IsTrue(actual);
        }

        [TestMethod]
        public void PresentationIdShouldReturnPositiveValue()
        {
            _block.Id = -2;

            Assert.AreEqual(UInt64.MaxValue - 1, _block.PresentationId);
        }

        
    }
}
