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

## Events

XmlParser exposes parsing lifecycle events:

- Parsing
- Parsed
- Error

These are useful for diagnostics, telemetry, and observability in hosted applications.
