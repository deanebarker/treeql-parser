using DeaneBarker.TreeQL;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Parlot;
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

        [TestMethod]
        public void MultipleFilters()
        {
            var q = TreeQueryParser.Parse("SELECT children OF target WHERE name = 'deane' AND age = '50'");

            Assert.AreEqual(2, q.Filters.Count());
            Assert.AreEqual("50", q.Filters[1].Value);
        }

        [TestMethod]
        public void DisallowedOperator()
        {
            _ = Assert.ThrowsException<ParseException>(() => TreeQueryParser.Parse("SELECT children OF target WHERE name === 'deane'"));
        }

        [TestMethod]
        public void AddCustomOperator()
        {
            TreeQueryParser.AllowedOperators.Add("~");

            _ = TreeQueryParser.Parse("SELECT children OF target WHERE name ~ 'Deane'");

            // No test; if we don't throw an exception, we pass
        }

        [TestMethod]
        public void CustomType()
        {
            var q = TreeQueryParser.Parse("SELECT children OF target WHERE age:number = '50'");

            Assert.AreEqual("age", q.Filters[0].FieldName);
            Assert.AreEqual("number", q.Filters[0].Type);
        }
    }
}
