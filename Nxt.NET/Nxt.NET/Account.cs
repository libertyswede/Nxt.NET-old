using System;
using System.Linq;
using NLog;
using StructureMap;

namespace Nxt.NET
{
    public interface IAccount
    {
        long Id { get; }
        int Height { get; }
        IAccountBalance Balance { get; }
        IAccountLease Lease { get; }
        byte[] PublicKey { get; }
        int KeyHeight { get; }
        void ApplyPublicKey(byte[] key, int height);
        bool SetAndVerifyPublicKey(byte[] key, int height);
    }

    public class Account : IAccount
    {
        public long Id { get; private set; }
        public int Height { get; private set; }
        public IAccountBalance Balance { get; private set; }
        public IAccountLease Lease { get; private set; }
        public byte[] PublicKey { get { return KeyHeight == -1 ? null : _publicKey; } }
        public int KeyHeight { get; private set; }

        private byte[] _publicKey;
        
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public Account(long id)
        {
            Id = id;
            Lease = new AccountLease(this);
            SetBalance();
        }

        private void SetBalance()
        {
            var blockRepository = ObjectFactory.GetInstance<IBlockRepository>();
            var config = ObjectFactory.GetInstance<IConfiguration>();
            Balance = new AccountBalance(this, blockRepository, config);
        }

        public void ApplyPublicKey(byte[] key, int height)
        {
            if (!SetAndVerifyPublicKey(key, Height))
                throw new ArgumentException("Generator public key mismatch");
            if (_publicKey == null)
                throw new ArgumentException("Public key has not been set for account " + (ulong) Id + " at height " +
                                            height + ", key height is " + KeyHeight);
            if (KeyHeight == -1 || KeyHeight > height)
            {
                KeyHeight = height;
            }
        }

        public bool SetAndVerifyPublicKey(byte[] key, int height)
        {
            if (_publicKey == null)
            {
                _publicKey = key;
                KeyHeight = -1;
                return true;
            }
            if (_publicKey.SequenceEqual(key))
                return true;
            if (KeyHeight == -1)
            {
                Logger.Warn("DUPLICATE KEY!!!");
                Logger.Warn("Account key for {0} was already set to a different one at the same height, current height is {1}, rejecting new key", (ulong)Id, height);
                return false;
            }
            if (KeyHeight >= height)
            {
                Logger.Warn("DUPLICATE KEY!!!");
                Logger.Warn("Changing key for account {0} at height {1}, was previously set to a different one at height {2}", (ulong)Id, height, KeyHeight);
                _publicKey = key;
                KeyHeight = height;
                return true;
            }
            Logger.Warn("DUPLICATE KEY!!!");
            Logger.Warn("Invalid key for account {0} at height {1}, was already set to a different one at height {2}", (ulong)Id, height,KeyHeight);
            return false;
        }
    }
}