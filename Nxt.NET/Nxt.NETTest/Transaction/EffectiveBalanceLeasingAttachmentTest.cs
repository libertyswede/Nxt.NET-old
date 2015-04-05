using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nxt.NET.Transaction;

namespace Nxt.NETTest.Transaction
{
    [TestClass]
    public class EffectiveBalanceLeasingAttachmentTest
    {
        private EffectiveBalanceLeasingAttachment _effectiveBalanceLeasingAttachment;
        private const short Period = 12345;

        [TestInitialize]
        public void TestInit()
        {
            _effectiveBalanceLeasingAttachment = new EffectiveBalanceLeasingAttachment(Period);
        }

        [TestMethod]
        public void PeriodProperty()
        {
            Assert.AreEqual(Period, _effectiveBalanceLeasingAttachment.Period);
        }

        [TestMethod]
        public void GetBytes()
        {
            var expected = new byte[] {57, 48};

            var actual = _effectiveBalanceLeasingAttachment.GetBytes();

            CollectionAssert.AreEqual(expected, actual);
        }
    }
}
