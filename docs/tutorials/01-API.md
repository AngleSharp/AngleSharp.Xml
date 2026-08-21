---
title: "API Documentation"
section: "AngleSharp.Xml"
---
# API Documentation

AngleSharp.Xml can be used in two complementary ways:

1. Integrated in an AngleSharp browsing context via configuration
2. Directly through the dedicated XmlParser type

## Core entry points

### Configuration extension

Use this for content loading and auto-selection of XML / SVG document types.

```cs
var config = Configuration.Default
	.WithXml();
```

The extension registers XML and SVG document factories and provides an IXmlParser service.

### XmlParser

Use this for direct parsing of strings or streams.

```cs
var parser = new XmlParser();
var document = parser.ParseDocument("<book><title>AngleSharp</title></book>");
```

Async methods are available for both strings and streams:

```cs
var document = await parser.ParseDocumentAsync(xmlText);
```

### Parsing fragments

Use ParseFragment when you need to parse child nodes relative to a context element.

```cs
var context = document.DocumentElement;
var nodes = parser.ParseFragment("<chapter>Intro</chapter>", context);
```

## XmlParserOptions

XmlParserOptions controls parser behavior.

### IsSuppressingErrors

If true, the parser tries to continue instead of throwing parse exceptions.

```cs
var parser = new XmlParser(new XmlParserOptions
{
	IsSuppressingErrors = true,
});
```

### IsKeepingSourceReferences

If true, elements keep token source references. This is useful for diagnostics and tooling.

### OnCreated

Callback invoked when an element has been created, including source position.

```cs
var parser = new XmlParser(new XmlParserOptions
{
	OnCreated = (element, position) =>
	{
		Console.WriteLine($"{element.NodeName} at {position.Line}:{position.Column}");
	}
});
```

## Serialization APIs

### ToXml

Serializes with XmlMarkupFormatter:

```cs
var xml = document.ToXml();
```

### ToMarkup

Auto-selects formatter (XML / XHTML / HTML) based on document type:

```cs
var markup = document.ToMarkup();
```

### XmlMarkupFormatter

You can customize serialization behavior:

```cs
var formatter = new XmlMarkupFormatter
{
	IsAlwaysSelfClosing = true,
};

var xml = document.ToHtml(formatter);
```

### Canonical XML

`ToCanonicalXml` produces canonical UTF-8 bytes without a byte-order mark. Canonical XML 1.1 is the default mode.

```cs
var canonicalBytes = document.ToCanonicalXml();
```

Select Exclusive XML Canonicalization 1.0 and its inclusive namespace prefixes through options:

```cs
var options = new XmlCanonicalizationOptions
{
	Mode = XmlCanonicalizationMode.ExclusiveXml10,
	IncludeComments = true,
	InclusiveNamespacePrefixes = new[] { "ds", "#default" },
};

document.ToCanonicalXml(outputStream, options);
```

The stream overload leaves the destination stream open. Both modes remove XML declarations and doctypes, expand empty elements, replace CDATA boundaries with character content, normalize escaping, and order namespace declarations and attributes canonically.

Canonicalization accepts complete documents and rooted element subtrees. Canonical XML 1.1 subtree output carries applicable ancestor namespace, `xml:lang`, `xml:space`, and fixed-up `xml:base` context. Exclusive mode emits visibly used namespaces plus any configured inclusive prefixes.

### XSD validation

Validate an existing document against one or more inline XML Schema 1.0 documents:

```cs
var result = document.ValidateXsd(commonSchema, documentSchema);

if (!result.IsValid)
{
	foreach (var diagnostic in result.Diagnostics)
	{
		Console.WriteLine($"{diagnostic.Severity}: {diagnostic.Message} ({diagnostic.LineNumber}:{diagnostic.LinePosition})");
	}
}
```

Validation collects errors and warnings by default. Use options to stop after the first error or suppress warnings:

```cs
var options = new XsdValidationOptions
{
	IsFailFast = true,
	IsReportingWarnings = false,
};

var result = document.ValidateXsd(schemas, options);
```

For schemas using `xs:include` or `xs:import` with locations, configure a `System.Xml.Schema.XmlSchemaSet` with source URIs and a resolver, then pass it to `ValidateXsd`. External resolution is disabled by default; enable a resolver only for trusted schema locations.

```cs
var schemas = new XmlSchemaSet
{
	XmlResolver = new XmlUrlResolver(),
};
schemas.Add(null, schemaPath);

var result = document.ValidateXsd(schemas, new XsdValidationOptions
{
	SchemaResolver = new XmlUrlResolver(),
});
```

Diagnostic locations refer to the current serialized DOM used for post-parse validation. A source URI is included when the document or configured schema provides one.

## DOM model and querying

AngleSharp.Xml uses AngleSharp DOM interfaces and works with standard operations:

- QuerySelector / QuerySelectorAll
- CreateElement / CreateTextNode
- AppendChild / InsertBefore / Remove
- Attribute read and write methods

Example:

```cs
var item = document.QuerySelector("item");
item.SetAttribute("status", "active");
```

### XML namespace semantics

Common attributes from the XML namespace have convenience APIs on elements and documents.

```cs
var item = document.QuerySelector("item");

var effectiveBaseUri = item.GetXmlBaseUri();
var effectiveBaseUrl = item.GetXmlBaseUrl();
var effectiveLanguage = item.GetXmlLanguage();
var xmlId = item.GetXmlId();
var target = document.GetElementByXmlId("chapter-1");
```

`GetXmlBaseUri` resolves inherited `xml:base` values against the document URL and returns non-ASCII LEIRI characters without escaping. `GetXmlBaseUrl` returns AngleSharp's URL representation, whose `Href` is URI-escaped.

`GetXmlLanguage` returns the nearest inherited `xml:lang` value. An empty value resets inherited language information and is returned as an empty string; null means no language was declared.

Parsed `xml:id` values receive ID whitespace normalization. `GetElementByXmlId` searches current DOM state in document order, so attribute mutations are reflected immediately.

### XML declaration metadata

`IXmlDocument` exposes the parsed XML declaration. Documents without a declaration use XML 1.0 defaults and have a null `XmlEncoding`.

```cs
var document = parser.ParseDocument(
	"<?xml version=\"1.0\" encoding=\"utf-8\" standalone=\"yes\"?><root />");

Console.WriteLine(document.XmlVersion);    // 1.0
Console.WriteLine(document.XmlEncoding);   // utf-8
Console.WriteLine(document.XmlStandalone); // true
```

### CDATA sections

Create XML-native CDATA sections through `IXmlDocument`. Parsed CDATA sections are also preserved as `IXmlCDataSection` nodes during DOM transformations and XML serialization.

```cs
var section = document.CreateCDataSection("<unescaped>content</unescaped>");
document.DocumentElement.AppendChild(section);
```

CDATA content cannot contain the closing delimiter `]]>`. Creation and character-data mutations that would introduce it throw `DomException` without changing the section.

AngleSharp's core DOM does not expose entity reference nodes. `CreateEntityReference` therefore throws `NotSupportedException`; parsed entity references continue to be resolved to their replacement text.

## DTD validity signal

When a document contains DOCTYPE declarations, AngleSharp.Xml evaluates DTD-related validity and exposes the result via `document.IsValid`.

```cs
var parser = new XmlParser();
var document = parser.ParseDocument(xmlText);

if (!document.IsValid)
{
	Console.WriteLine("DTD validity check failed.");
}
```

For a detailed support matrix and examples, see the DTD Validation tutorial.

## Events

XmlParser exposes parsing lifecycle events:

- Parsing
- Parsed
- Error

These are useful for diagnostics, telemetry, and observability in hosted applications.
