using Nxt.NET.Crypto;

namespace Nxt.NET.Transaction
{
    public interface ITransactionSignatureVerifyer
    {
        bool VerifyTransaction(ITransaction transaction);
    }

    public class TransactionSignatureVerifyer : ITransactionSignatureVerifyer
    {
        private readonly IAccountContainer _accountContainer;
        private readonly ICryptoFactory _cryptoFactory;

        public TransactionSignatureVerifyer(IAccountContainer accountContainer,
            ICryptoFactory cryptoFactory)
        {
            _accountContainer = accountContainer;
            _cryptoFactory = cryptoFactory;
        }

        public bool VerifyTransaction(ITransaction transaction)
        {
            var account = _accountContainer.GetAccount(transaction.SenderId);
            if (account == null || transaction.Signature == null)
            {
                return false;
            }
            var data = transaction.GetBytes(true);

            var crypto = _cryptoFactory.Create();
            var enforceCanonical = transaction.UseNQT() &&
                                   account.SetAndVerifyPublicKey(transaction.SenderPublicKey, transaction.Height);
            var verified = crypto.Verify(transaction.Signature, data, transaction.SenderPublicKey, enforceCanonical);
            return verified;
        }
    }
}
