using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nxt.NET.Crypto;

namespace Nxt.NETTest.Crypto
{
    [TestClass]
    public class ReedSolomonTest
    {
        private readonly IList<Tuple<ulong, string>> _testValues = new List<Tuple<ulong, string>>();

        public ReedSolomonTest()
        {
            _testValues.Add(new Tuple<ulong, string>(8264278205416377583UL, "K59H-9RMF-64CY-9X6E7"));
            _testValues.Add(new Tuple<ulong, string>(8301188658053077183UL, "4Q7Z-5BEE-F5JZ-9ZXE8"));
            _testValues.Add(new Tuple<ulong, string>(1798923958688893959UL, "GM29-TWRT-M5CK-3HSXK"));
            _testValues.Add(new Tuple<ulong, string>(6899983965971136120UL, "MHMS-VHZT-W5CY-7CFJZ"));
            _testValues.Add(new Tuple<ulong, string>(1629938923029941274UL, "JM2U-U4AE-G7WF-3NP9F"));
            _testValues.Add(new Tuple<ulong, string>(6474206656034063375UL, "4K2H-NVHQ-7WXY-72AQM"));
            _testValues.Add(new Tuple<ulong, string>(1691406066100673814UL, "Y9AQ-VE8F-U9SY-3NAYG"));
            _testValues.Add(new Tuple<ulong, string>(2992669254877342352UL, "6UNJ-UMFM-Z525-4S24M"));
            _testValues.Add(new Tuple<ulong, string>(43918951749449909UL, "XY7P-3R8Y-26FC-2A293"));
            _testValues.Add(new Tuple<ulong, string>(9129355674909631300UL, "YSU6-MRRL-NSC4-9WHEX"));
            _testValues.Add(new Tuple<ulong, string>(0UL, "2222-2222-2222-22222"));
            _testValues.Add(new Tuple<ulong, string>(1UL, "2223-2222-KB8Y-22222"));
            _testValues.Add(new Tuple<ulong, string>(10UL, "222C-2222-VJTL-22222"));
            _testValues.Add(new Tuple<ulong, string>(100UL, "2256-2222-QFKF-22222"));
            _testValues.Add(new Tuple<ulong, string>(1000UL, "22ZA-2222-ZK43-22222"));
            _testValues.Add(new Tuple<ulong, string>(10000UL, "2BSJ-2222-KC3Y-22222"));
            _testValues.Add(new Tuple<ulong, string>(100000UL, "53P2-2222-SQQW-22222"));
            _testValues.Add(new Tuple<ulong, string>(1000000UL, "YJL2-2222-ZZPC-22222"));
            _testValues.Add(new Tuple<ulong, string>(10000000UL, "K7N2-222B-FVFG-22222"));
            _testValues.Add(new Tuple<ulong, string>(100000000UL, "DSA2-224Z-849U-22222"));
            _testValues.Add(new Tuple<ulong, string>(1000000000UL, "PLJ2-22XT-DVNG-22222"));
            _testValues.Add(new Tuple<ulong, string>(10000000000UL, "RT22-2BC2-SMPD-22222"));
            _testValues.Add(new Tuple<ulong, string>(100000000000UL, "FU22-4X69-74VX-22222"));
            _testValues.Add(new Tuple<ulong, string>(1000000000000UL, "C622-X5CC-EMM8-22222"));
            _testValues.Add(new Tuple<ulong, string>(10000000000000UL, "7A22-5399-RNFK-2B222"));
            _testValues.Add(new Tuple<ulong, string>(100000000000000UL, "NJ22-YEA9-KWDV-2U422"));
            _testValues.Add(new Tuple<ulong, string>(1000000000000000UL, "F222-HULE-NWMS-2FW22"));
            _testValues.Add(new Tuple<ulong, string>(10000000000000000UL, "4222-YBRW-T4XW-28WA2"));
            _testValues.Add(new Tuple<ulong, string>(100000000000000000UL, "N222-H3GS-QPZD-27US4"));
            _testValues.Add(new Tuple<ulong, string>(1000000000000000000UL, "A222-QGMQ-WDH2-2Q7SV"));
        }

        [TestMethod]
        public void TestEncoding()
        {
            foreach (var testValue in _testValues)
            {
                var actual = ReedSolomon.Encode((long)testValue.Item1);
                Assert.AreEqual(testValue.Item2, actual);
            }
        }

        [TestMethod]
        public void TestDecoding()
        {
            foreach (var testValue in _testValues)
            {
                var actual = ReedSolomon.Decode(testValue.Item2);
                Assert.AreEqual((long) testValue.Item1, actual);
            }
        }
    }
}
