using Parlot;
using Parlot.Fluent;
using System;
using System.Collections.Generic;
using System.Linq;
using static Parlot.Fluent.Parsers;

namespace DeaneBarker.TreeQL
{
    public static class TreeQueryParser
    {
        public static Func<TextSpan, bool> TargetValidator { get; set; } = (t) => { return true; }; // Validate anything by default
        public static string TargetValidatorError { get; set; } = string.Empty;
        public static string[] AllowedOperators { get; set; } = new[] { "=", "!=", ">", ">=", "<", "<=" };
        public static string[] AllowedScopes { get; set; } = new[] { "results", "self", "children", "parent", "ancestors", "descendants", "siblings" };
        public static string CommentPrefix { get; set; } = "#";


        // This is populated once in the static construtor
        private static Parser<TreeQuery> parser;

        // This is the only available method on the type
        public static TreeQuery Parse(string q)
        {
            if(string.IsNullOrWhiteSpace(q))
            {
                return new TreeQuery();
            }

            q = Clean(q);
            var query = parser.Parse(q);
            query.OriginalQueryText = q;
            return query;
        }

        private static string Clean(string input)
        {
            var lines = input.Split(new string[] { "\n", "\r\n", Environment.NewLine }, StringSplitOptions.None).AsQueryable();

            lines = lines
                .Where(l => !l.Trim().StartsWith(CommentPrefix))
                .Select(s => s.Trim());

            return string.Join(" ", lines).ToLower().Trim(); 
        }

        static TreeQueryParser()
        {
            var of = Terms.Text("of");
            var and = Terms.Text("and");
            var or = Terms.Text("or");
            var select = Terms.Text("select");
            var comma = Literals.Char(',');


            // Target
            var inclusive = Terms.Text("inclusive");
            var exclusive = Terms.Text("exclusive");

            var source =
                Terms.NonWhiteSpace() // Item1
                .AndSkip(of)
                .And(Terms.NonWhiteSpace()) // Item2
                .And(ZeroOrOne(OneOf(inclusive, exclusive))) // Item3
                .Then(v =>
                {
                    // Make sure this is a valid scope
                    // We have to do it this way because we want the list of scopes to be dynamic
                    // So, everything PARSES, but then we VALIDATE
                    if (!AllowedScopes.Contains(v.Item1.ToString().ToLower().Trim()))
                    {
                        throw new ParseException($"Scope \"{v.Item1}\" not allowed. Allowed scopes: {string.Join(", ", AllowedScopes)}", new TextPosition(v.Item1.Offset, 0, 0));
                    }

                    // Make sure this is a valid path
                    // We have to do it this way because we want the validation of paths to be dynamic
                    // So, everything PARSES, but then we VALIDATE
                    if (!TargetValidator(v.Item2.ToString()))
                    {
                        throw new ParseException(TargetValidatorError, new TextPosition(v.Item2.Offset, 0, 0));
                    }

                    return new Source()
                    {
                        Scope = v.Item1.ToString(),
                        Target = v.Item2.ToString(),
                        Inclusive = v.Item3 == "inclusive"
                    };
                });



            // Where clause
            var conjunction = OneOf(and, or);
            var where = Terms.Text("where");
            var fieldName = Terms.NonWhiteSpace();
            var value = Terms.String(StringLiteralQuotes.SingleOrDouble);
            var whereClause = ZeroOrOne(conjunction) // Item1
                .And(fieldName) // Item2
                .And(Terms.NonWhiteSpace()) // Item3
                .And(value) // Item4
                .Then(v =>
                {
                    // Make sure this is a valid operator
                    // We have to do it this way because we want the list of operators to be dynamic
                    // So, everything PARSES, but then we VALIDATE
                    if (!AllowedOperators.Contains(v.Item3.ToString().ToLower().Trim()))
                    {
                        throw new ParseException($"Operation \"{v.Item3}\" not allowed. Allowed operators: {string.Join(", ", AllowedOperators)}", new TextPosition(v.Item3.Offset, 0, 0));
                    }

                    var fieldName = v.Item2.ToString().Split(':').First();
                    var type = v.Item2.ToString().Contains(":") ? v.Item2.ToString().Split(':').Last() : "string";

                    return new Filter()
                    {
                        Conjunction = v.Item1,
                        FieldName = fieldName,
                        Type = type,
                        Operator = v.Item3.ToString(),
                        Value = v.Item4.ToString()
                    };
                });



            // Sort Value
            var orderBy = Terms.Text("order by");
            var ascending = Terms.Text("asc");
            var descending = Terms.Text("desc");
            var sortDirection = OneOf(ascending, descending);
            var sortValue = SkipWhiteSpace(Literals.Pattern(c => char.IsLetterOrDigit(c) || c == ':')).And(ZeroOrOne(OneOf(Terms.Text("asc"), Terms.Text("desc")))).Then(v =>
            {
                return new Sort()
                {
                    Value = v.Item1.ToString(),
                    Direction = v.Item2?.ToString() == "desc" ? SortDirection.Descending : SortDirection.Ascending
                };
            });


            // Skip value
            var skip = Terms.Text("skip").SkipAnd(Terms.Integer());


            // Limit value
            var limit = Terms.Text("limit").SkipAnd(Terms.Integer());


            // Full Command
            parser =
                select.ElseError("Expected \"select\"")
                .SkipAnd(Separated(and, source)) // Item 1
                .And(ZeroOrOne(where.SkipAnd(OneOrMany(whereClause)))) // Item 2
                .And(ZeroOrOne(orderBy.SkipAnd(Separated(comma, sortValue)))) // Item 3
                .And(ZeroOrOne(skip)) // Item 4
                .And(ZeroOrOne(limit)) // Item 5
                .Then(v =>
                {
                    var query = new TreeQuery()
                    {
                        Sources = v.Item1,
                        Filters = v.Item2 ?? new List<Filter>(),
                        Sort = v.Item3 ?? new List<Sort>(),
                        Skip = (int)v.Item4,
                        Limit = (int)v.Item5
                    };

                    return query;

                });
        }
    }

    public class TreeQuery
    {
        public List<Source> Sources { get; set; } = new List<Source>();
        public List<Sort> Sort { get; set; } = new List<Sort>();
        public long Limit { get; set; }
        public int Skip { get; set; }
        public string Tag { get; set; }
        public List<Filter> Filters { get; set; } = new List<Filter>();

        // Just for debugguing
        public string OriginalQueryText { get; set; }
    }

    public class Filter
    {
        public string FieldName { get; set; }
        public string Type { get; set; }
        public string Operator { get; set; }
        public string Value { get; set; }
        public string Conjunction { get; set; }
    }

    public class Source
    {
        public string Scope { get; set; }
        public string Target { get; set; }
        public bool Inclusive { get; set; } // Whether or not the include the target
    }

    public class Sort
    {
        public string Value { get; set; }
        public SortDirection Direction { get; set; }
    }

    public enum SortDirection
    {
        Ascending,
        Descending
    }
}
