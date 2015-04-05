using System.Linq;
using Nxt.NET.Crypto;

namespace Nxt.NET.Block
{
    public interface IBlockSignatureVerifyer
    {
        bool VerifyBlockSignature(IBlock block);
    }

    public class BlockSignatureVerifyer : IBlockSignatureVerifyer
    {
        private readonly ICryptoFactory _cryptoFactory;
        private readonly IAccountContainer _accountContainer;

        public BlockSignatureVerifyer(ICryptoFactory cryptoFactory, IAccountContainer accountContainer)
        {
            _cryptoFactory = cryptoFactory;
            _accountContainer = accountContainer;
        }

        public bool VerifyBlockSignature(IBlock block)
        {
            var account = _accountContainer.GetAccount(block.GeneratorId);
            if (account == null)
                return false;

            var blockBytes = block.GetBytes();
            var data = blockBytes.Take(blockBytes.Length - 64).ToArray();
            var crypto = _cryptoFactory.Create();

            return crypto.Verify(block.BlockSignature, data, block.GeneratorPublicKey, block.Version >= 3) &&
                   account.SetAndVerifyPublicKey(block.GeneratorPublicKey, block.Height);
        }
    }
}
