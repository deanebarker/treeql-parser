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

        [TestMethod]
        public void MultipleSorts()
        {
            var q = TreeQueryParser.Parse("SELECT children OF target ORDER BY name, age");

            Assert.AreEqual(2, q.Sort.Count());
            Assert.AreEqual("age", q.Sort[1].Value);
        }

        [TestMethod]
        public void DescendingSort()
        {
            var q = TreeQueryParser.Parse("SELECT children OF target ORDER BY name DESC");

            Assert.AreEqual(SortDirection.Descending, q.Sort.First().Direction);
        }

        [TestMethod]
        public void DescendingSortWithMultipleSorts()
        {
            var q = TreeQueryParser.Parse("SELECT children OF target ORDER BY name DESC, age");

            Assert.AreEqual(SortDirection.Descending, q.Sort.First().Direction);
            Assert.AreEqual(SortDirection.Ascending, q.Sort[1].Direction);
        }
    }
}
