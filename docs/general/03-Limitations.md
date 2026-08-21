---
title: "Limitations"
section: "AngleSharp.Xml"
---
# Limitations and Boundaries

AngleSharp.Xml is designed for practical XML parsing and DOM workflows in the AngleSharp ecosystem. It is not intended to replace every specialized XML stack.

## XSD validation scope

XSD validation targets XML Schema 1.0 through the platform `System.Xml.Schema` engine. XML Schema 1.1 is not supported. Validation operates on the current serialized DOM after parsing, so diagnostic line positions describe that representation rather than necessarily matching the original source after DOM mutations.

External schema imports and includes are disabled by default. A configured resolver should only be enabled for trusted schema locations.

## Query model differences

AngleSharp.Xml is centered on AngleSharp DOM operations and selector-based querying. If your architecture requires XPath-first querying, plan for an additional library.

## Entity reference nodes

AngleSharp's core DOM does not expose entity reference nodes. `IXmlDocument.CreateEntityReference` explicitly throws `NotSupportedException`; entity references encountered while parsing are resolved to replacement text instead.

## Error suppression tradeoff

When IsSuppressingErrors is enabled, malformed input may still produce a DOM, but document structure can be surprising. Treat this as recovery mode, not strict validation mode.

## Formatter behavior considerations

Serialization behavior depends on the selected formatter. If deterministic output style is important, explicitly choose XmlMarkupFormatter and configure it instead of relying on auto-selection.

## Canonical XML input scope

Canonical serialization operates on the existing AngleSharp DOM and accepts complete documents or rooted element subtrees. It does not accept arbitrary XPath node sets. Canonical output therefore reflects the declarations, entity replacements, and default attributes materialized by the parser; the current partial DTD implementation may not materialize every default required by a validating XML processor.

## Performance and memory

Like other DOM parsers, full-document parsing keeps an in-memory object graph. For very large inputs, consider chunking or stream-first preprocessing before constructing a full DOM.

## DTD and advanced validation workflows

DTD-related behavior exists but should be evaluated against your own compliance requirements. For regulatory or strict interoperability requirements, run your own conformance test set as part of CI.

## Practical guidance

Use AngleSharp.Xml when you want:

- Strong integration with AngleSharp
- A unified DOM style across HTML / XML / SVG
- Programmatic XML manipulation and serialization

Use additional tooling when you need:

- XML Schema 1.1 validation
- XPath-centric querying
- Specialized industry-specific XML validation stacks
