using DeaneBarker.TreeQL;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace TreeQL.Tests
{
    [TestClass]
    public class Filters
    {
        [TestMethod]
        public void SimpleFilter()
        {
            var q = TreeQueryParser.Parse("SELECT children OF target WHERE name = 'deane'");

            Assert.AreEqual("name", q.Filters.First().FieldName);
            Assert.AreEqual("=", q.Filters.First().Operator);
            Assert.AreEqual("deane", q.Filters.First().Value);
        }
    }
}
