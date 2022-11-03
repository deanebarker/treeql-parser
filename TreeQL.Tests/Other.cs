using DeaneBarker.TreeQL;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;

namespace TreeQL.Tests
{
    [TestClass]
    public class Other
    {
        [TestMethod]
        public void TheEverythingBagel()
        {
            var t = new Strings()
            {
                "SELECT children OF target",              // Basic source
                "  AND self OF other_target",             // Second source
                "  AND parent OF third_target INCLUSIVE", // Third source, inclusive
                "WHERE field1 = \"value1\"",              // Basic filter, double quoted
                "  AND field2 != 'value2'",               // Second filter, single quoted
                "  OR field3:number > 'value3'",          // Third filter, typed, "or" conjuction (note: this is logically wrong...)
                "ORDER BY field1,",                       // Basic sort, default ascending
                "  field2 DESC",                          // Second sort, descending
                "SKIP 100",                               // Basic skip
                "LIMIT 100"                               // Basic limit
            };

            var q = TreeQueryParser.Parse(t);

            // Sources
            Assert.AreEqual(3, q.Sources.Count());

            // Source 1
            Assert.AreEqual("children", q.Sources[0].Scope);
            Assert.AreEqual("target", q.Sources[0].Target);
            Assert.IsFalse(q.Sources[0].Inclusive);

            // Source 2
            Assert.AreEqual("self", q.Sources[1].Scope);
            Assert.AreEqual("other_target", q.Sources[1].Target);
            Assert.IsFalse(q.Sources[1].Inclusive);

            // Source 3
            Assert.AreEqual("parent", q.Sources[2].Scope);
            Assert.AreEqual("third_target", q.Sources[2].Target);
            Assert.IsTrue(q.Sources[2].Inclusive);

            // Filters
            Assert.AreEqual(3, q.Filters.Count());

            // Filter 1
            Assert.AreEqual("field1", q.Filters[0].FieldName);
            Assert.AreEqual("=", q.Filters[0].Operator);
            Assert.AreEqual("value1", q.Filters[0].Value);

            // Filter 2
            Assert.AreEqual("and", q.Filters[1].Conjunction);
            Assert.AreEqual("field2", q.Filters[1].FieldName);
            Assert.AreEqual("!=", q.Filters[1].Operator);
            Assert.AreEqual("value2", q.Filters[1].Value);

            // Filter 3
            Assert.AreEqual("or", q.Filters[2].Conjunction);
            Assert.AreEqual("field3", q.Filters[2].FieldName);
            Assert.AreEqual(">", q.Filters[2].Operator);
            Assert.AreEqual("value3", q.Filters[2].Value);
            Assert.AreEqual("number", q.Filters[2].Type);

            // Sorts
            Assert.AreEqual(2, q.Sort.Count());

            // Sort 1
            Assert.AreEqual("field1", q.Sort[0].Value);
            Assert.AreEqual(SortDirection.Ascending, q.Sort[0].Direction);

            // Sort 2
            Assert.AreEqual("field2", q.Sort[1].Value);
            Assert.AreEqual(SortDirection.Descending, q.Sort[1].Direction);

            // Skip
            Assert.AreEqual(100, q.Skip);

            // Limit
            Assert.AreEqual(100, q.Limit);
        }

        [TestMethod]
        public void LimitBeforeSkip()
        {
            var q = TreeQueryParser.Parse("SELECT children OF target LIMIT 100 SKIP 200");

            Assert.AreEqual(0, q.Skip); // This should fail to parse, because you can't have a skip after a limit
            Assert.AreEqual(100, q.Limit);
        }

        [TestMethod]
        public void WeirdSpacing()
        {
            var q = TreeQueryParser.Parse("SELECT             children               OF target");

            Assert.AreEqual("children", q.Sources.First().Scope);
        }


        [TestMethod]
        public void WeirdCasing()
        {
            var q = TreeQueryParser.Parse("Select CHILDREN of TaRgEt");

            Assert.AreEqual("children", q.Sources.First().Scope);
        }





        public class Strings : List<string>
        {
            public override string ToString()
            {
                return string.Join(" ", this.ToArray());
            }

            public static implicit operator string(Strings s) { return s.ToString(); }
        }
    }
}
