---
title: "Practical Use Cases"
section: "AngleSharp.Xml"
---
# Practical Use Cases

This guide shows when AngleSharp.Xml is a good fit and how to structure real-world usage.

## 1. Configuration migration tool

Scenario: You need to update legacy XML configuration files to a new shape.

Approach:

1. Parse each file with XmlParser
2. Use selector-based discovery for target nodes
3. Create missing elements and attributes
4. Serialize with ToXml

```cs
var parser = new XmlParser();
var document = parser.ParseDocument(xmlText);

foreach (var node in document.QuerySelectorAll("settings > add"))
{
    if (node.GetAttribute("enabled") == null)
    {
        node.SetAttribute("enabled", "true");
    }
}

var output = document.ToXml();
```

## 2. XML feed enrichment

Scenario: A service receives XML and must append calculated metadata.

Approach:

1. Parse inbound XML
2. Compute metadata
3. Append extension nodes
4. Return serialized XML

```cs
var parser = new XmlParser();
var doc = parser.ParseDocument(feed);
var item = doc.QuerySelector("item");

var meta = doc.CreateElement("meta");
meta.SetAttribute("processedAt", DateTimeOffset.UtcNow.ToString("O"));
item.AppendChild(meta);

return doc.ToXml();
```

## 3. Unified HTML and XML processing pipeline

Scenario: The same backend processes HTML pages and XML documents.

Approach:

1. Build one configuration with WithXml and other AngleSharp features
2. Load inputs by content type
3. Work with a consistent DOM API

```cs
var config = Configuration.Default
    .WithDefaultLoader()
    .WithXml();

var context = BrowsingContext.New(config);
var xmlDoc = await context.OpenAsync(req =>
{
    req.Content(xmlContent);
    req.Header("Content-Type", "text/xml");
});
```

## 4. Diagnostics and source mapping

Scenario: You need to report precise XML errors in an editor or CI report.

Approach:

1. Enable IsKeepingSourceReferences
2. Capture OnCreated positions
3. Map business rules back to line and column

```cs
var parser = new XmlParser(new XmlParserOptions
{
    IsKeepingSourceReferences = true,
    OnCreated = (el, pos) => Console.WriteLine($"{el.NodeName} at {pos.Line}:{pos.Column}")
});

var doc = parser.ParseDocument(xmlText);
```

## 5. Namespace-safe processing

Scenario: Your input uses multiple namespace prefixes and xml-prefixed attributes.

Approach:

1. Parse normally with XmlParser
2. Read attributes by full name when needed
3. Use NamespaceUri checks for robust routing logic

```cs
var doc = new XmlParser().ParseDocument("<root xml:lang=\"en\" xmlns:ns=\"urn:demo\"><ns:item /></root>");
var lang = doc.DocumentElement.Attributes["xml:lang"];

if (lang?.NamespaceUri == NamespaceNames.XmlUri)
{
    // Handle language-sensitive processing
}
```

## Picking the right strategy

Choose this library first when your priority is:

- AngleSharp ecosystem integration
- DOM-based XML transformations
- Consistent programming style across markup types

Combine with specialized XML libraries when your priority is:

- Strict schema validation requirements
- XPath-dominant workflows
- Domain-specific XML standards validation
