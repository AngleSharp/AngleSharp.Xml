---
title: "DTD Validation"
section: "AngleSharp.Xml"
---
# DTD Validation with AngleSharp.Xml

This guide explains what a DTD is, how AngleSharp.Xml uses it, and what validation behavior is currently supported.

## What is a DTD?

A Document Type Definition (DTD) defines allowed structure for an XML document:

- Which elements may appear
- Which attributes are allowed or required
- Which entities are declared
- Which root element is expected

In practice, a DTD can be internal (inside the DOCTYPE declaration) or external (referenced by SYSTEM or PUBLIC identifiers).

## How validation works in AngleSharp.Xml

AngleSharp.Xml builds a DOM and sets document validity on the resulting XML document.

Typical flow:

1. Parse XML with XmlParser
2. Read document.IsValid
3. Handle invalid documents according to your app policy

Example:

```cs
var parser = new XmlParser();
var document = parser.ParseDocument(@"<!DOCTYPE root [
  <!ELEMENT root (item+)>
  <!ELEMENT item (#PCDATA)>
  <!ATTLIST item code CDATA #REQUIRED>
]>
<root><item code=""A1"">ok</item></root>");

if (!document.IsValid)
{
    // Reject, log, or route for remediation
}
```

## Internal subset example

This example is invalid because the required attribute is missing:

```cs
var parser = new XmlParser();
var invalid = parser.ParseDocument(@"<!DOCTYPE root [
  <!ELEMENT root (item+)>
  <!ELEMENT item (#PCDATA)>
  <!ATTLIST item code CDATA #REQUIRED>
]>
<root><item>missing code</item></root>");

Console.WriteLine(invalid.IsValid); // False
```

## What is supported today

Current DTD-related behavior includes:

- DOCTYPE root-name consistency check (root element name must match DOCTYPE name)
- Internal subset declaration parsing for core validation scenarios
- Element content checks for common models:
  - ANY
  - EMPTY
  - Mixed content in form (#PCDATA|name|...)*
  - Simple ordered sequence in form (a,b,c)
- Attribute validation in internal subset scenarios:
  - Undeclared attributes are flagged invalid (except namespace declarations)
  - #REQUIRED constraints are enforced
  - #FIXED constraints are enforced when attribute is present
  - ID values are normalized and must be unique across the document
  - Each element type may declare at most one ID attribute, with #IMPLIED or #REQUIRED defaults
  - IDREF and IDREFS values must resolve to declared IDs, including forward references
  - ENTITY and ENTITIES values must name declared unparsed entities
  - Unparsed entities and NOTATION attributes must reference declared notations
- Internal general entity replacement in text nodes for declared internal entities
- External subset loading through the browsing context's resource loader (see [Loading the external subset](#loading-the-external-subset))
  - Relative SYSTEM identifiers are resolved against the document's URL
- Replacement of entities declared in a loaded external subset

## Loading the external subset

A DOCTYPE's SYSTEM identifier names a resource the document chooses, so it is fetched like the document's other resources: through the browsing context's resource loader, which only exists when resource loading is enabled, and its requesters, which decide which protocols are served. Without them, SYSTEM identifiers are ignored.

```cs
var config = Configuration.Default
  .WithXml()
  .WithDefaultLoader(new LoaderOptions { IsResourceLoadingEnabled = true });
var context = BrowsingContext.New(config);
var document = await context.GetService<IXmlParser>().ParseDocumentAsync(source);
```

WithDefaultLoader adds AngleSharp's HTTP requester when no requester is configured, so DTDs then load over HTTP from the URLs documents name; only enable resource loading for documents you trust. Local files need a ile: requester, such as the one in AngleSharp.Io. External entities declared in a DTD are not loaded.

The synchronous ParseDocument waits for the download. Parse asynchronously when the caller runs on a synchronization context (a UI thread, classic ASP.NET) that the requesters resume on, as blocking that thread can deadlock.
## DTD ID lookup

DTD-declared IDs can be inspected and looked up after parsing:

```cs
var id = element.GetDtdId();
var target = document.GetElementByDtdId("chapter-1");
```

Lookup reads the current attribute value, so later DOM mutations are reflected immediately. The declared ID attribute metadata is preserved when XML elements are cloned.

## Entity expansion limit

Expanding declared entities can multiply a small document into a huge one: each entity can reference another several times. The parser counts every character produced by expanding a declared entity, in text and in attribute values, against `XmlParserOptions.MaxCharactersFromEntities` (10,000,000 by default, 0 for no limit). Beyond the limit it throws an `XmlParseException`, or keeps the remaining references as text when errors are suppressed. See the [API tutorial](01-API.md#maxcharactersfromentities).

## What is currently limited or not supported

You should be aware of these boundaries:

- External DTD/entity retrieval is limited
  - The external subset only loads with resource loading enabled and a requester for its protocol
  - External entities are not loaded
  - HTTP/network retrieval is not implemented
  - PUBLIC identifier resolution/catalog behavior is not implemented
- Parameter entity and external-subset behavior is not a full XML 1.0 conformance implementation
  - Parameter entity references inside entity values are kept as literal text, not expanded
- Full content-model grammar support is incomplete in internal fallback paths
  - Complex nested groups and advanced quantifier combinations may not be fully validated
- Attribute default-value materialization from DTD declarations is limited
- XML Schema 1.0 validation is available separately through `ValidateXsd`

## Recommended usage pattern

For production XML workflows:

1. Use internal subset validation where feasible if you rely on built-in IsValid.
2. Load external subsets through the resource loader only for trusted documents, and parse asynchronously.
3. Treat IsValid as practical DTD validation, not full standards-compliance validation.
4. If you need strict external DTD processing (for example, network retrieval or XML catalog workflows) or full conformance, add a dedicated XML validation layer.

## Troubleshooting tips

If document.IsValid is unexpectedly true or false:

- Confirm DOCTYPE/internal subset declarations are present and syntactically correct
- Disable recovery behavior in your own pipeline (for example, avoid suppressing parse errors unless needed)
- Reduce XML to a minimal reproducible case and verify which declaration triggers the mismatch
- Add integration tests around your exact DTD patterns
