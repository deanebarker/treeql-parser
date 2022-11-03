using DeaneBarker.TreeQL;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TreeQL.Tests
{
    [TestClass]
    public class Skip
    {
        [TestMethod]
        public void SimpleSkip()
        {
            var q = TreeQueryParser.Parse("SELECT children OF target SKIP 1");

            Assert.AreEqual(1, q.Skip);
        }
    }
}
