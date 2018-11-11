using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WpfMouseWheel.Test
{
    [TestClass]
    public class UL01_Overflow
    {
        [TestMethod]
        public void T01_UncheckedLong()
        {
            long x1 = long.MaxValue;
            long x2 = unchecked(x1 + 10);
            long delta = unchecked(x2 - x1);

            Assert.AreEqual(long.MinValue + 9, x2);
            Assert.AreEqual(10, delta);

        }
    }
}
