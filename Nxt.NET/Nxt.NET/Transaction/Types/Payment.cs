namespace Nxt.NET.Transaction.Types
{
    public abstract class Payment : TransactionType
    {
        protected Payment(IConfiguration configuration) : base(configuration)
        {
        }

        public override void LoadAttachment(ITransaction transaction, byte[] attachmentData)
        {
        }

        public override byte GetTypeByte()
        {
            return TypePayment;
        }
    }
}