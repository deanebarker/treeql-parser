# TreeQL Parser

This library is a parser to convert text into a structured object which represents a query for a tree-based datasource (or any data source, really -- how you apply it is up to you).

To be clear: this does not do the searching. How could it -- I don't anything about your repository. This library just converts text into an object, from which you can configure and execute a search.

This library is built on Parlot, which is a parsing library written by Sebastian Ros. Parlot is the parser used in the Fluid templating language.

>Note: it's sometimes diffucult to describe this library because we necessarily have to discuss what it's _supposed to do_ when used to power a search experience. Remember, the code here is simply to parse text into a query object, which you will then use to query your repository. So, forgive some (assumed) specifics on execution below.

## Basic Syntax

At its most basic:

```
TARGETS (one or many)
FILTERS (zero or many)
SORTS (zero or many)
SKIP (zero or one)
LIMIT (zero or one)
```

The details are very much like SQL:

```
SELECT
  [TARGET: [SCOPE] of [PATH] [INCLUSIVE/EXCLUSIVE]] AND [additional targets...]
  WHERE [FILTER: [FIELDNAME] [OPERATOR] [VALUE]] [AND/OR] [additional filters...]
  ORDER BY [FIELD] [ASC/DESC], [additional sorts]
  [SKIP #]
  [LIMIT #]
```

Text entered into this format will be turned in to a `TreeQuery` object (included in this library), which looks like this:

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
  Type: string
Sorts: List<Sort>
  FieldName: string
  Direction: SortDirection
Skip: int
Limit: int
```

Again, _what you do with this is up to you_. All this does is organize the information for you to use it to query whatever datasource you have.

## C# Usage

It's quite simple:

```
var query = TreeQueryParser.Parse(queryString);
```


## Example Query

```
SELECT children OF /some/path AND ancestors OF /some/other/path INCLUSIVE
  WHERE field == "value" AND other_field != "other_value"
  SORT BY field1, field2 desc
  SKIP 5
  LIMIT 10
```

With an expected implementation, this will --

* It will retrieve the "children" of `/some/path/` (whatever that means to your search implementation) and the "ancestors" of `/some/other/path/` and `/some/other/path/` _itself_ (that's the `INCLUSIVE` token). It will then combine these into a pool of content
* It will then filter this pool of content to find objects where `field` equals `value` _and_ `other_field` does not equal `other_value`
* It will then sort the results by `field1` in ascending order (the default). For items that have the same value for `field1`, they will be sorted by `field2` in descending order
* It will then skip the first five items
* It will retrieve up to the next 10 (so, items 6-15, assuming there's at least 15)

>Again, this is what it's _intended_ to do. What you do with your implementation is up to you.

## Targets

At least one Target is required. 

Target are intiated by the token `SELECT`.

Targets tell the query where to start -- what is the pool of content objects to gather, then optionally filter, sort, and subdivide?

Targets are "geographical," meaning they query based on a location in a tree of content. Where they start is a combination of scope and path.

* **Scope:** For a tree-based system, this would usually be `self`, `children`, `descendants`, `parent`, or `ancestors`. These descriptors are meant to be used in relation to the path.
* **Path:** This is the location on the tree that the scope refers to.

Scope and path are always separated by the token `OF`.

Some examples:

```
children OF /some/path/
ancestors OF /some/other/path/
self OF /
```

The scopes allowed are defined in a public static collection: `AllowedScopes`. The defaults are;

* "results"
* "self"
* "children"
* "parent"
* "ancestors"
* "descendants"
* "siblings

Any parsed scope _not_ in this collection will throw an error.

The path does not need to be quoted.

The path is validated by a public static `Func<string,bool>` called `TargetValidator`. If this returns false a parse error will be thrown with the text from `TargetValidatorError`. By default, `TargetValidator` always returns `true` (any path will validate)

For example, this `TargetValidator` will check for some specific formats:

```
TreeQueryParser.TargetValidator = (t) =>
{
    if (t.ToString().StartsWith("/") && t.ToString().EndsWith("/"))
        return true;

    if (t.ToString().StartsWith("@"))
        return true;

    if (t.ToString().All(c => Char.IsDigit(c)))
        return true;

    return false;
};
```

If it returns false, a descriptive error message can be specified:

```
TreeQueryParser.TargetValidatorError = "Target must (1) begin and end with a forward slash, (2) be an integer, or (3) start with \"@\"";
```

>TODO: The naming is off here. "path" should be "target" and the combination of scope/target should be...a "set"? A "collection"? Not sure.

The default is for the target to be exclusive, meaning `children OF /some/path/` does not include _/some/path/ itself_. If you want the Target to be inclusive, meaning you want both the children *and* the path itself, you can append `INCLUSIVE` to the end of the Target.

```
children OF /some/path/ INCLUSIVE
```

You can also append `EXCLUSIVE`, but this is assumed.

Target can be chained with `AND`. In these cases, all the Targets are retrieved individually and combined, then de-duped.

```
SELECT children OF /some/path/ AND siblings OF /some/other/path` INCLUSIVE
```

## Filters

Filters are optional. There can be an unlimited number of Filters.

Filters are initiated by by the token `WHERE`.

Filters are in the common format of:

```
[FIELD] [OPERATOR] [VALUE]
```

All will be parsed as strings.

If the field name contains a colon, it will be split on this. The value before the colon will become the `FieldName`, and the value after the colon will become the `Type`. This is for weakly typed repositories where the datatype needs to be specified for comparisons. If the field does not contain a colon, `Type` will default to "string" and can usually be ignored.

The operators allowed are defined in a public static collection: `AllowedOperators`. The defaults are;

* `=`
* `!=`
* `>`
* `>=`
* `<`
* `<=`

Any parsed operator _not_ in this collection will throw an error.

Filters can be chained with boolean `AND` or `OR` operators.

Parentheticals are not currently supported.

>Necesarily, this means that mixing `AND` and `OR` booleans doesn't make much sense. Logically, they all have to be one or the other, because without parentheticals, mixing them doesn't...work.

_All_ values have to be quoted with either single or double quotes. This differs from SQL where unquoted numbers are allowed.

Some examples:

```
WHERE name = "Deane"
WHERE age > "50"
WHERE name = "Annie" OR name = "Deane" OR name = "Alec"
```

## Sorts

Sorts are optional. There can be an unlimited number of Sorts.

Sorts are initiated by the token sequence `ORDER BY`.

A Sort specification can be a simple field name. The assumed direction is ascending, but this can be made explicit by appending `ASC`.

Descending can be specified by appending `DESC`.

Multiple sorts are separated by a comma.

Examples:

```
ORDER BY name
ORDER BY age DESC
ORDER BY age ASC, name DESC

## Skip

Skip is optional.

A Skip specification is initiated by the token `SKIP`.

There can only be a single Skip specification.

Following the token `SKIP` should be a simple integer, not quoted, no commas or decimals.

## Limit

Limit is optional.

A Limit specification is initiated by the token `LIMIT`.

There can only be a single Limit specification.

Following the token `LIMIT` should be a simple integer, not quoted, no commas or decimals.

## Whitespace and Indentation

Whitespace is ignored by the parser.

This works fine:

```
SELECT       children      OF       /
```

Indentation is also ignored. Any indentation in the examples in this document is solely for clarity.

## Multiline Queries

Queries can be single line or multiline. Before parsing, all newlines are replaced by spaces, effectively "gluing" the lines together.

This:

```
SELECT children OF / WHERE name = "Deane"
```

Is the same as:

```
SELECT
  children OF /
  WHERE name = "Deane"
  ```

## Comments

You can provide comments for entire lines. Before the lines are combined, any line that begins `#` (regardless of indentation) will be removed.

This query:

```
# This is my query
SELECT
  children of /
  WHERE name = "Deane"
  # AND age = "50"
  AND sex = "male"
 ```

 Will be parsed as:

 ```
 SELECT
   children of /
   WHERE name = "Deane"
   AND sex = "male"
 ```

Remember: comments only work on entire lines. You cannot put `#` in the middle of a line.

## Casing

The parser is completely case-insensitive. In fact, the entire query will be lower-cased before parsing.

Any casing in the examples in this document is solely for clarity.

>Note: this might change in the future. By lower-casing everything, your search execution is necessaily also case-insenstive. This is a limitation that might be addressed at some point.
