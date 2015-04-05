using System;
using Nxt.NET.Transaction.Types;
using StructureMap;

namespace Nxt.NET.Transaction
{
    public interface ITransactionTypeFactory
    {
        ITransactionType FindTransactionType(byte type, byte subType);
    }

    public class TransactionTypeFactory : ITransactionTypeFactory
    {
        public ITransactionType FindTransactionType(byte type, byte subType)
        {
            switch (type)
            {
                case TransactionType.TypePayment:
                    switch (subType)
                    {
                        case TransactionType.SubtypePaymentOrdinaryPayment:
                            return ObjectFactory.GetInstance<OrdinaryPayment>();
                    }
                    break;
                case TransactionType.TypeMessaging:
                    switch (subType)
                    {
                        case TransactionType.SubtypeMessagingArbitraryMessage:
                            return ObjectFactory.GetInstance<ArbitraryMessage>();

                        case TransactionType.SubtypeMessagingAliasAssignment:
                            return ObjectFactory.GetInstance<AliasAssignment>();
                    }
                    break;
            }
            throw new NotSupportedException(string.Format("Type: {0}, Subtype: {1} is not supported transactiontype!", type, subType));
        }
    }
}
