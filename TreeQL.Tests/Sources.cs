using DeaneBarker.TreeQL;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace TreeQL.Tests
{
    [TestClass]
    public class Sources
    {
        [TestMethod]
        public void SimpleSource()
        {
            var q = TreeQueryParser.Parse("SELECT children OF target");

            Assert.AreEqual("target", q.Sources.First().Target);
            Assert.AreEqual("children", q.Sources.First().Scope);           
        }
    }
}
