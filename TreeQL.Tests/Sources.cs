using DeaneBarker.TreeQL;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Parlot;
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

        [TestMethod]
        public void MultipleSources()
        {
            var q = TreeQueryParser.Parse("SELECT children OF target AND parent OF other_target");

            Assert.AreEqual(2, q.Sources.Count());
            Assert.AreEqual("other_target", q.Sources[1].Target);
        }

        // This test reveals the problem in setting options as static properties...
        // I need to use the correct pattern with options...

        // Update: I took a run at it; the problem is that these options are needed in the 
        // parser definition, and that's created in the static constructor. So I can't pass
        // in custom options, because there's only ONE parser

        //[TestMethod]
        //public void FailsCustomTargetValidator()
        //{
        //    var s = "Target must end with \"foo\"";

        //    TreeQueryParser.TargetValidator = (s) => { return s.ToString().EndsWith("foo"); };
        //    TreeQueryParser.TargetValidatorError = s;

        //    var e = Assert.ThrowsException<ParseException>(() => TreeQueryParser.Parse("SELECT children OF target"));
        //    Assert.AreEqual(e.Message, s);
        //}

        [TestMethod]
        public void InvalidScope()
        {
            var e = Assert.ThrowsException<ParseException>(() => TreeQueryParser.Parse("SELECT blah OF target"));
            Assert.IsTrue(e.Message.Contains("not allowed"));
        }

        [TestMethod]
        public void AddCustomScope()
        {
            TreeQueryParser.AllowedScopes.Add("foo");

            _ = TreeQueryParser.Parse("SELECT foo OF target");

            // No test; if we don't throw an exception, we pass
        }
    }
}
