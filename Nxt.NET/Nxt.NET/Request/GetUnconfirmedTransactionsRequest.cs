using Nxt.NET.Response;
using Nxt.NET.Transaction;
using StructureMap;

namespace Nxt.NET.Request
{
    public class GetUnconfirmedTransactionsRequest : BaseRequest
    {

        public GetUnconfirmedTransactionsRequest()
            : base("getUnconfirmedTransactions")
        {
        }

        public override object ParseResponse(string json)
        {
            var transactionParser = ObjectFactory.GetInstance<ITransactionParser>();
            return new GetUnconfirmedTransactionsResponse(transactionParser, json);
        }
    }
}
