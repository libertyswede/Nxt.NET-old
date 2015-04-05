namespace Nxt.NET.Transaction.Types
{
    public abstract class Messaging : TransactionType
    {
        protected Messaging(IConfiguration configuration) : base(configuration)
        {
        }

        public override byte GetTypeByte()
        {
            return TypeMessaging;
        }

        protected override bool ApplyAttachmentUnconfirmed(ITransaction transaction, IAccount senderAccount)
        {
            return true;
        }

        protected override void UndoAttachmentUnconfirmed(ITransaction transaction, IAccount senderAccount)
        {
        }
    }
}