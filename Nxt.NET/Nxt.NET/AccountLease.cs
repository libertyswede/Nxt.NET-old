using System;
using System.Collections.Concurrent;

namespace Nxt.NET
{
    public interface IAccountLease
    {
        int CurrentLeasingHeightFrom { get; }
        int CurrentLeasingHeightTo { get; }
        long? CurrentLesseeId { get; }
        int NextLeasingHeightFrom { get; }
        int NextLeasingHeightTo { get; }
        long? NextLesseeId { get; }
        ConcurrentDictionary<IAccount, bool> Lessors { get; }
        void LeaseEffectiveBalance(int blockHeight, short period, long lesseeId);
        void StartCurrentLease(IAccount lessee);
        void StopCurrentLease(int blockHeight, IAccount lessee);
    }

    public class AccountLease : IAccountLease
    {
        private readonly IAccount _account;
        public int CurrentLeasingHeightFrom { get; private set; }
        public int CurrentLeasingHeightTo { get; private set; }
        public long? CurrentLesseeId { get; private set; }
        public int NextLeasingHeightFrom { get; private set; }
        public int NextLeasingHeightTo { get; private set; }
        public long? NextLesseeId { get; private set; }
        public ConcurrentDictionary<IAccount, bool> Lessors { get; private set; }

        public AccountLease(IAccount account)
        {
            _account = account;
            Lessors = new ConcurrentDictionary<IAccount, bool>();
            CurrentLeasingHeightFrom = Int32.MaxValue;
        }

        public void LeaseEffectiveBalance(int blockHeight, short period, long lesseeId)
        {
            if (CurrentLeasingHeightFrom == Int32.MaxValue)
            {
                CurrentLeasingHeightFrom = blockHeight;
                CurrentLeasingHeightTo = CurrentLeasingHeightFrom + period;
                CurrentLesseeId = lesseeId;
                NextLeasingHeightFrom = Int32.MaxValue;
            }
            else
            {
                NextLeasingHeightFrom = blockHeight;
                if (NextLeasingHeightFrom < CurrentLeasingHeightTo)
                {
                    NextLeasingHeightFrom = CurrentLeasingHeightTo;
                }
                NextLeasingHeightTo = NextLeasingHeightFrom + period;
                NextLesseeId = lesseeId;
            }
        }

        public void StartCurrentLease(IAccount lessee)
        {
            lessee.Lease.Lessors[_account] = true;
        }

        public void StopCurrentLease(int blockHeight, IAccount lessee)
        {
            lessee.Lease.Lessors.Remove(_account);

            if (NextLeasingHeightFrom == Int32.MaxValue)
            {
                CurrentLeasingHeightFrom = Int32.MaxValue;
                CurrentLesseeId = null;
            }
            else
            {
                CurrentLeasingHeightTo = NextLeasingHeightTo;
                CurrentLesseeId = NextLesseeId;
                NextLeasingHeightFrom = Int32.MaxValue;
                NextLesseeId = null;
                if (CurrentLeasingHeightFrom == blockHeight)
                {
                    lessee.Lease.StartCurrentLease(_account);
                }
            }
        }
    }
}
