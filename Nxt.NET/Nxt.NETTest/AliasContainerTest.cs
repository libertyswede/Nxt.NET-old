using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Nxt.NET;
using Nxt.NET.Transaction;

namespace Nxt.NETTest
{
    [TestClass]
    public class AliasContainerTest
    {
        private const int Id = 1234;
        private const string Name = "Da Name";
        private const string Uri = "Some uri";
        private const int Timestamp = 2345;

        private readonly Mock<IAccount> _accountMock = new Mock<IAccount>();
        private Mock<ITransaction> _transactionMock;
        private Mock<IAliasAssignmentAttachment> _attachmentMock;

        private AliasContainer _aliasContainer;

        [TestInitialize]
        public void TestInit()
        {
            _transactionMock = new Mock<ITransaction>();
            _attachmentMock = new Mock<IAliasAssignmentAttachment>();

            _transactionMock.SetupGet(t => t.Id).Returns(Id);
            _transactionMock.SetupGet(t => t.BlockTimestamp).Returns(Timestamp);
            _attachmentMock.SetupGet(a => a.AliasName).Returns(Name);
            _attachmentMock.SetupGet(a => a.AliasUri).Returns(Uri);

            _aliasContainer = new AliasContainer();
        }

        [TestMethod]
        public void AddOrUpdateShouldAdd()
        {
            _aliasContainer.AddOrUpdate(_accountMock.Object, _transactionMock.Object, _attachmentMock.Object);

            var alias = _aliasContainer.Aliases.Single();
            Assert.AreSame(_accountMock.Object, alias.Account);
            Assert.AreEqual(Id, alias.Id);
            Assert.AreEqual(Name, alias.Name);
            Assert.AreEqual(Uri, alias.Uri);
            Assert.AreEqual(Timestamp, alias.Timestamp);
            Assert.AreSame(_aliasContainer.GetAlias(Id), _aliasContainer.GetAlias(Name));
        }

        [TestMethod]
        public void AddOrUpdateShouldUpdateUriAndTimestamp()
        {
            _aliasContainer.AddOrUpdate(_accountMock.Object, _transactionMock.Object, _attachmentMock.Object);
            const string newUri = "new uri";
            const int newTimestamp = 3456;
            _transactionMock.SetupGet(t => t.BlockTimestamp).Returns(newTimestamp);
            _attachmentMock.SetupGet(a => a.AliasUri).Returns(newUri);

            _aliasContainer.AddOrUpdate(_accountMock.Object, _transactionMock.Object, _attachmentMock.Object);

            var alias = _aliasContainer.Aliases.Single();
            Assert.AreEqual(newUri, alias.Uri);
            Assert.AreEqual(newTimestamp, alias.Timestamp);
            Assert.AreSame(_aliasContainer.GetAlias(Id), _aliasContainer.GetAlias(Name));
        }

        [TestMethod]
        public void RemoveShouldRemoveAlias()
        {
            _aliasContainer.AddOrUpdate(_accountMock.Object, _transactionMock.Object, _attachmentMock.Object);
            var alias = _aliasContainer.Aliases.Single();

            _aliasContainer.Remove(alias);

            Assert.AreEqual(0, _aliasContainer.Aliases.Count);
            Assert.IsNull(_aliasContainer.GetAlias(Id));
            Assert.IsNull(_aliasContainer.GetAlias(Name));
        }

        [TestMethod]
        public void ClearShouldRemoveAllAliases()
        {
            _aliasContainer.AddOrUpdate(_accountMock.Object, _transactionMock.Object, _attachmentMock.Object);

            _aliasContainer.Clear();

            Assert.AreEqual(0, _aliasContainer.Aliases.Count);
            Assert.IsNull(_aliasContainer.GetAlias(Id));
            Assert.IsNull(_aliasContainer.GetAlias(Name));
        }
    }
}
