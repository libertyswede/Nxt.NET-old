using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Nxt.NET;
using Nxt.NET.Block;
using Nxt.NET.Transaction;

namespace Nxt.NETTest
{
    [TestClass]
    public class AccountBalanceTest
    {
        private Mock<IAccount> _accountMock;
        private AccountBalance _accountBalance;
        private Mock<IBlockRepository> _blockRepositoryMock;
        private Mock<IBlock> _lastBlockMock;
        private Mock<IConfiguration> _configurationMock;

        [TestInitialize]
        public void TestInit()
        {
            _accountMock = new Mock<IAccount>();
            _blockRepositoryMock = new Mock<IBlockRepository>();
            _lastBlockMock = new Mock<IBlock>();
            _configurationMock = new Mock<IConfiguration>();
            _blockRepositoryMock.SetupGet(br => br.LastBlock).Returns(_lastBlockMock.Object);
            _lastBlockMock.SetupGet(b => b.Height).Returns(1);
            _configurationMock.SetupGet(c => c.IsTestnet).Returns(false);

            _accountBalance = new AccountBalance(_accountMock.Object, _blockRepositoryMock.Object,
                _configurationMock.Object);
        }

        [TestMethod]
        public void AddToForgedBalanceNQTShouldAddForgedBalance()
        {
            const int expected = 100;

            _accountBalance.AddToForgedBalanceNQT(expected);

            Assert.AreEqual(expected, _accountBalance.ForgedBalanceNQT); 
        }

        [TestMethod]
        public void AddToUnconfirmedBalanceNQTShouldAddUnconfirmedBalanceNQT()
        {
            const long expected = 100;

            _accountBalance.AddToBalanceNQT(expected);
            _accountBalance.AddToUnconfirmedBalanceNQT(expected);

            Assert.AreEqual(expected, _accountBalance.UnconfirmedBalanceNQT);
        }

        [TestMethod]
        public void AddToBalanceNQTShouldAddBalance()
        {
            const int expected = 100;

            _accountBalance.AddToBalanceNQT(expected);

            Assert.AreEqual(expected, _accountBalance.BalanceNQT);
        }

        [TestMethod]
        [ExpectedException(typeof(DoubleSpendingException))]
        public void CheckBalanceShouldNotAcceptNegativeBalance()
        {
            _accountBalance.AddToBalanceNQT(-100);
        }

        [TestMethod]
        [ExpectedException(typeof(DoubleSpendingException))]
        public void CheckBalanceShouldNotAcceptSmallerBalanceThanUnconfirmed()
        {
            _accountBalance.AddToBalanceAndUnconfirmedBalanceNQT(100);
            _accountBalance.AddToBalanceNQT(-50);
        }

        [TestMethod]
        [ExpectedException(typeof(DoubleSpendingException))]
        public void CheckBalanceShouldNotAcceptNegativeUnconfirmedBalanceNQT()
        {
            _accountBalance.AddToBalanceNQT(100);
            _accountBalance.AddToBalanceAndUnconfirmedBalanceNQT(-50);
        }

        [TestMethod]
        public void CheckBalanceShouldAcceptIfGenesisBlock()
        {
            _accountMock.Setup(a => a.Id).Returns(Genesis.CreatorId);

            _accountBalance.AddToBalanceNQT(100);
            _accountBalance.AddToBalanceAndUnconfirmedBalanceNQT(-50);
        }

        [TestMethod]
        public void AddToBalanceAndUnconfirmedBalanceNQTShouldAddBalances()
        {
            const int expected = 100;

            _accountBalance.AddToBalanceAndUnconfirmedBalanceNQT(expected);

            Assert.AreEqual(expected, _accountBalance.BalanceNQT);
            Assert.AreEqual(expected, _accountBalance.UnconfirmedBalanceNQT);
        }

        [TestMethod]
        public void GetGuaranteedBalanceNQTShouldReturnZeroWhenBlockHeightIsSmallerThanNumberOfConfirmations()
        {
            _accountBalance.AddToBalanceNQT(100);

            var actual = _accountBalance.GetGuaranteedBalanceNQT(2);

            Assert.AreEqual(0, actual);
        }

        [TestMethod]
        [ExpectedException(typeof (ArgumentOutOfRangeException))]
        public void GetGuaranteedBalanceNQTShouldNotAcceptTooHighNumberOfConfirmations()
        {
            _lastBlockMock.SetupGet(b => b.Height).Returns(3000);
            _accountBalance.GetGuaranteedBalanceNQT(2900);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void GetGuaranteedBalanceNQTShouldNotAcceptNegativeNumberOfConfirmations()
        {
            _accountBalance.GetGuaranteedBalanceNQT(-1);
        }

        [TestMethod]
        public void GetGuaranteedBalanceNQTShouldReturnZeroWhenNoBalancesExist()
        {
            var actual = _accountBalance.GetGuaranteedBalanceNQT(0);

            Assert.AreEqual(0, actual);
        }

        [TestMethod]
        public void GetGuaranteedBalanceNQTShouldReturnCorrectValues()
        {
            _lastBlockMock.SetupGet(b => b.Height).Returns(2);
            _accountBalance.AddToBalanceNQT(10);
            _lastBlockMock.SetupGet(b => b.Height).Returns(3);
            _accountBalance.AddToBalanceNQT(10);
            _lastBlockMock.SetupGet(b => b.Height).Returns(4);
            _accountBalance.AddToBalanceNQT(10);
            _lastBlockMock.SetupGet(b => b.Height).Returns(5);

            Assert.AreEqual(30, _accountBalance.GetGuaranteedBalanceNQT(0));
            Assert.AreEqual(30, _accountBalance.GetGuaranteedBalanceNQT(1));
            Assert.AreEqual(20, _accountBalance.GetGuaranteedBalanceNQT(2));
            Assert.AreEqual(10, _accountBalance.GetGuaranteedBalanceNQT(3));
            Assert.AreEqual( 0, _accountBalance.GetGuaranteedBalanceNQT(4));
        }

        [TestMethod]
        public void GetEffectiveBalanceNXTShouldReturnZeroWhenPublicKeyIsNotRevealed()
        {
            _lastBlockMock.SetupGet(b => b.Height).Returns(Constants.TransparentForgingBlock6 + 1);
            _accountMock.SetupGet(a => a.PublicKey).Returns((byte[])null);

            var actual = _accountBalance.GetEffectiveBalanceNXT();

            Assert.AreEqual(0, actual);
        }

        [TestMethod]
        public void GetEffectiveBalanceNXTShouldReturnZeroWhenPublicKeyIsNotRevealedByKeyHeight()
        {
            _lastBlockMock.SetupGet(b => b.Height).Returns(Constants.TransparentForgingBlock6 + 1);
            _accountMock.SetupGet(a => a.PublicKey).Returns(new byte[0]);
            _accountMock.SetupGet(a => a.KeyHeight).Returns(-1);

            var actual = _accountBalance.GetEffectiveBalanceNXT();

            Assert.AreEqual(0, actual);
        }

        [TestMethod]
        public void GetEffectiveBalanceNXTShouldReturnZeroWhenPublicKeyIsRevealedLessThan1440BlocksAgo()
        {
            _lastBlockMock.SetupGet(b => b.Height).Returns(Constants.TransparentForgingBlock6 + 1);
            _accountMock.SetupGet(a => a.PublicKey).Returns(new byte[0]);
            _accountMock.SetupGet(a => a.KeyHeight).Returns(_lastBlockMock.Object.Height - 1000);

            var actual = _accountBalance.GetEffectiveBalanceNXT();

            Assert.AreEqual(0, actual);
        }

        [TestMethod]
        public void GetEffectiveBalanceNXTShouldReturnGuaranteedBalance()
        {
            const long expected = 1000;
            _lastBlockMock.SetupGet(b => b.Height).Returns(Constants.TransparentForgingBlock5 - 2000);
            _accountBalance.AddToBalanceNQT(expected * Constants.OneNxt);
            _lastBlockMock.SetupGet(b => b.Height).Returns(Constants.TransparentForgingBlock5);
            _accountMock.SetupGet(a => a.PublicKey).Returns(new byte[0]);
            _accountMock.SetupGet(a => a.KeyHeight).Returns(_lastBlockMock.Object.Height - 2000);
            _accountMock.SetupGet(a => a.Lease).Returns(new AccountLease(_accountMock.Object));

            var actual = _accountBalance.GetEffectiveBalanceNXT();

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void GetEffectiveBalanceNXTShouldIncludeLeasedBalance()
        {
            const long expected = 1100;
            _lastBlockMock.SetupGet(b => b.Height).Returns(Constants.TransparentForgingBlock5 - 2000);
            _accountBalance.AddToBalanceNQT(1000 * Constants.OneNxt);
            _lastBlockMock.SetupGet(b => b.Height).Returns(Constants.TransparentForgingBlock5);
            _accountMock.SetupGet(a => a.PublicKey).Returns(new byte[0]);
            _accountMock.SetupGet(a => a.KeyHeight).Returns(_lastBlockMock.Object.Height - 2000);
            SetupLease();

            var actual = _accountBalance.GetEffectiveBalanceNXT();

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void GetEffectiveBalanceNXTShouldCountOnlyLeasedBalance()
        {
            const long expected = 100;
            _lastBlockMock.SetupGet(b => b.Height).Returns(Constants.TransparentForgingBlock5 - 2000);
            _accountBalance.AddToBalanceNQT(1000 * Constants.OneNxt);
            _lastBlockMock.SetupGet(b => b.Height).Returns(Constants.TransparentForgingBlock5);
            _accountMock.SetupGet(a => a.PublicKey).Returns(new byte[0]);
            _accountMock.SetupGet(a => a.KeyHeight).Returns(_lastBlockMock.Object.Height - 2000);
            var lease = SetupLease();
            lease.SetupGet(l => l.CurrentLeasingHeightFrom).Returns(Constants.TransparentForgingBlock5 - 2001);

            var actual = _accountBalance.GetEffectiveBalanceNXT();

            Assert.AreEqual(expected, actual);
        }

        private Mock<IAccountLease> SetupLease()
        {
            var lease = new Mock<IAccountLease>();
            lease.SetupGet(l => l.CurrentLeasingHeightFrom).Returns(Int32.MaxValue);
            var lessor = new Mock<IAccount>();
            var lessorBalance = new Mock<IAccountBalance>();
            lessorBalance.Setup(b => b.GetGuaranteedBalanceNQT(1440)).Returns(100*Constants.OneNxt);
            var lessorLeases = new ConcurrentDictionary<IAccount, bool>();
            lessorLeases[lessor.Object] = true;
            lease.SetupGet(l => l.Lessors).Returns(lessorLeases);
            lessor.SetupGet(l => l.Balance).Returns(lessorBalance.Object);
            _accountMock.SetupGet(a => a.Lease).Returns(lease.Object);
            return lease;
        }

        [TestMethod]
        public void GetEffectiveBalanceNXTShouldReturnBalanceBeforeTransparentForgingBlock3()
        {
            const long expected = 1000;
            _lastBlockMock.SetupGet(b => b.Height).Returns(Constants.TransparentForgingBlock3 - 2000);
            _accountBalance.AddToBalanceNQT(expected * Constants.OneNxt);
            _lastBlockMock.SetupGet(b => b.Height).Returns(Constants.TransparentForgingBlock3 - 1);
            _accountMock.SetupGet(a => a.PublicKey).Returns(new byte[0]);
            _accountMock.SetupGet(a => a.KeyHeight).Returns(_lastBlockMock.Object.Height - 2000);
            _accountMock.SetupGet(a => a.Height).Returns(0);

            var actual = _accountBalance.GetEffectiveBalanceNXT();

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void GetEffectiveBalanceNXTShouldReturnZeroWhenLessThan1440HeightDiff()
        {
            _lastBlockMock.SetupGet(b => b.Height).Returns(Constants.TransparentForgingBlock2 - 2000);
            _accountBalance.AddToBalanceNQT(1000 * Constants.OneNxt);
            _lastBlockMock.SetupGet(b => b.Height).Returns(Constants.TransparentForgingBlock2 - 1);
            _accountMock.SetupGet(a => a.PublicKey).Returns(new byte[0]);
            _accountMock.SetupGet(a => a.KeyHeight).Returns(_lastBlockMock.Object.Height - 2000);
            _accountMock.SetupGet(a => a.Height).Returns(Constants.TransparentForgingBlock2 - 1000);

            var actual = _accountBalance.GetEffectiveBalanceNXT();

            Assert.AreEqual(0, actual);
        }

        [TestMethod]
        public void GetEffectiveBalanceNXTShouldSubtractRecievedInLastBlock()
        {
            _lastBlockMock.SetupGet(b => b.Height).Returns(Constants.TransparentForgingBlock2 - 2000);
            _accountBalance.AddToBalanceNQT(1000 * Constants.OneNxt);
            _lastBlockMock.SetupGet(b => b.Height).Returns(Constants.TransparentForgingBlock2 - 1);
            _accountMock.SetupGet(a => a.PublicKey).Returns(new byte[0]);
            _accountMock.SetupGet(a => a.KeyHeight).Returns(_lastBlockMock.Object.Height - 2000);
            _accountMock.SetupGet(a => a.Height).Returns(Constants.TransparentForgingBlock2 - 2000);
            var transactionMock = new Mock<ITransaction>();
            const int accountId = 7;
            transactionMock.SetupGet(t => t.RecipientId).Returns(accountId);
            _accountMock.SetupGet(a => a.Id).Returns(accountId);
            transactionMock.SetupGet(t => t.AmountNQT).Returns(100*Constants.OneNxt);
            _lastBlockMock.SetupGet(b => b.Transactions).Returns(new List<ITransaction> {transactionMock.Object});

            var actual = _accountBalance.GetEffectiveBalanceNXT();

            Assert.AreEqual(900, actual);
        }
    }
}
