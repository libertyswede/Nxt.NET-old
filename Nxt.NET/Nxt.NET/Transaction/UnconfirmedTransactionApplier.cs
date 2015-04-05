using System;
using System.Collections.Generic;

namespace Nxt.NET.Transaction
{
    public interface IUnconfirmedTransactionApplier : IDisposable
    {
        bool ApplyUnconfirmedTransaction(ITransaction transaction);
        void Commit();
    }

    public class UnconfirmedTransactionApplier : IUnconfirmedTransactionApplier
    {
        private readonly List<ITransaction> _appliedUnconfirmed = new List<ITransaction>();

        public bool ApplyUnconfirmedTransaction(ITransaction transaction)
        {
            var applied = transaction.ApplyUnconfirmed();
            if (applied)
                _appliedUnconfirmed.Add(transaction);
            return applied;
        }

        public void Commit()
        {
            _appliedUnconfirmed.Clear();
        }

        public void Dispose()
        {
            foreach (var transaction in _appliedUnconfirmed)
            {
                transaction.UndoUnconfirmed();
            }
        }
    }
}
