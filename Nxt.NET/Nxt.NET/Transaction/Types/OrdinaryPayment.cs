using Newtonsoft.Json.Linq;

namespace Nxt.NET.Transaction.Types
{
    public sealed class OrdinaryPayment : Payment
    {
        public OrdinaryPayment(IConfiguration configuration) : base(configuration)
        {
        }

        public override byte GetSubtypeByte()
        {
            return SubtypePaymentOrdinaryPayment;
        }

        public override void LoadAttachment(ITransaction transaction, JToken attachmentData)
        {
        }

        public override void ValidateAttachment(ITransaction transaction)
        {
            if (transaction.AmountNQT <= 0 || transaction.AmountNQT >= Constants.MaxBalanceNqt)
            {
                throw new NxtValidationException("Invalid ordinary payment");
            }
        }

        protected override bool ApplyAttachmentUnconfirmed(ITransaction transaction, IAccount senderAccount)
        {
            return true;
        }

        protected override void ApplyAttachment(ITransaction transaction, IAccount senderAccount, IAccount recipientAccount)
        {
            recipientAccount.Balance.AddToBalanceAndUnconfirmedBalanceNQT(transaction.AmountNQT);
        }

        protected override void UndoAttachmentUnconfirmed(ITransaction transaction, IAccount senderAccount)
        {
        }

        protected override void UndoAttachment(ITransaction transaction, IAccount senderAccount, IAccount recipientAccount)
        {
            recipientAccount.Balance.AddToBalanceAndUnconfirmedBalanceNQT(-transaction.AmountNQT);
        }
    }
}