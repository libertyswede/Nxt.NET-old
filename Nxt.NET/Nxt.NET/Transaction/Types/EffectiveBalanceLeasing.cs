using System;
using Newtonsoft.Json.Linq;

namespace Nxt.NET.Transaction.Types
{
    public class EffectiveBalanceLeasing : AccountControl
    {
        private readonly IAccountContainer _accountContainer;

        public EffectiveBalanceLeasing(IConfiguration configuration, IAccountContainer accountContainer) : base(configuration)
        {
            _accountContainer = accountContainer;
        }

        public override byte GetSubtypeByte()
        {
            return SubtypeAccountControlEffectiveBalanceLeasing;
        }

        public override void LoadAttachment(ITransaction transaction, JToken attachmentData)
        {
            var period = (short) attachmentData.SelectToken("period");
            transaction.Attachment = new EffectiveBalanceLeasingAttachment(period);
        }

        public override void LoadAttachment(ITransaction transaction, byte[] attachmentData)
        {
            var period = BitConverter.ToInt16(attachmentData, 0);
            transaction.Attachment = new EffectiveBalanceLeasingAttachment(period);
        }

        public override void ValidateAttachment(ITransaction transaction)
        {
            var attachment = (IEffectiveBalanceLeasingAttachment) transaction.Attachment;
            var recipientAccount = _accountContainer.GetAccount(transaction.RecipientId);

            if (transaction.RecipientId == transaction.SenderId
                || transaction.AmountNQT != 0
                || attachment.Period < 1440
                || recipientAccount == null
                || (recipientAccount.PublicKey == null && transaction.PresentationId != 5081403377391821646))
            {
                throw new NxtValidationException("Invalid effective balance leasing for transaction: " +
                                                 transaction.PresentationId);
            }
        }
        
        protected override void ApplyAttachment(ITransaction transaction, IAccount senderAccount, IAccount recipientAccount)
        {
            var attachment = (IEffectiveBalanceLeasingAttachment)transaction.Attachment;

            asdf
            // TODO: test this shit
            _accountContainer.LeaseEffectiveBalance(senderAccount.Id, recipientAccount.Id, attachment.Period);
        }

        protected override void UndoAttachment(ITransaction transaction, IAccount senderAccount, IAccount recipientAccount)
        {
            throw new UndoNotSupportedException("Reversal of effective balance leasing not supported");
        }
    }
}
