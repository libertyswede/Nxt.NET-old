using System;
using System.Collections.Generic;

namespace Nxt.NET
{
    public class Constants
    {
        internal const string ApplicationName = "NRS.NET";
        internal const string ApplicationVersion = "1.1.5";
        internal const string ApplicationPlatform = "Pc";

        public const long MaxBalanceNxt = 1000000000;
        public const long OneNxt = 100000000;
        public const long MaxBalanceNqt = MaxBalanceNxt*OneNxt;
        public const int MaxNumberOfTransactions = 255;
        public const int MaxPayloadLength = MaxNumberOfTransactions*160;
        public const long InitialBaseTarget = 153722867;
        public const long MaxBaseTarget = MaxBalanceNxt * InitialBaseTarget;
        
        public const int MaxAliasLength = 100;
        public const int MaxAliasUriLength = 1000;
        public const string AliasAlphabet = "0123456789abcdefghijklmnopqrstuvwxyz";

        public const int TransparentForgingBlock = 30000;
        public const int TransparentForgingBlock2 = 47000;
        public const int TransparentForgingBlock3 = 51000;
        public const int TransparentForgingBlock4 = 64000;
        public const int TransparentForgingBlock5 = 67000;
        public const int TransparentForgingBlock6 = 130000;
        public const int NQTBlock = 132000;
        public const int ReferencedTransactionFullHashBlock = 140000;
        public const int TransparentForgingBlock7 = Int32.MaxValue;
        public const int ReferencedTransactionFullHashBlockTimestamp = 15134204;

        public const long UnconfirmedPoolDepositNQT = 100*OneNxt;
        public const long UnconfirmedPoolDepositNQTTestnet = 50*OneNxt;

        public const int NQTBlockTestnet = 76500;
        public const int TransparentForgingBlock6Testnet = 75000;
        public const int TransparentForgingBlock7Testnet = 75000;
        public const int ReferencedTransactionFullHashBlockTestnet = 78000;
        public const int ReferencedTransactionFullHashBlockTimestampTestnet = 13031352;

        public const int MaxAccountNameLength = 100;
        public const int MaxAccountDescriptionLength = 1000;
        public const int MaxArbitraryMessageLength = 1000;

        public const long EpochBeginning = 1385294400000; // milliseconds between 1970-01-01 00:00:00 and 2013-11-24 12:00:00

        public static readonly byte[] ChecksumTransparentForging =
        {
            27, 202, 197, 158, 49, 214, 48, 188, 144, 49, 41,
            94, 215, 78, 172, 27, 169, 234, 228, 36, 222, 166, 112, 206, 247, 5, 89, 221, 80, 135, 128, 112
        };
        public static readonly byte[] ChecksumNqtBlock =
        {
            131, 17, 63, 236, 90, 158, 52, 114, 7, 156, 236, 153, 206, 76, 46, 218, 227, 213, 213, 45, 81, 12, 226,
            100, 189, 206, 144, 241, 22, 199, 84, 150
        };
        public static readonly byte[] ChecksumNqtBlockTestnet =
        {
            130, 139, 162, 240, 125, 162, 38, 10, 11, 37, 223, 4, 186, 248, 216, 176, 18, 235, 202, 130, 109, 183,
            63, 200, 67, 59, 226, 83, 250, 165, 232, 34
        };

        public static int GetNQTBlockHeight(IConfiguration configuration)
        {
            return configuration.IsTestnet ? NQTBlockTestnet : NQTBlock;
        }

        public static int GetTransparentForgingBlock6(IConfiguration configuration)
        {
            return configuration.IsTestnet ? TransparentForgingBlock6Testnet : TransparentForgingBlock6;
        }

        public static int GetTransparentForgingBlock7(IConfiguration configuration)
        {
            return configuration.IsTestnet ? TransparentForgingBlock7Testnet : TransparentForgingBlock7;
        }

        public static int GetReferencedTransactionFullHashBlock(IConfiguration configuration)
        {
            return configuration.IsTestnet
                ? ReferencedTransactionFullHashBlockTestnet
                : ReferencedTransactionFullHashBlock;
        }

        public static IEnumerable<byte> GetNqtBlockChecksum(IConfiguration configuration)
        {
            return configuration.IsTestnet ? ChecksumNqtBlockTestnet : ChecksumNqtBlock;
        }

        public static int GetReferencedTransactionFullHashBlockTimestamp(IConfiguration configuration)
        {
            return configuration.IsTestnet
                ? ReferencedTransactionFullHashBlockTimestampTestnet
                : ReferencedTransactionFullHashBlockTimestamp;
        }

        public static long GetUnconfirmedPoolDepositNQT(IConfiguration configuration)
        {
            return configuration.IsTestnet ? UnconfirmedPoolDepositNQTTestnet : UnconfirmedPoolDepositNQT;
        }
    }
}
