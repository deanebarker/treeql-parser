﻿using System.Collections.Generic;

namespace DeaneBarker.TreeQL
{
    public class TreeQuery
    {
        public List<Target> Targets { get; set; } = new List<Target>();
        public List<Sort> Sort { get; set; } = new List<Sort>();
        public long Limit { get; set; }
        public int Skip { get; set; }
        public string Tag { get; set; }
        public List<Filter> Filters { get; set; } = new List<Filter>();

        // Just for debugguing
        public string Source { get; set; }
    }

    public class Filter
    {
        public string FieldName { get; set; }
        public string Type { get; set; }
        public string Operator { get; set; }
        public string Value { get; set; }
        public string Conjunction { get; set; }
    }

    public class Target
    {
        public string Scope { get; set; }
        public string Path { get; set; }
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
