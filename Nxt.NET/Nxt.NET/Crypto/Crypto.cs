using System.Linq;
using System.Security.Cryptography;
using NLog;

namespace Nxt.NET.Crypto
{
    public interface ICrypto
    {
        byte[] Hash { get; }
        int TransformBlock(byte[] value);
        byte[] TransformFinalBlock(byte[] value);
        byte[] ComputeHash(byte[] value);
        string ReedSolomonEncode(long value);
        long ReedSolomonDecode(string value);
        bool Verify(byte[] signature, byte[] message, byte[] publicKey, bool enforceCanonical);
    }

    public class Crypto : ICrypto
    {
        private readonly SHA256 _sha256;
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public Crypto()
        {
            _sha256 = SHA256.Create();
        }

        public byte[] Hash { get { return _sha256.Hash; } }

        public int TransformBlock(byte[] value)
        {
            return _sha256.TransformBlock(value, 0, value.Length, value, 0);
        }

        public byte[] TransformFinalBlock(byte[] value)
        {
            return _sha256.TransformFinalBlock(value, 0, value.Length);
        }

        public byte[] ComputeHash(byte[] value)
        {
            return _sha256.ComputeHash(value);
        }

        public string ReedSolomonEncode(long value)
        {
            return ReedSolomon.Encode(value);
        }

        public long ReedSolomonDecode(string value)
        {
            return ReedSolomon.Decode(value);
        }

        public bool Verify(byte[] signature, byte[] message, byte[] publicKey, bool enforceCanonical)
        {
            if (enforceCanonical && !Curve25519.IsCanonicalSignature(signature))
            {
                Logger.Debug("Rejecting non-canonical signature");
                return false;
            }

            if (enforceCanonical && !Curve25519.IsCanonicalPublicKey(publicKey))
            {
                Logger.Debug("Rejecting non-canonical public key");
                return false;
            }

            var y = new byte[32];
            var v = signature.Take(32).ToArray();
            var h = signature.Skip(32).Take(32).ToArray();
            Curve25519.Verify(y, v, h, publicKey);

            var sha256 = SHA256.Create();
            var messageHash = sha256.ComputeHash(message);
            sha256.TransformBlock(messageHash, 0, messageHash.Length, messageHash, 0);
            sha256.TransformFinalBlock(y, 0, y.Length);

            return h.SequenceEqual(sha256.Hash);
        }
    }
}
