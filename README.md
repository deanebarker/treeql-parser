# TreeQL Parser

This library is a parser to convert text into a structured object which represents a query for a tree-based datasource (or any data source, really -- how you apply it is up to you).

To be clear: this does not do the searching, it just creates a structure object for the query, which you can then use for your search.

This library is built on Parlot, which is a parsing library written by Sebastian Ros.

## Basic Syntax

It's pretty SQL-ish:

```
SELECT
  [TARGET: [SCOPE] of [PATH] [INCLUSIVE/EXCLUSIVE]]
  WHERE [FILTER: [FIELDNAME] [OPERATOR] [VALUE]]
  ORDER BY [FIELD] [ASC/DESC]
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

## Example Query

```
SELECT children OF /some/path AND ancestors OF /some/other/path INCLUSIVE
  WHERE field == "value" AND other_field != "other_value"
  SORT BY field1, field2 desc
  SKIP 5
  LIMIT 10
```

With an expected implementation, this will --

* It will retrieve the "children" of "/some/path/" (whatever that means to your implementation) and the "ancestors" of "/some/other/path/" and "/some/other/path/" _itself_ (that's the "inclusive" part). It will combine these in a big pool of content
* It will then filter this pool of content to find objects where `field` equals value _and_ "other_field" does not equal "other_value"
* It will then sort the results by `field1` in ascending order (the default). For items that have the same value for `field1`, they will be sorted by `field2` in descending order
* It will then skis the first five items
* It will retrieve the next 10 (so, items 6-15)

Again, this is what it's _intended_ to do. What you do with your implementation is up to you.

## Targets

Targets tell the query where to start -- what is the pool of content objects to accumulate then filter?

Targets are "geographical," meaning they query based on a location in a tree of content. Where they start is a combination of scope and path, and this is called a Target.

* **The Scope:** For a tree-based system, this would usually be `self`, `children`, `descendants`, `parent`, or `ancestors`. These descriptors are meant to be used in relation to the path.
* **The Path:** This is the location on the tree that the scope refers to.

Scope and path are always separated by the constant `OF`.

The result should be intuitive:

```
children OF /some/path/
ancestors OF /some/other/path/
self OF /
```

The default is for the target to be exclusive, meaning `children OF /some/path/` does not include _/some/path/ itself_. If you want the Target to be inclusive, meaning you want both the children and the path itself, you can append `INCLUSIVE` to the end of the target.

```
children OF /some/path/ INCLUSIVE
```

Target can be chained with `AND`. In these cases, all the Targets are retrieved individually and combined, then de-duped.

```
SELECT children of /some/path/ AND siblings of /some/other/path` INCLUSIVE
```
