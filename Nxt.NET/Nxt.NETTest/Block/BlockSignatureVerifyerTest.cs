using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Nxt.NET;
using Nxt.NET.Block;
using Nxt.NET.Crypto;

namespace Nxt.NETTest.Block
{
    [TestClass]
    public class BlockSignatureVerifyerTest
    {
        private Mock<ICryptoFactory> _cryptoFactoryMock;
        private Mock<IAccountContainer> _accountContainerMock;
        private BlockSignatureVerifyer _blockSignatureVerifyer;
        private Mock<IBlock> _blockMock;
        private Mock<IAccount> _accountMock;
        private Mock<ICrypto> _cryptoMock;

        [TestInitialize]
        public void TestInit()
        {
            _cryptoFactoryMock = new Mock<ICryptoFactory>();
            _cryptoMock = new Mock<ICrypto>();
            _accountContainerMock = new Mock<IAccountContainer>();
            _accountMock = new Mock<IAccount>();
            _blockMock = new Mock<IBlock>();

            _cryptoFactoryMock.Setup(cf => cf.Create()).Returns(_cryptoMock.Object);
            SetupCryptoVerify(true);
            _accountMock.Setup(a => a.SetAndVerifyPublicKey(It.IsAny<byte[]>(), It.IsAny<int>())).Returns(true);
            _blockMock.SetupGet(b => b.GeneratorId).Returns(42);
            _accountContainerMock.Setup(ac => ac.GetAccount(It.Is<long>(id => id == 42))).Returns(_accountMock.Object);

            _blockSignatureVerifyer = new BlockSignatureVerifyer(_cryptoFactoryMock.Object, _accountContainerMock.Object);
        }

        [TestMethod]
        public void VerifyBlockSignatureShouldSucceed()
        {
            var actual = _blockSignatureVerifyer.VerifyBlockSignature(_blockMock.Object);

            Assert.IsTrue(actual);
        }

        [TestMethod]
        public void VerifyBlockSignatureShouldNotAcceptNotFoundAccount()
        {
            _accountContainerMock.Setup(ac => ac.GetAccount(It.Is<long>(id => id == 42))).Returns((IAccount)null);

            var actual = _blockSignatureVerifyer.VerifyBlockSignature(_blockMock.Object);

            Assert.IsFalse(actual);
        }

        [TestMethod]
        public void VerifyBlockSignatureShouldNotAcceptCryptoVerificationFailure()
        {
            SetupCryptoVerify(false);

            var actual = _blockSignatureVerifyer.VerifyBlockSignature(_blockMock.Object);

            Assert.IsFalse(actual);
        }

        [TestMethod]
        public void VerifyBlockSignatureShouldNotAcceptVerifyPublicKeyFailure()
        {
            _accountMock.Setup(a => a.SetAndVerifyPublicKey(It.IsAny<byte[]>(), It.IsAny<int>())).Returns(false);

            var actual = _blockSignatureVerifyer.VerifyBlockSignature(_blockMock.Object);

            Assert.IsFalse(actual);
        }

        private void SetupCryptoVerify(bool value)
        {
            _cryptoMock.Setup(
                c => c.Verify(It.IsAny<byte[]>(), It.IsAny<byte[]>(), It.IsAny<byte[]>(), It.IsAny<bool>()))
                .Returns(value);
        }
    }
}
