---
title: "Capabilities"
section: "AngleSharp.Xml"
---
# What AngleSharp.Xml Can Do

AngleSharp.Xml extends the AngleSharp ecosystem with XML-native parsing and serialization while keeping a familiar DOM programming model.

## Parsing

- Parse complete XML documents from strings and streams
- Parse fragments relative to an existing context element
- Parse synchronously or asynchronously
- Integrate into BrowsingContext loading by content type

## DOM integration

- Use AngleSharp DOM interfaces (IDocument, IElement, IAttr, INode)
- Query and update XML nodes with the same API style used in AngleSharp
- Manipulate attributes, text nodes, comments, and processing instructions
- Inspect XML declaration version, encoding, and standalone metadata
- Create and preserve XML CDATA section nodes
- Resolve effective XML Base URIs and URLs
- Normalize and look up live `xml:id` values
- Resolve inherited `xml:lang` values

## Namespace handling

- Supports prefixed and default namespaces
- Handles xml-prefixed attributes with the XML namespace URI
- Resolves namespace declarations and prefixed attributes consistently during parse

## Document type support

- Produces XML documents and SVG documents depending on content type
- Works with XML-oriented workflows in mixed markup processing pipelines
- Validates DTD ID uniqueness and IDREF / IDREFS references
- Validates ENTITY / ENTITIES against declared unparsed entities and notations
- Looks up elements by their DTD-declared ID attributes

## Serialization

- Serialize to XML-oriented output with ToXml
- Use auto-selected formatter behavior with ToMarkup
- Configure empty-element behavior using XmlMarkupFormatter.IsAlwaysSelfClosing
- Serialize documents and rooted element subtrees using Canonical XML 1.1
- Serialize using Exclusive XML Canonicalization 1.0 with inclusive namespace prefixes

## Diagnostics and control

- Suppress parse errors in best-effort scenarios
- Keep source references for analysis or tooling
- Observe element creation positions via callback hooks
- Subscribe to parser lifecycle events (Parsing, Parsed, Error)
- Validate existing documents against one or more XSD 1.0 schemas
- Collect XSD errors and warnings with available source locations
- Resolve trusted schema imports and includes through a configured resolver

## Typical high-value scenarios

- Build or transform XML feeds and configuration files
- Process XML in services already using AngleSharp for HTML
- Normalize or re-serialize XML documents after DOM manipulation
- Inspect XML data with a browser-like DOM API
