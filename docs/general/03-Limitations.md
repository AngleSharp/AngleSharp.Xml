---
title: "Limitations"
section: "AngleSharp.Xml"
---
# Limitations and Boundaries

AngleSharp.Xml is designed for practical XML parsing and DOM workflows in the AngleSharp ecosystem. It is not intended to replace every specialized XML stack.

## Not a full XML schema stack

AngleSharp.Xml does not provide a full XSD validation subsystem. If strict schema validation is required, pair it with dedicated validation tools.

## Query model differences

AngleSharp.Xml is centered on AngleSharp DOM operations and selector-based querying. If your architecture requires XPath-first querying, plan for an additional library.

## Error suppression tradeoff

When IsSuppressingErrors is enabled, malformed input may still produce a DOM, but document structure can be surprising. Treat this as recovery mode, not strict validation mode.

## Formatter behavior considerations

Serialization behavior depends on the selected formatter. If deterministic output style is important, explicitly choose XmlMarkupFormatter and configure it instead of relying on auto-selection.

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

- Strict schema validation
- XPath-centric querying
- Specialized industry-specific XML validation stacks
