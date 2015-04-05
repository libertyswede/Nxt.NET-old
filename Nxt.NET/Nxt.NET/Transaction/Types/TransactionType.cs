using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace Nxt.NET.Transaction.Types
{
    public interface ITransactionType : IEquatable<ITransactionType>
    {
        byte GetTypeByte();
        byte GetSubtypeByte();
        void LoadAttachment(ITransaction transaction, JToken attachmentData);
        void LoadAttachment(ITransaction transaction, byte[] attachmentData);
        void ValidateAttachment(ITransaction transaction);
        bool ApplyUnconfirmed(ITransaction transaction, IAccount senderAccount);
        void Apply(ITransaction transaction, IAccount senderAccount, IAccount recipientAccount);
        void UndoUnconfirmed(ITransaction transaction, IAccount senderAccount);
        void Undo(ITransaction transaction, IAccount senderAccount, IAccount recipientAccount);
        bool IsDuplicate(ITransaction transaction, IDictionary<ITransactionType, HashSet<string>> duplicates);
    }

    public abstract class TransactionType : ITransactionType
    {
        protected readonly IConfiguration Configuration;
        public const byte TypePayment = 0;
        public const byte TypeMessaging = 1;
        public const byte TypeAccountControl = 4;

        public const byte SubtypePaymentOrdinaryPayment = 0;

        public const byte SubtypeMessagingArbitraryMessage = 0;
        public const byte SubtypeMessagingAliasAssignment = 1;

        public const byte SubtypeAccountControlEffectiveBalanceLeasing = 0;

        protected TransactionType(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public abstract byte GetTypeByte();
        public abstract byte GetSubtypeByte();
        public abstract void LoadAttachment(ITransaction transaction, JToken attachmentData);
        public abstract void LoadAttachment(ITransaction transaction, byte[] attachmentData);
        public abstract void ValidateAttachment(ITransaction transaction);

        public bool ApplyUnconfirmed(ITransaction transaction, IAccount senderAccount)
        {
            var totalAmountNQT = transaction.AmountNQT + transaction.FeeNQT;
            if (UseUnconfirmedPoolDeposit(transaction))
            {
                totalAmountNQT += Constants.GetUnconfirmedPoolDepositNQT(Configuration);
            }
            if (senderAccount.Balance.UnconfirmedBalanceNQT < totalAmountNQT && !transaction.IsGenesisTransaction())
            {
                return false;
            }
            senderAccount.Balance.AddToUnconfirmedBalanceNQT(- totalAmountNQT);
            if (!ApplyAttachmentUnconfirmed(transaction, senderAccount))
            {
                senderAccount.Balance.AddToUnconfirmedBalanceNQT(totalAmountNQT);
                return false;
            }
            return true;
        }

        protected abstract bool ApplyAttachmentUnconfirmed(ITransaction transaction, IAccount senderAccount);

        public void Apply(ITransaction transaction, IAccount senderAccount, IAccount recipientAccount)
        {
            senderAccount.Balance.AddToBalanceNQT(- (transaction.AmountNQT + transaction.FeeNQT));
            if (UseUnconfirmedPoolDeposit(transaction))
            {
                senderAccount.Balance.AddToUnconfirmedBalanceNQT(Constants.GetUnconfirmedPoolDepositNQT(Configuration));
            }
            ApplyAttachment(transaction, senderAccount, recipientAccount);
        }

        protected abstract void ApplyAttachment(ITransaction transaction, IAccount senderAccount, IAccount recipientAccount);

        public void UndoUnconfirmed(ITransaction transaction, IAccount senderAccount)
        {
            senderAccount.Balance.AddToUnconfirmedBalanceNQT(transaction.AmountNQT + transaction.FeeNQT);
            if (UseUnconfirmedPoolDeposit(transaction))
            {
                senderAccount.Balance.AddToUnconfirmedBalanceNQT(Constants.GetUnconfirmedPoolDepositNQT(Configuration));
            }
            UndoAttachmentUnconfirmed(transaction, senderAccount);
        }

        protected abstract void UndoAttachmentUnconfirmed(ITransaction transaction, IAccount senderAccount);

        public void Undo(ITransaction transaction, IAccount senderAccount, IAccount recipientAccount)
        {
            senderAccount.Balance.AddToBalanceNQT(transaction.AmountNQT + transaction.FeeNQT);
            if (!UseUnconfirmedPoolDeposit(transaction))
            {
                senderAccount.Balance.AddToUnconfirmedBalanceNQT(-Constants.GetUnconfirmedPoolDepositNQT(Configuration));
            }
            UndoAttachment(transaction, senderAccount, recipientAccount);
        }

        protected abstract void UndoAttachment(ITransaction transaction, IAccount senderAccount, IAccount recipientAccount);

        public virtual bool IsDuplicate(ITransaction transaction, IDictionary<ITransactionType, HashSet<string>> duplicates)
        {
            return false;
        }

        public bool Equals(ITransactionType other)
        {
            return other.GetTypeByte() == GetTypeByte() && other.GetSubtypeByte() == GetSubtypeByte();
        }

        private bool UseUnconfirmedPoolDeposit(ITransaction transaction)
        {
            return transaction.ReferencedTransactionFullHash != null
                   && transaction.Timestamp > Constants.GetReferencedTransactionFullHashBlockTimestamp(Configuration);
        }
    }
}
