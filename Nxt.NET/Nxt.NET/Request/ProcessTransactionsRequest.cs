using System;
using System.Collections.Generic;
using Nxt.NET.Transaction;

namespace Nxt.NET.Request
{
    public class ProcessTransactionsRequest : BaseRequest
    {
        private readonly IList<ITransaction> _transactions;

        public ProcessTransactionsRequest(IList<ITransaction> transactions)
            : base("processTransactions")
        {
            _transactions = transactions;
            throw new NotImplementedException();
        }

        public override object ParseResponse(string json)
        {
            throw new NotImplementedException();
        }
    }
}
