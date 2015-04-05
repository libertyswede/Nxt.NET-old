using System;
using System.Collections.Generic;
using System.Linq;

namespace Nxt.NET
{
    public interface IAccountBalance
    {
        long ForgedBalanceNQT { get; }
        long BalanceNQT { get; }
        long UnconfirmedBalanceNQT { get; }
        void AddToBalanceNQT(long amountNQT);
        void AddToUnconfirmedBalanceNQT(long amountNQT);
        void AddToBalanceAndUnconfirmedBalanceNQT(long amountNQT);
        void AddToForgedBalanceNQT(long amountNqt);
        long GetGuaranteedBalanceNQT(int numberOfConfirmations);
        long GetEffectiveBalanceNXT();
    }

    public class AccountBalance : IAccountBalance
    {
        private readonly IAccount _account;
        private readonly IBlockRepository _blockRepository;
        private readonly IConfiguration _configuration;
        public long ForgedBalanceNQT { get; private set; }
        public long BalanceNQT { get; private set; }
        public long UnconfirmedBalanceNQT { get; private set; }
        

        private readonly List<GuaranteedBalance> _guaranteedBalances = new List<GuaranteedBalance>();
        private const int MaxTrackedBalanceConfirmations = 2881;

        public AccountBalance(IAccount account, IBlockRepository blockRepository, IConfiguration configuration)
        {
            _account = account;
            _blockRepository = blockRepository;
            _configuration = configuration;
        }

        public void AddToBalanceNQT(long amountNQT)
        {
            BalanceNQT += amountNQT;
            AddToGuaranteedBalanceNQT(amountNQT);
            CheckBalance();
        }

        public void AddToUnconfirmedBalanceNQT(long amountNQT)
        {
            if (amountNQT == 0)
                return;

            UnconfirmedBalanceNQT += amountNQT;
            CheckBalance();
        }

        public void AddToBalanceAndUnconfirmedBalanceNQT(long amountNQT)
        {
            BalanceNQT += amountNQT;
            UnconfirmedBalanceNQT += amountNQT;
            AddToGuaranteedBalanceNQT(amountNQT);
            CheckBalance();
        }

        public void AddToForgedBalanceNQT(long amountNqt)
        {
            ForgedBalanceNQT += amountNqt;
        }

        public long GetGuaranteedBalanceNQT(int numberOfConfirmations)
        {
            var blockchainHeight = _blockRepository.LastBlock.Height;

            if (numberOfConfirmations >= blockchainHeight)
            {
                return 0;
            }
            if (numberOfConfirmations > MaxTrackedBalanceConfirmations || numberOfConfirmations < 0)
            {
                throw new ArgumentOutOfRangeException("Number of required confirmations must be between 0 and " +
                                                      MaxTrackedBalanceConfirmations);
            }
            if (!_guaranteedBalances.Any())
            {
                return 0;
            }

            _guaranteedBalances.Sort();
            var i = _guaranteedBalances.BinarySearch(new GuaranteedBalance(blockchainHeight - numberOfConfirmations, 0));
            if (i == -1)
            {
                // Did not find any guaranteed balance at specified height or below
                return 0;
            }
            if (i < -1)
            {
                // Did not find any guaranteed balance at specified height, but there are gb's with lower height
                i = -i - 2;
            }
            //if (i > _guaranteedBalances.Count - 1)
            //{
            //    i = _guaranteedBalances.Count - 1;
            //}
            GuaranteedBalance result;
            while ((result = _guaranteedBalances[i]).Ignore && i > 0)
            {
                i--;
            }
            return result.Ignore || result.Balance < 0 ? 0 : result.Balance;
        }

        private void AddToGuaranteedBalanceNQT(long amountNQT)
        {
            var blockchainHeight = _blockRepository.LastBlock.Height;
            GuaranteedBalance last = null;

            _guaranteedBalances.Sort();
            if (_guaranteedBalances.Any() && (last = _guaranteedBalances.Last()).Height > blockchainHeight)
            {
                // this only happens while last block is being popped off
                if (amountNQT > 0)
                {
                    // this is a reversal of a withdrawal or a fee, so previous gb records need to be corrected
                    foreach (var guaranteedBalance in _guaranteedBalances)
                    {
                        guaranteedBalance.Balance += amountNQT;
                    }
                } // deposits don't need to be reversed as they have never been applied to old gb records to begin with
                last.Ignore = true; // set dirty flag
                return;
            }
            var trimTo = 0;

            for (var i = 0; i < _guaranteedBalances.Count; i++)
            {
                var guaranteedBalance = _guaranteedBalances[i];
                if (guaranteedBalance.Height < blockchainHeight - MaxTrackedBalanceConfirmations
                    && i < _guaranteedBalances.Count - 1
                    && _guaranteedBalances[i + 1].Height >= blockchainHeight - MaxTrackedBalanceConfirmations)
                {
                    trimTo = i;
                        // trim old gb records but keep at least one at height lower than the supported MaxTrackedBalanceConfirmations
                    if (blockchainHeight >= Constants.TransparentForgingBlock4 &&
                        blockchainHeight < Constants.TransparentForgingBlock5)
                    {
                        guaranteedBalance.Balance += amountNQT; // because of a bug which leads to a fork
                    }
                    else if (blockchainHeight >= Constants.TransparentForgingBlock5 && amountNQT < 0)
                    {
                        guaranteedBalance.Balance += amountNQT;
                    }
                }
                else if (amountNQT < 0)
                {
                    guaranteedBalance.Balance += amountNQT;
                        // subtract current block withdrawals from all previous gb records
                }
                // ignore deposits when updating previous gb records
            }

            if (trimTo > 0)
            {
                _guaranteedBalances.RemoveRange(0, trimTo);
            }
            if (!_guaranteedBalances.Any() || last.Height < blockchainHeight)
            {
                // this is the first transaction affecting this account in a newly added block
                _guaranteedBalances.Add(new GuaranteedBalance(blockchainHeight, BalanceNQT));
            }
            else if (last.Height == blockchainHeight)
            {
                // following transactions for same account in a newly added block
                // for the current block, guaranteedBalance (0 confirmations) must be same as balance
                last.Balance = BalanceNQT;
                last.Ignore = false;
            }
            else
            {
                // should have been handled in the block popped off case
                throw new NxtException("last guaranteed balance height exceeds blockchain height");
            }
        }

        private void CheckBalance()
        {
            if (_account.Id.Equals(Genesis.CreatorId))
            {
                return;
            }
            if (BalanceNQT < 0)
            {
                throw new DoubleSpendingException("Negative balance for account " + (ulong) _account.Id);
            }
            if (UnconfirmedBalanceNQT < 0)
            {
                throw new DoubleSpendingException("Negative unconfirmed balance for account " + (ulong) _account.Id);
            }
            if (UnconfirmedBalanceNQT > BalanceNQT)
            {
                throw new DoubleSpendingException("Unconfirmed balance exceeds balance for account " +
                                                  (ulong) _account.Id);
            }
        }

        public long GetEffectiveBalanceNXT()
        {
            var lastBlock = _blockRepository.LastBlock;

            if (lastBlock.Height >= Constants.GetTransparentForgingBlock6(_configuration) &&
                (_account.PublicKey == null || _account.KeyHeight == -1 || lastBlock.Height - _account.KeyHeight <= 1440))
            {
                // cfb: Accounts with the public key revealed less than 1440 blocks ago are not allowed to generate blocks
                return 0;
            }

            if (lastBlock.Height < Constants.TransparentForgingBlock3 &&
                _account.Height < Constants.TransparentForgingBlock2)
            {
                if (_account.Height == 0)
                {
                    return BalanceNQT / Constants.OneNxt;
                }
                if (lastBlock.Height - _account.Height < 1440)
                {
                    return 0;
                }

                var receivedInlastBlock = lastBlock.Transactions
                    .Where(transaction => transaction.RecipientId.Equals(_account.Id))
                    .Sum(transaction => transaction.AmountNQT);

                return (BalanceNQT - receivedInlastBlock) / Constants.OneNxt;
            }
            if (lastBlock.Height < _account.Lease.CurrentLeasingHeightFrom)
            {
                return (GetGuaranteedBalanceNQT(1440) + GetLessorsGuaranteedBalanceNQT()) / Constants.OneNxt;
            }

            return GetLessorsGuaranteedBalanceNQT() / Constants.OneNxt;
        }

        private long GetLessorsGuaranteedBalanceNQT()
        {
            return _account.Lease.Lessors.Keys.Sum(lessor => lessor.Balance.GetGuaranteedBalanceNQT(1440));
        }
    }

    public class DoubleSpendingException : Exception
    {
        public DoubleSpendingException(string message) : base(message)
        {
        }
    }
}
