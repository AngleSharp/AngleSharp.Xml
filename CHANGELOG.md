# 1.3.0

Released on Sunday, November 1 2026.

- Updated to use a minimum of AngleSharp 1.9
- Improved performance of `TextContent` on XML elements, which no longer allocates for an element holding a single text or CDATA section
- Improved performance of DTD validation: content models are parsed once per document, and documents without ID declarations skip the identity walk
- Fixed parsing deeply nested documents overflowing the stack and ending the process; the DTD and identity validation walks now use an explicit stack
- Fixed `TextContent` of deeply nested XML elements overflowing the stack
- Fixed escaped text that looks like a CDATA section (`&lt;![CDATA[...]]&gt;`) losing its markers in documents that declare entities, which also took quadratic time; only the CDATA sections in an entity's replacement text are unwrapped now
- Fixed unbounded expansion of nested entities ("billion laughs"), which let a small document exhaust memory; expansion now stops at `XmlParserOptions.MaxCharactersFromEntities`
- Added trimming and Native AOT compatibility (`IsAotCompatible`) for the .NET 8 and .NET 10 targets
- Added `XmlParserOptions.MaxCharactersFromEntities` (default 10,000,000, 0 for no limit), like `System.Xml`'s setting of the same name

# 1.2.0

Released on Friday, August 21 2026.

- Updated to complete DTD identity and reference validation semantics (#31)
- Improved preservation of CDATA sections as first-class DOM nodes (#30)
- Added the XML-specific `CDATA` factory (#29)
- Added support for XML document metadata (#29)
- Added Canonical XML 1.1 and Exclusive XML Canonicalization 1.0 serialization (#34)
- Added XML Base, `xml:id`, and inherited `xml:lang` convenience semantics (#33)
- Added optional XSD 1.0 document validation with diagnostics (#35)

# 1.1.0

Released on Friday, July 31 2026.

- Updated to use a minimum of AngleSharp 1.5
- Fixed serialization of self-closing in case of children (#27)
- Fixed namespace declaration processing (#22) @jbrayfaithlife
- Added optional DTD validation

# 1.0.0

Released on Sunday, January 15 2023.

- Updated to use AngleSharp 1.0

# 0.17.0

Released on Tuesday, May 31 2022.

- Updated to use AngleSharp 0.17
- Dropped .NET Framework 4.6
- Fixed parser ignoring IsKeepingSourceReferences (#16)

# 0.16.0

Released on Sunday, June 13 2021.

- Updated to use AngleSharp 0.16

# 0.15.0

Released on Thursday, April 22 2021.

- Dropped support for .NET Standard 1.3 target
- Fixed breaking parsing upon invalid syntax when suppressing errors (#14)

# 0.14.0

Released on Tuesday, March 31 2020.

- Added more overloads for `ToXml` and `ToMarkup`
- Allow transforming all empty elements to self closing tags
- Serialize self-closing tags correctly (#11)
- Updated the `IMarkupFormatter` implementations
- Added shallow support for source link
- Support more target frameworks

# 0.13.0

Released on Friday, September 6 2019.

- Added `ToXml` and `ToMarkup` extensions for the formatting

# 0.12.1

Released on Wednesday, May 15 2019.

- Binary version fix
- Placed `WithXml` in the AngleSharp namespace (#4)

# 0.12.0

Released on Thursday, May 2 2019.

- Reference latest AngleSharp
- Improved `InnerHtml` / XML fragment parsing (#1)

# 0.11.0

Released on Tuesday, February 12 2019.

- Initial release
