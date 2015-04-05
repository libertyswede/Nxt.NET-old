namespace Nxt.NET.Transaction.Types
{
    public abstract class AccountControl : TransactionType
    {
        protected AccountControl(IConfiguration configuration) : base(configuration)
        {
        }

        public override byte GetTypeByte()
        {
            return TypeAccountControl;
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
