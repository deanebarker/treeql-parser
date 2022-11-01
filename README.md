# TreeQL Parser

This library is a parser to convert text into a structured object which represents a query for a tree-based datasource (or any data source, really -- how you apply it is up to you).

To be clear: this does not do the searching, it just creates a structure object for the query, which you can then use for your search.

This library is built on Parlot, which is a parsing library written by Sebastian Ros.

## Basic Syntax

It's pretty SQL-ish:

```
SELECT
  [[SCOPE] of [TARGET][INCLUSIVE/EXCLUSIVE]]
  WHERE [FILTER]
  ORDER BY [FIELD] ["asc" or "desc"]
  [SKIP #]
  [LIMIT #]
```

Text entered into this format will be turned in to a `TreeQLQUery` object, which looks like this:

```
TreeQLQuery
---
Targets: List<Target>
  Scope: string
  Path: string (validated)
  Inclusive/Exclusive
Filters: List<Filter>
  FieldName: string
  Operator: string
  Value: string
Sorts: List<Sort>
  FieldName: string
  Direction: SortDirection
Skip: int
Limit: int
```

Again, _what you do with this is up to you_. All this does is organize the information for you to use it to query whatever datasource you have.
