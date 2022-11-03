using DeaneBarker.TreeQL;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace TreeQL.Tests
{
    [TestClass]
    public class Sorts
    {
        [TestMethod]
        public void SimpleSort()
        {
            var q = TreeQueryParser.Parse("SELECT children OF target ORDER BY name");

            Assert.AreEqual("name", q.Sort.First().Value);
            Assert.AreEqual(SortDirection.Ascending, q.Sort.First().Direction);
        }
    }
}
