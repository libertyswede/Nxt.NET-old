using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using Nxt.NET.Block;
using Nxt.NET.Crypto;

namespace Nxt.NET
{
    public interface IAccountContainer
    {
        IAccount GetAccount(long id);
        IReadOnlyCollection<IAccount> GetAllAccounts();
        IAccount GetOrAddAccount(long id);
        void LeaseEffectiveBalance(long lessorId, long lesseeId, short period);
        void Clear();
    }

    public class AccountContainer : IAccountContainer
    {
        private readonly ICryptoFactory _cryptoFactory;
        private readonly IBlockRepository _blockRepository;
        private readonly ConcurrentDictionary<long, IAccount> _accounts = new ConcurrentDictionary<long, IAccount>();
        private readonly ConcurrentDictionary<long, IAccount> _leasingAccounts = new ConcurrentDictionary<long, IAccount>();

        public AccountContainer(ICryptoFactory cryptoFactory, IBlockRepository blockRepository, IBlockchainProcessor blockchainProcessor)
        {
            _cryptoFactory = cryptoFactory;
            _blockRepository = blockRepository;
            blockchainProcessor.AfterApplyBlock += BlockchainProcessorOnAfterApplyBlock;
        }

        private void BlockchainProcessorOnAfterApplyBlock(IBlockchainProcessor blockchainProcessor, ApplyBlockEventArgs eventArgs)
        {
            var block = eventArgs.Block;
            var height = block.Height;
            foreach (var account in _leasingAccounts.Values.ToList())
            {
                if (height == account.Lease.CurrentLeasingHeightFrom)
                {
                    Debug.Assert(account.Lease.CurrentLesseeId != null, "account.Lease.CurrentLesseeId != null");
                    var lessee = GetAccount(account.Lease.CurrentLesseeId.Value);
                    account.Lease.StartCurrentLease(lessee);
                }
                else if (height == account.Lease.CurrentLeasingHeightTo)
                {
                    Debug.Assert(account.Lease.CurrentLesseeId != null, "account.Lease.CurrentLesseeId != null");
                    var lessee = GetAccount(account.Lease.CurrentLesseeId.Value);
                    account.Lease.StopCurrentLease(height, lessee);
                }
                else if (height == account.Lease.CurrentLeasingHeightTo + 1440)
                {
                    //keep expired leases for up to 1440 blocks to be able to handle block pop-off
                    _leasingAccounts.Remove(account.Id);
                }
            }
        }

        public IAccount GetAccount(long id)
        {
            IAccount account;
            _accounts.TryGetValue(id, out account);
            return account;
        }

        public IReadOnlyCollection<IAccount> GetAllAccounts()
        {
            return new ReadOnlyCollection<IAccount>(_accounts.Values.ToList());
        }

        public IAccount GetOrAddAccount(long id)
        {
            IAccount account;
            if (!_accounts.TryGetValue(id, out account))
            {
                CheckReedSolomon(id);
                account = _accounts.GetOrAdd(id, new Account(id));
            }
            return account;
        }

        // lessor = sender (the one leasing), Lessee = recipient (the one forging)
        public void LeaseEffectiveBalance(long lessorId, long lesseeId, short period)
        {
            var lessor = GetAccount(lessorId);
            var lessee = GetAccount(lesseeId);
            if (lessee != null && lessee.PublicKey != null && lessor != null)
            {
                var lastBlock = _blockRepository.LastBlock;
                _leasingAccounts.AddOrUpdate(lessor.Id, lessor, (l, account) => lessor);
                lessor.Lease.LeaseEffectiveBalance(lastBlock.Height + 1440, period, lesseeId);
            }
        }

        private void CheckReedSolomon(long value)
        {
            var crypto = _cryptoFactory.Create();
            if (value != crypto.ReedSolomonDecode(crypto.ReedSolomonEncode(value)))
                throw new NxtException("CRITICAL ERROR: Reed-Solomon encoding fails for " + value);
        }

        public void Clear()
        {
            _accounts.Clear();
            _leasingAccounts.Clear();
        }
    }
}
