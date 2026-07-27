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
- Internal general entity replacement in text nodes for declared internal entities
- External subset loading for local file-based SYSTEM identifiers
  - Absolute file paths are supported
  - Relative paths are resolved against the current process working directory
- External general entity replacement when entities are declared in loaded local external subsets

## What is currently limited or not supported

You should be aware of these boundaries:

- External DTD/entity retrieval is limited
  - HTTP/network retrieval is not implemented
  - PUBLIC identifier resolution/catalog behavior is not implemented
- Parameter entity and external-subset behavior is not a full XML 1.0 conformance implementation
- Full content-model grammar support is incomplete in internal fallback paths
  - Complex nested groups and advanced quantifier combinations may not be fully validated
- Attribute default-value materialization from DTD declarations is limited
- XSD validation is not included

## Recommended usage pattern

For production XML workflows:

1. Use internal subset validation where feasible if you rely on built-in IsValid.
2. Use local file-based SYSTEM identifiers if you need external subset declarations without adding extra tooling.
3. Treat IsValid as practical DTD validation, not full standards-compliance validation.
4. If you need strict external DTD processing (for example, network retrieval or XML catalog workflows) or full conformance, add a dedicated XML validation layer.

## Troubleshooting tips

If document.IsValid is unexpectedly true or false:

- Confirm DOCTYPE/internal subset declarations are present and syntactically correct
- Disable recovery behavior in your own pipeline (for example, avoid suppressing parse errors unless needed)
- Reduce XML to a minimal reproducible case and verify which declaration triggers the mismatch
- Add integration tests around your exact DTD patterns
