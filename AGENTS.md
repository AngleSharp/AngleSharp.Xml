# AGENTS.md

## Repository Purpose

AngleSharp.Xml is the AngleSharp ecosystem's XML extension library. It provides:

- XML parsing and serialization
- XML and SVG document integration with AngleSharp BrowsingContext
- An AngleSharp DOM model for XML documents
- XML fragments and asynchronous parsing
- DTD parsing and practical validity checks
- Namespace-aware XML handling

The package is published as `AngleSharp.Xml` and is MIT licensed. The repository is hosted at `https://github.com/AngleSharp/AngleSharp.Xml`.

The public README describes the intended positioning: XML behavior that integrates with AngleSharp's DOM, configuration, loading, SVG, XHTML, and formatting APIs. Do not assume this project should be made equivalent to `System.Xml`; preserve the AngleSharp integration model.

## Current Checkout Facts

- Primary development branch is `devel`.
- The repository currently uses the Fallout build orchestrator, version 10.4.x in the checked-in build tooling.
- The current project version in `src/Directory.Build.props` is `1.2.0`.
- The package's AngleSharp dependency defaults to version `1.5.0` and allows versions below `2.0.0`.
- There is no repository-level AGENTS.md or copilot instruction file other than this one.
- The worktree should be checked with `git status --short` before making assumptions about local changes. Never discard changes that are already present.

## Important Commands

### Fast local test

From the repository root:

```sh
dotnet test src/AngleSharp.Xml.Tests/AngleSharp.Xml.Tests.csproj -v q
```

The tests target `net8.0`. The test project references the library with its `netstandard2.0` target framework, so a test-only run does not necessarily exercise every library target framework.

Run one fixture or test with NUnit adapter filtering:

```sh
dotnet test src/AngleSharp.Xml.Tests/AngleSharp.Xml.Tests.csproj \
  --filter 'FullyQualifiedName~AngleSharp.Xml.Tests.Parser.XmlInvalidDocuments'
```

Use a more specific filter for a single method when debugging. Avoid broad test output when possible; filter or redirect it and inspect the final `Test summary`.

### Build orchestrator

The supported repository build entry points are:

```sh
./build.sh
```

```powershell
.\build.ps1
```

The scripts:

1. Prefer a globally installed `dotnet`.
2. Otherwise install a local SDK under `.fallout/temp`.
3. Restore local .NET tools.
4. Invoke the Fallout build target.

The default local target runs restore, compile, tests, and package creation. The build reads the version from `CHANGELOG.md` and supports parameters such as:

```sh
./build.sh -AngleSharpVersion 1.5.0
./build.sh -Target RunUnitTests
./build.sh -Target Compile
./build.sh -Target Package
```

The exact target names are defined in `build/Build.cs`. Important targets include:

- `Clean`
- `Restore`
- `Compile`
- `RunUnitTests`
- `CreatePackage`
- `Package`
- `Publish`
- `PrePublish`

Publishing requires secrets and must not be attempted casually:

- `NUGET_API_KEY` for NuGet publishing
- `GITHUB_TOKEN` for GitHub release publishing

### Direct project commands

Useful focused commands from the root:

```sh
dotnet build src/AngleSharp.Xml/AngleSharp.Xml.csproj
dotnet build src/AngleSharp.Xml.Tests/AngleSharp.Xml.Tests.csproj
dotnet test src/AngleSharp.Xml.Tests/AngleSharp.Xml.Tests.csproj
```

The solution is `src/AngleSharp.Xml.sln`; it contains the library, tests, and build project. CI uses the repository build scripts rather than direct solution commands.

## CI and Target Frameworks

`.github/workflows/ci.yml` runs on push and pull request.

- Linux uses .NET `10.0.x` and runs `./build.sh -AngleSharpVersion 1.5.0`.
- Windows uses .NET `10.0.x` and runs `build.ps1`.
- Windows publishes on `main`, pre-publishes on `devel`, and performs the default build for other refs.
- Documentation deployment is conditional on repository secrets and the configured docs branch/path.

`src/AngleSharp.Xml/AngleSharp.Xml.csproj` targets:

- `netstandard2.0`
- `net8.0`
- `net10.0`
- `net462` and `net472` on Windows only

The project enables XML documentation generation, strong-name signing, SourceLink, symbols, and package metadata. Warnings are treated as errors through `src/Directory.Build.props`.

## Repository Layout

### Root

- `README.md`: package overview and basic XML setup.
- `CHANGELOG.md`: release notes and source of the build version.
- `CONTRIBUTORS.md`: contributor information.
- `LICENSE`: MIT license.
- `build.sh`, `build.ps1`, `build.cmd`: cross-platform build entry points.
- `build/`: Fallout build project and build targets.
- `docs/`: Markdown documentation.
- `src/`: solution, library, tests, and docs web project.

### Library: `src/AngleSharp.Xml`

Public integration and formatter files:

- `XmlConfigurationExtensions.cs`: `IConfiguration.WithXml()` registration for XML, application/xml, and SVG document factories plus `IXmlParser`.
- `DomImplementationExtension.cs`: XML DOM factory helpers.
- `XmlMarkupFormatter.cs`: XML serialization formatter.
- `AutoSelectedMarkupFormatter.cs` and `MarkupFormatterExtensions.cs`: formatter selection/helpers.
- `XmlEntityProvider.cs`: built-in XML entities (`amp;`, `lt;`, `gt;`, `apos;`, `quot;`).

DOM files:

- `Dom/IXmlDocument.cs`: public `IXmlDocument`, including `IsValid`.
- `Dom/ISvgDocument.cs`: SVG document contract.
- `Dom/Internal/XmlDocument.cs`: internal XML document implementation and validity storage.
- `Dom/Internal/XmlElement.cs`: XML element implementation.
- `Dom/Internal/SvgDocument.cs`: SVG document implementation.
- `Dom/Events/XmlParseEvent.cs`: parser lifecycle event payload.

Parser files:

- `Parser/IXmlParser.cs`: public parser contract for strings, streams, fragments, and async operations.
- `Parser/XmlParser.cs`: parser facade and document creation.
- `Parser/XmlDomBuilder.cs`: tree construction, DOCTYPE handling, DTD validation, and entity expansion.
- `Parser/XmlTokenizer.cs`: XML lexical tokenizer and character/entity reference handling.
- `Parser/XmlParserOptions.cs`: suppress-errors, source-reference, and element-created callback options.
- `Parser/XmlParseError*.cs`: parse error definitions and exception mapping.
- `Parser/Tokens/`: token models for declarations, DOCTYPE, tags, text, CDATA, comments, processing instructions, and EOF.

DTD files:

- `Dtd/Parser/DtdParser.cs`: DTD parser.
- `Dtd/Parser/DtdTokenizer.cs`: DTD lexical/token parsing.
- `Dtd/Parser/DtdPlainTokenizer.cs`: plain external DTD tokenization support.
- `Dtd/Parser/DtdContainer.cs`: parsed DTD declarations/entities and invalid state.
- `Dtd/Declaration/`: element and attribute declaration models.

### Tests: `src/AngleSharp.Xml.Tests`

- `Parser/XmlParsing.cs`: general parser behavior.
- `Parser/XmlNamespace.cs`: namespace behavior.
- `Parser/XmlValidDocuments.cs`: large valid XML conformance fixture suite.
- `Parser/XmlInvalidDocuments.cs`: invalid document/DTD fixture suite.
- `Parser/XmlValidExtDtd.cs`: valid external DTD fixtures.
- `Parser/XmlNotWfDocuments.cs`: not-well-formed XML fixtures.
- `Parser/XmlNotWfExtDtd.cs`: not-well-formed external DTD fixtures.
- `Parser/XmlDtdImplementedCases.cs`: focused, currently supported DTD validation cases.
- `Parser/XmlExternalDtdSupport.cs`: local-file external DTD/entity regression tests.
- `Tokenizer/XmlDTD.cs`: DTD tokenizer/parser samples, including historical commented assertions.
- `Tokenizer/XmlTokenization.cs`: XML tokenizer tests.
- `Dom/`: DOM samples and tree behavior.
- `Xhtml/`: XHTML formatter and preservation tests.
- `Mocks/MockEntityProvider.cs`: entity provider test double.
- `TestExtensions.cs`: test parsing helpers, including permissive conformance fallback behavior.

The tests use NUnit 3, NUnit3TestAdapter, Microsoft.NET.Test.Sdk, and target `net8.0`.

### Documentation

Markdown documentation is under `docs/`:

- `docs/general/01-Basics.md`: setup and NuGet usage.
- `docs/general/02-Capabilities.md`: supported capabilities.
- `docs/general/03-Limitations.md`: project boundaries.
- `docs/tutorials/01-API.md`: parser/configuration APIs.
- `docs/tutorials/02-Examples.md`: code examples.
- `docs/tutorials/03-Use-Cases.md`: practical workflows.
- `docs/tutorials/04-Questions.md`: FAQ.
- `docs/tutorials/05-DTD-Validation.md`: DTD concepts, validation, supported behavior, and limitations.

The docs web project is under `src/AngleSharp.Xml.Docs` and uses Node.js/npm with a TypeScript/React entry point. Its CI deployment is secret-controlled; ordinary library changes generally only need Markdown updates unless the docs web app itself is changed.

## Public Usage Patterns

### Register XML in AngleSharp

```cs
var config = Configuration.Default
    .WithXml();
```

`WithXml()` registers document factories for:

- `text/xml`
- `application/xml`
- SVG content

It also registers an `IXmlParser` service in the browsing context.

### Parse directly

```cs
var parser = new XmlParser();
var document = parser.ParseDocument(xmlText);
```

The parser also supports streams, fragments, and asynchronous overloads. `XmlParserExtensions` supplies cancellation-token-free async convenience methods.

### Parser options

- `IsSuppressingErrors`: recovery mode; the parser attempts to return a document instead of throwing. The resulting DOM may be incomplete or surprising. Do not treat this as strict validation.
- `IsKeepingSourceReferences`: keeps source token references on created elements for diagnostics/tooling.
- `OnCreated`: callback receiving each created element and its `TextPosition`.

### Validity

`IXmlDocument.IsValid` is the library's DTD-related validity signal. It is initialized true and is updated after parsing when applicable. A document can be syntactically parsed but invalid according to its DOCTYPE/DTD declarations.

## DTD Behavior and Boundaries

The DTD implementation is useful but intentionally should not be treated as a complete XML 1.0 validation stack.

Currently supported or partially supported behavior includes:

- DOCTYPE root-name consistency checking.
- Internal subset declarations for common validation cases.
- Common element models such as `ANY`, `EMPTY`, mixed `(#PCDATA|name|...)*`, and simple ordered sequences.
- Internal attribute checks, including undeclared attributes, `#REQUIRED`, and `#FIXED` cases.
- Internal general entity replacement.
- Local file-based external `SYSTEM` subset loading.
- Local external general entity replacement in supported cases.

Known limitations:

- No HTTP/network retrieval for external DTDs/entities.
- No complete PUBLIC identifier or XML catalog resolution workflow.
- Parameter entities and external-subset semantics are not full XML conformance.
- Complex content-model grammar and all quantifier combinations are not guaranteed by fallback validation paths.
- DTD default-value materialization is limited.
- No built-in XSD validation.
- Recovery/conformance test helpers can mask parser gaps and must not be used as evidence of strict parsing.

When changing DTD code, update both focused tests in `XmlDtdImplementedCases.cs`/`XmlExternalDtdSupport.cs` and the DTD documentation if the support boundary changes.

## Parser Change Guidance

The main ownership path for XML behavior is:

1. `XmlTokenizer` recognizes lexical XML constructs.
2. `XmlDomBuilder` consumes tokens and controls tree state.
3. `XmlDomBuilder.ApplyValidation()` sets `XmlDocument.IsValid` after the tree is built.
4. `XmlParser` exposes the builder through public synchronous/asynchronous APIs.

When debugging a behavior:

- If parsing throws before a DOM exists, inspect `XmlTokenizer` and `XmlParseError` first.
- If the DOM shape is wrong, inspect `XmlDomBuilder` state transitions and token consumption.
- If `IsValid` is wrong, inspect DOCTYPE parsing, DTD declarations, `ApplyValidation`, and recursive content/attribute checks.
- If an entity is unresolved, inspect both tokenizer entity lookup and DTD declaration loading.
- If loading through a browsing context fails, inspect `XmlConfigurationExtensions` and document factory registration.

Prefer the smallest change at the controlling layer. Keep strict parsing and recovery parsing separate; do not make production parser behavior permissive just to satisfy a broad conformance fixture.

## Testing Guidance

Use focused tests first, then the full suite:

```sh
dotnet test src/AngleSharp.Xml.Tests/AngleSharp.Xml.Tests.csproj \
  --filter 'FullyQualifiedName~XmlDtdImplementedCases'

dotnet test src/AngleSharp.Xml.Tests/AngleSharp.Xml.Tests.csproj \
  --filter 'FullyQualifiedName~XmlExternalDtdSupport'

dotnet test src/AngleSharp.Xml.Tests/AngleSharp.Xml.Tests.csproj -v q
```

For parser changes, include tests for:

- Strict well-formed input.
- Strict malformed input that must throw.
- Valid DTD input and invalid DTD input through `IsValid`.
- Internal and external entity behavior where relevant.
- Recovery mode only when recovery behavior is explicitly the subject.

Conformance fixture suites are broad and include historical XML test cases that depend on external resources, complex DTD features, or behavior not fully implemented in this repository. Treat `ToXmlDocumentConformance` and similar helpers as compatibility/recovery harnesses, not as a replacement for strict parser assertions.

## Coding and Editing Conventions

- C# uses four spaces; project files use two spaces.
- Repository line endings are LF and files use UTF-8.
- Warnings are errors for the library build.
- Preserve existing public APIs and AngleSharp patterns.
- Avoid unrelated formatting or refactoring.
- Use explicit, descriptive variable names; follow the surrounding style.
- Keep comments concise and explanatory only where needed.
- Do not add license headers.
- Do not commit, reset, checkout, or create branches unless explicitly requested.
- Do not remove user changes from a dirty worktree.
- Keep generated `bin/`, `obj/`, `.fallout/`, and test-result artifacts out of source changes unless the task explicitly concerns them.

## Common Pitfalls

- The test project uses `netstandard2.0` for its library project reference even though the tests run on `net8.0`.
- Windows-only target frameworks are conditionally added based on `OS == Windows_NT`; do not assume Linux can build `net462`/`net472`.
- The build version comes from `CHANGELOG.md`, not only the csproj version property.
- `WithXml()` is required for BrowsingContext XML/SVG loading; direct `XmlParser` construction is separate.
- XML entity names passed to the built-in provider include the trailing semicolon (`amp;`, not `amp`).
- `IsSuppressingErrors` changes control flow and can produce a non-null but structurally unreliable document.
- A passing broad conformance helper test does not prove strict XML conformance.
- External DTD support is local-file-oriented and must not be described as network/catalog-aware.
- DTD `IsValid` is not XSD validation and does not replace domain-specific validation.
- The docs index and DTD support matrix should be updated when DTD behavior changes.

## Preferred Change Workflow

1. Check `git status --short` and identify the nearest file/symbol/test.
2. Read the controlling code path and one neighboring test.
3. State a falsifiable local hypothesis and choose the cheapest focused test.
4. Make the smallest edit with the repository's existing style.
5. Run the focused test immediately.
6. Repair only the same local slice if it fails; do not broaden prematurely.
7. Run the relevant fixture or project test suite.
8. Run the full test suite when shared parser, tokenizer, DOM, DTD, or build behavior changed.
9. Update docs and tests when public behavior or support boundaries change.
10. Report exact validation commands and any remaining limitations.
