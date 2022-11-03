using DeaneBarker.TreeQL;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace TreeQL.Tests
{
    [TestClass]
    public class Limit
    {
        [TestMethod]
        public void SimpleSkip()
        {
            var q = TreeQueryParser.Parse("SELECT children OF target LIMIT 1");

            Assert.AreEqual(1, q.Limit);
        }
    }
}
