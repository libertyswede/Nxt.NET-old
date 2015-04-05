using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json.Linq;

namespace Nxt.NET.Transaction.Types
{
    public sealed class AliasAssignment : Messaging
    {
        private readonly IAliasContainer _aliasContainer;

        public AliasAssignment(IConfiguration configuration, IAliasContainer aliasContainer) : base(configuration)
        {
            _aliasContainer = aliasContainer;
        }

        public override byte GetSubtypeByte()
        {
            return SubtypeMessagingAliasAssignment;
        }

        public override void LoadAttachment(ITransaction transaction, JToken attachmentData)
        {
            var alias = (string)attachmentData.SelectToken("alias");
            var uri = (string)attachmentData.SelectToken("uri");
            transaction.Attachment = new AliasAssignmentAttachment(alias, uri);
        }

        public override void LoadAttachment(ITransaction transaction, byte[] attachmentData)
        {
            var aliasLength = (sbyte) attachmentData[0];
            if (aliasLength > Constants.MaxAliasLength)
                throw new NxtValidationException("Max alias length exceeded");
            var alias = Encoding.UTF8.GetString(attachmentData, 1, aliasLength);
            var uriLength = BitConverter.ToUInt16(attachmentData.Skip(1 + aliasLength).Take(2).ToArray(), 0);
            if (uriLength > Constants.MaxAliasUriLength)
                throw new NxtValidationException("Max alias URI length exceeded");
            var uri = Encoding.UTF8.GetString(attachmentData, 1 + aliasLength + 2, uriLength);
            transaction.Attachment = new AliasAssignmentAttachment(alias, uri);
        }

        protected override void ApplyAttachment(ITransaction transaction, IAccount senderAccount, IAccount recipientAccount)
        {
            var attachment = (IAliasAssignmentAttachment) transaction.Attachment;
            _aliasContainer.AddOrUpdate(senderAccount, transaction, attachment);
        }

        protected override void UndoAttachment(ITransaction transaction, IAccount senderAccount, IAccount recipientAccount)
        {
            var attachment = (IAliasAssignmentAttachment)transaction.Attachment;
            var alias = _aliasContainer.GetAlias(attachment.AliasName.ToLower());
            if (alias.Id == transaction.Id)
            {
                _aliasContainer.Remove(alias);
            }
            else
            {
                // alias has been updated, can't tell what was its previous uri
                throw new UndoNotSupportedException("Reversal of alias assignment not supported");
            }
        }

        public override bool IsDuplicate(ITransaction transaction, IDictionary<ITransactionType, HashSet<string>> duplicates)
        {
            HashSet<string> myDuplicates;
            if (!duplicates.TryGetValue(this, out myDuplicates))
            {
                myDuplicates = new HashSet<string>();
                duplicates[this] = myDuplicates;
            }
            var attachment = (IAliasAssignmentAttachment)transaction.Attachment;
            return ! myDuplicates.Add(attachment.AliasName.ToLower());
        }

        public override void ValidateAttachment(ITransaction transaction)
        {
            var attachment = (IAliasAssignmentAttachment) transaction.Attachment;
            if (transaction.RecipientId != Genesis.CreatorId || transaction.AmountNQT != 0
                || attachment.AliasName.Length == 0
                || attachment.AliasName.Length > Constants.MaxAliasLength
                || attachment.AliasUri.Length > Constants.MaxAliasUriLength)
            {
                throw new NxtValidationException("Invalid alias assignment: " + attachment);
            }
            var normalizedAlias = attachment.AliasName.ToLower();
            if (normalizedAlias.Any(aliasChar => Constants.AliasAlphabet.IndexOf(aliasChar) < 0))
            {
                throw new NxtValidationException("Invalid alias name: " + normalizedAlias);
            }
            var alias = _aliasContainer.GetAlias(normalizedAlias);
            if (alias != null && !alias.Account.PublicKey.SequenceEqual(transaction.SenderPublicKey))
            {
                throw new NxtValidationException("Alias already owned by another account: " + normalizedAlias);
            }
        }
    }
}