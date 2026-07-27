---
title: "Questions"
section: "AngleSharp.Xml"
---
# Frequently Asked Questions

## Do I need AngleSharp.Xml if I already use AngleSharp?

Yes, if you want robust XML parsing and XML-specific behavior. AngleSharp.Xml integrates with the same DOM and configuration pipeline used by AngleSharp.

## Should I use WithXml or XmlParser directly?

Use WithXml when loading resources via BrowsingContext and content types. Use XmlParser when you already have raw XML text or streams.

## Can I parse fragments instead of full documents?

Yes. Use ParseFragment with a context element when you only need child nodes from partial XML content.

## Can I keep source positions for diagnostics?

Yes. Set IsKeepingSourceReferences to true and optionally use the OnCreated callback in XmlParserOptions.

## Does the serializer preserve namespaces and prefixes?

Yes. Prefixes and namespace URIs are preserved through the AngleSharp DOM model and formatter pipeline.

## Why does my malformed XML throw?

By default, malformed XML throws parse exceptions. If you prefer best-effort parsing, enable IsSuppressingErrors.

## Can I transform a self-closing element into a non-empty element through DOM operations?

Yes. In XML DOM terms, self-closing syntax and explicit open+close syntax both represent elements. If children are added, serialization should emit open+close form.

## Is XML Schema (XSD) validation included?

No built-in XSD validation pipeline is provided by this package. If you need strict schema validation, combine AngleSharp.Xml with dedicated schema validation tooling.

## How do I validate XML against a DTD?

Parse with XmlParser and inspect `document.IsValid`. This gives practical DTD validity results for supported declaration patterns, especially for internal subsets.

## Are external DTD files automatically resolved?

Not as a full conformance feature. Internal subset scenarios are the most reliable path today. If you depend on external subsets/entities for strict compliance, add an external validation layer in your pipeline.

## Is XPath included?

This package is focused on the AngleSharp DOM and parser integration. Most users query nodes through AngleSharp DOM APIs (such as CSS selectors).
