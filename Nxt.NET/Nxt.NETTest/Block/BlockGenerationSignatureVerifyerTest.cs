using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Nxt.NET;
using Nxt.NET.Block;
using Nxt.NET.Crypto;

namespace Nxt.NETTest.Block
{
    [TestClass]
    public class BlockGenerationSignatureVerifyerTest
    {
        private Mock<IAccountContainer> _accountContainerMock;
        private BlockGenerationSignatureVerifyer _verifyer;
        private Mock<IBlock> _blockMock;
        private Mock<IBlock> _previousBlockMock;
        private Mock<ICrypto> _cryptoMock;
        private Mock<IAccount> _accountMock;
        private Mock<IAccountBalance> _accountBalanceMock;
        private const long BlockGeneratorId = 42;

        [TestInitialize]
        public void TestInit()
        {
            var cryptoFactoryMock = new Mock<ICryptoFactory>();
            _cryptoMock = new Mock<ICrypto>();
            _accountContainerMock = new Mock<IAccountContainer>();
            _accountBalanceMock = new Mock<IAccountBalance>();
            _accountMock = new Mock<IAccount>();
            _blockMock = new Mock<IBlock>();
            _previousBlockMock = new Mock<IBlock>();

            _accountContainerMock.Setup(ac => ac.GetAccount(It.Is<long>(id => id == BlockGeneratorId)))
                .Returns(_accountMock.Object);
            _accountMock.SetupGet(a => a.Balance).Returns(_accountBalanceMock.Object);
            _blockMock.SetupGet(b => b.GeneratorId).Returns(BlockGeneratorId);
            cryptoFactoryMock.Setup(cf => cf.Create()).Returns(_cryptoMock.Object);
            _accountBalanceMock.Setup(ab => ab.GetEffectiveBalanceNXT()).Returns(10);
            _blockMock.SetupGet(b => b.Version).Returns(2);
            _previousBlockMock.SetupGet(b => b.BaseTarget).Returns(10);
            _blockMock.SetupGet(b => b.Timestamp).Returns(10);
            SetupCryptoVerification(true);

            _verifyer = new BlockGenerationSignatureVerifyer(cryptoFactoryMock.Object, _accountContainerMock.Object);
        }

        [TestMethod]
        public void VerifyShouldReturnTrue()
        {
            var actual = _verifyer.VerifyGenerationSignature(_blockMock.Object, _previousBlockMock.Object);

            Assert.IsTrue(actual);
        }

        [TestMethod]
        public void VerifyShouldReturnTrueForVersionOne()
        {
            _blockMock.SetupGet(b => b.Version).Returns(1);
            _cryptoMock.Setup(c => c.ComputeHash(It.IsAny<byte[]>())).Returns(new byte[0]);

            var actual = _verifyer.VerifyGenerationSignature(_blockMock.Object, _previousBlockMock.Object);

            Assert.IsTrue(actual);
        }

        [TestMethod]
        public void VerifyShouldNotAcceptDifferentSignatureHashes()
        {
            _cryptoMock.SetupGet(c => c.Hash).Returns(new byte[] {1});

            var actual = _verifyer.VerifyGenerationSignature(_blockMock.Object, _previousBlockMock.Object);

            Assert.IsFalse(actual);
        }

        [TestMethod]
        public void VerifyShouldNotAcceptCryptoVerificationFail()
        {
            _blockMock.SetupGet(b => b.Version).Returns(1);
            SetupCryptoVerification(false);

            var actual = _verifyer.VerifyGenerationSignature(_blockMock.Object, _previousBlockMock.Object);

            Assert.IsFalse(actual);
        }

        [TestMethod]
        public void VerifyShouldNotAcceptNonExistingAccount()
        {
            _accountContainerMock.Setup(ac => ac.GetAccount(It.Is<long>(id => id == BlockGeneratorId)))
                .Returns((IAccount)null);

            var actual = _verifyer.VerifyGenerationSignature(_blockMock.Object, _previousBlockMock.Object);

            Assert.IsFalse(actual);
        }

        [TestMethod]
        public void VerifyShouldNotAcceptAccountWithZeroEffectiveBalance()
        {
            _accountBalanceMock.Setup(ab => ab.GetEffectiveBalanceNXT()).Returns(0);

            var actual = _verifyer.VerifyGenerationSignature(_blockMock.Object, _previousBlockMock.Object);

            Assert.IsFalse(actual);
        }

        private void SetupCryptoVerification(bool value)
        {
            _cryptoMock.Setup(
                c => c.Verify(It.IsAny<byte[]>(), It.IsAny<byte[]>(), It.IsAny<byte[]>(), It.IsAny<bool>()))
                .Returns(value);
        }
    }
}
