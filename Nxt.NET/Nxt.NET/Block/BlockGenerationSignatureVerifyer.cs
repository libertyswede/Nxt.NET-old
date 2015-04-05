using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Nxt.NET.Crypto;

namespace Nxt.NET.Block
{
    public interface IBlockGenerationSignatureVerifyer
    {
        bool VerifyGenerationSignature(IBlock block, IBlock previousBlock);
    }

    public class BlockGenerationSignatureVerifyer : IBlockGenerationSignatureVerifyer
    {
        private readonly ICryptoFactory _cryptoFactory;
        private readonly IAccountContainer _accountContainer;

        public BlockGenerationSignatureVerifyer(ICryptoFactory cryptoFactory, IAccountContainer accountContainer)
        {
            _cryptoFactory = cryptoFactory;
            _accountContainer = accountContainer;
        }

        public bool VerifyGenerationSignature(IBlock block, IBlock previousBlock)
        {
            var crypto = _cryptoFactory.Create();

            if (VerifySignatureCrypto(block, previousBlock, crypto))
                return false;

            var effectiveBalance = GetEffectiveBalance(block);
            if (effectiveBalance <= 0)
                return false;

            var elapsedTime = block.Timestamp - previousBlock.Timestamp;
            var target = CalculateTarget(previousBlock, effectiveBalance, elapsedTime);

            var generationSignatureHash = GetGenerationSignatureHash(block, previousBlock);
            if (VerifyGenerationSignatureHash(block, generationSignatureHash))
                return false;

            var hit = new BigInteger(generationSignatureHash.Take(8).ToArray());
            return hit.CompareTo(target) < 0;
        }

        private static bool VerifySignatureCrypto(IBlock block, IBlock previousBlock, ICrypto crypto)
        {
            return block.Version == 1 &&
                   !crypto.Verify(block.GenerationSignature, previousBlock.GenerationSignature, block.GeneratorPublicKey, false);
        }

        private long GetEffectiveBalance(IBlock block)
        {
            var account = _accountContainer.GetAccount(block.GeneratorId);
            var effectiveBalance = (account == null ? 0 : account.Balance.GetEffectiveBalanceNXT());
            return effectiveBalance;
        }

        private static BigInteger CalculateTarget(IBlock previousBlock, long effectiveBalance, int elapsedTime)
        {
            return BigInteger.Multiply(BigInteger.Multiply(previousBlock.BaseTarget, effectiveBalance), elapsedTime);
        }

        private byte[] GetGenerationSignatureHash(IBlock block, IBlock previousBlock)
        {
            var crypto = _cryptoFactory.Create();

            if (block.Version == 1)
            {
                return crypto.ComputeHash(block.GenerationSignature);
            }
            crypto.TransformBlock(previousBlock.GenerationSignature);
            crypto.TransformFinalBlock(block.GeneratorPublicKey);
            return crypto.Hash;
        }

        private static bool VerifyGenerationSignatureHash(IBlock block, IEnumerable<byte> generationSignatureHash)
        {
            return block.Version > 1 && !block.GenerationSignature.SequenceEqual(generationSignatureHash);
        }
    }
}