using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nxt.NET.Crypto;

namespace Nxt.NETTest.Crypto
{
    [TestClass]
    public class Curve25519Test
    {
        [TestMethod]
        public void IsCanonicalSignatureShouldReturnTrue()
        {
            var signature = new byte[]
            {
                25, 170, 168, 175, 158, 35, 249, 222, 150, 44, 85, 200, 164, 250, 175, 53, 123, 83, 113, 73, 48, 252, 93,
                13, 189, 136, 133, 15, 167, 141, 158, 13, 61, 64, 66, 49, 149, 71, 76, 199, 92, 243, 127, 254, 189, 186,
                26, 109, 137, 55, 196, 64, 116, 156, 187, 163, 207, 230, 183, 237, 105, 253, 6, 89
            };

            Assert.IsTrue(Curve25519.IsCanonicalSignature(signature));
        }

        [TestMethod]
        public void IsCanonicalPublicKey()
        {
            var publicKey = new byte[]
            {
                163, 194, 119, 246, 220, 214, 94, 149, 64, 145, 195, 6, 34, 10, 55, 36, 17, 42, 8, 227, 111, 81, 230, 127,
                119, 131, 169, 211, 162, 196, 255, 47
            };

            Assert.IsTrue(Curve25519.IsCanonicalPublicKey(publicKey));
        }

        [TestMethod]
        public void Verify()
        {
            var signaturePart1 = new byte[]
            {
                177, 156, 143, 181, 91, 68, 9, 212, 4, 85, 211, 2, 124, 157, 77, 48, 124, 43, 64, 121, 253, 58, 251, 236,
                145, 210, 244, 80, 55, 226, 152, 1
            };

            var signaturePart2 = new byte[]
            {
                64, 104, 72, 33, 50, 146, 73, 7, 111, 182, 174, 77, 14, 45, 253, 216, 4, 123, 233, 6, 35, 141, 57, 207,
                201, 255, 133, 31, 148, 206, 90, 36
            };

            var publicKey = new byte[]
            {
                35, 147, 159, 165, 175, 92, 127, 72, 232, 236, 93, 28, 167, 56, 100, 110, 33, 107, 68, 59, 197, 228, 31,
                254, 87, 7, 84, 3, 203, 185, 1, 50
            };

            var expected = new byte[]
            {
                68, 115, 178, 235, 186, 83, 155, 230, 160, 231, 143, 250, 97, 133, 252, 3, 91, 240, 233, 156, 162, 112, 57,
                116, 75, 204, 76, 235, 133, 222, 252, 14
            };
            var actual = new byte[32];

            Curve25519.Verify(actual, signaturePart1, signaturePart2, publicKey);

            CollectionAssert.AreEqual(expected, actual);
        }
    }
}
