using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Nxt.NET;
using Nxt.NET.Block;
using StructureMap;

namespace Nxt.NETTest
{
    [TestClass]
    public class AccountTest
    {
        private Account _account;
        private Mock<IBlockRepository> _blockRepositoryMock;
        private Mock<IConfiguration> _configurationMock;

        [TestInitialize]
        public void TestInit()
        {
            _blockRepositoryMock = new Mock<IBlockRepository>();
            _configurationMock = new Mock<IConfiguration>();
            ObjectFactory.Configure(x => x.For<IBlockRepository>().Use(_blockRepositoryMock.Object));
            ObjectFactory.Configure(x => x.For<IConfiguration>().Use(_configurationMock.Object));

            _account = new Account(123);
        }

        [TestMethod]
        public void SetOrVerifyShouldAcceptNewValues()
        {
            var actual = _account.SetAndVerifyPublicKey(new byte[0], 1);

            Assert.IsTrue(actual);
        }

        [TestMethod]
        public void SetOrVerifyShouldAcceptDuplicateEqualValues()
        {
            var key = new Byte[] {1, 2, 3};
            _account.SetAndVerifyPublicKey(key, 1);
            var actual = _account.SetAndVerifyPublicKey(key, 1);
            
            Assert.IsTrue(actual);
        }

        [TestMethod]
        public void SetOrVerifyShouldNotAcceptUnEqualValues()
        {
            _account.SetAndVerifyPublicKey(new byte[] {3, 2, 1}, 1);
            var actual = _account.SetAndVerifyPublicKey(new byte[] {1, 2, 3}, 1);

            Assert.IsFalse(actual);
        }

        [TestMethod]
        public void SetOrVerifyShouldAcceptNewKeyWhenPreviousIsNewer()
        {
            _account.ApplyPublicKey(new byte[] { 3, 2, 1 }, 2);
            var actual = _account.SetAndVerifyPublicKey(new byte[] { 1, 2, 3 }, 1);

            Assert.IsTrue(actual);
        }

        [TestMethod]
        public void SetOrVerifyShouldNotAcceptNewKeyWhenPreviousIsOlder()
        {
            _account.ApplyPublicKey(new byte[] { 3, 2, 1 }, 1);
            var actual = _account.SetAndVerifyPublicKey(new byte[] { 1, 2, 3 }, 2);

            Assert.IsFalse(actual);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void ApplyShouldNotAcceptNull()
        {
            _account.ApplyPublicKey(null, 1);
        }

        [TestMethod]
        public void PublicKeyShouldBeNull()
        {
            Assert.IsNull(_account.PublicKey);
        }

        [TestMethod]
        public void ApplyShouldSetPublicKey()
        {
            var expected = new byte[] {1, 2, 3};
            _account.ApplyPublicKey(expected, 1);

            CollectionAssert.AreEqual(expected, _account.PublicKey);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void ApplyShouldNotAcceptPublicKeyMismatch()
        {
            _account.SetAndVerifyPublicKey(new byte[] {1, 2, 3}, 1);
            _account.ApplyPublicKey(new byte[] {3, 2, 1}, 1);
        }
    }
}
