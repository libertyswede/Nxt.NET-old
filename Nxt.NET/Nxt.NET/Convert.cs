using System;
using System.Linq;
using System.Numerics;

namespace Nxt.NET
{
    public interface IConvert
    {
        int GetEpochTime();
        long FullHashToId(byte[] fullHash);
        string ToHexString(byte[] hash);
    }

    public class Convert : IConvert
    {
        private static readonly DateTime Jan1St1970 = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        public int GetEpochTime()
        {
            return (int)(((DateTime.UtcNow - Jan1St1970).TotalMilliseconds - Constants.EpochBeginning + 500) / 1000);
        }

        public long FullHashToId(byte[] fullHash)
        {
            if (fullHash == null || fullHash.Length < 8)
            {
                throw new ArgumentException("Invalid hash: " + fullHash);
            }
            var bigInteger = new BigInteger(fullHash.Take(8).ToArray());
            return (long) bigInteger;
        }

        public string ToHexString(byte[] hash)
        {
            var chars = new char[hash.Length / sizeof(char)];
            Buffer.BlockCopy(hash, 0, chars, 0, hash.Length);
            var stringHash = new string(chars);
            return stringHash;
        }
    }
}
