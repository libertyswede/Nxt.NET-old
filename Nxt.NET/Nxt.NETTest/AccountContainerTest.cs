using System;
using System.Globalization;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Nxt.NET;
using Nxt.NET.Block;
using Nxt.NET.Crypto;
using StructureMap;

namespace Nxt.NETTest
{
    [TestClass]
    public class AccountContainerTest
    {
        private AccountContainer _accountContainer;
        private Mock<ICryptoFactory> _cryptoFactoryMock;
        private Mock<ICrypto> _cryptoMock;
        private Mock<IConfiguration> _configurationMock;

        [TestInitialize]
        public void TestInit()
        {
            var blockRepository = new Mock<IBlockRepository>().Object;
            ObjectFactory.Configure(x => x.For<IBlockRepository>().Use(blockRepository));
            _cryptoFactoryMock = new Mock<ICryptoFactory>();
            _cryptoMock = new Mock<ICrypto>();
            _configurationMock = new Mock<IConfiguration>();
            ObjectFactory.Configure(x => x.For<IConfiguration>().Use(_configurationMock.Object));

            _cryptoMock.Setup(c => c.ReedSolomonDecode(It.IsAny<string>())).Returns((string s) => Int64.Parse(s));
            _cryptoMock.Setup(c => c.ReedSolomonEncode(It.IsAny<long>())).Returns((long l) => l.ToString(CultureInfo.InvariantCulture));
            _cryptoFactoryMock.Setup(cf => cf.Create()).Returns(_cryptoMock.Object);

            _accountContainer = new AccountContainer(_cryptoFactoryMock.Object, blockRepository, new Mock<IBlockchainProcessor>().Object);
        }

        [TestMethod]
        public void GetAccountShouldReturnNull()
        {
            Assert.IsNull(_accountContainer.GetAccount(1234));
        }

        [TestMethod]
        public void GetAccountShouldReturnExistingAccount()
        {
            const long id = 1234;

            _accountContainer.GetOrAddAccount(id);

            Assert.IsNotNull(_accountContainer.GetAccount(id));
        }

        [TestMethod]
        public void GetOrAddAccountShouldAddNewAccount()
        {
            const long id = 1234;

            var account = _accountContainer.GetOrAddAccount(id);

            Assert.AreEqual(id, account.Id);
        }

        [TestMethod]
        public void GetOrAddAccountTwiceShouldReturnExistingAccount()
        {
            const long id = 1234;

            var account1 = _accountContainer.GetOrAddAccount(id);
            var account2 = _accountContainer.GetOrAddAccount(id);

            Assert.AreSame(account1, account2);
        }

        [TestMethod]
        public void ClearShouldRemoveAllExistingAccounts()
        {
            const long id = 123;
            _accountContainer.GetOrAddAccount(id);

            _accountContainer.Clear();

            Assert.AreEqual(0, _accountContainer.GetAllAccounts().Count);
        }

        [TestMethod]
        public void GetAllAccountsShouldReturnAllAccounts()
        {
            _accountContainer.GetOrAddAccount(123);
            _accountContainer.GetOrAddAccount(1234);
            _accountContainer.GetOrAddAccount(12345);

            var allAccounts = _accountContainer.GetAllAccounts();

            Assert.IsTrue(allAccounts.Any(a => a.Id == 123));
            Assert.IsTrue(allAccounts.Any(a => a.Id == 1234));
            Assert.IsTrue(allAccounts.Any(a => a.Id == 12345));
        }
    }
}
