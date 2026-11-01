namespace AngleSharp.Xml.Parser
{
    using AngleSharp.Xml.Dtd.Declaration;
    using AngleSharp.Xml.Dtd.Parser;
    using AngleSharp.Dom;
    using AngleSharp.Text;
    using AngleSharp.Xml.Dom;
    using AngleSharp.Xml.Parser.Tokens;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text.RegularExpressions;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Represents the Tree construction as specified in the official W3C
    /// specification for XML:
    /// http://www.w3.org/TR/REC-xml/
    /// </summary>
    sealed class XmlDomBuilder
    {
        #region Fields

        private readonly XmlTokenizer _tokenizer;
        private readonly Document _document;
        private readonly List<Element> _openElements;
        private readonly Dictionary<String, String> _internalElementModels;
        private readonly Dictionary<String, HashSet<String>> _internalAttributeDeclarations;
        private readonly Dictionary<String, Dictionary<String, InternalAttributeRule>> _internalAttributeRules;
        private readonly Dictionary<String, String> _internalGeneralEntities;
        private readonly Dictionary<String, String> _internalGeneralEntityContents;
        private readonly Dictionary<String, String> _fallbackUnparsedEntities;
        private readonly HashSet<String> _fallbackNotations;
        private DtdContainer _dtd;
        private String _doctypeName;

        private XmlParserOptions _options;
        private XmlTreeMode _currentMode;
        private Boolean _standalone;

        #endregion

        #region ctor

        /// <summary>
        /// Creates a new instance of the XML parser.
        /// </summary>
        /// <param name="document">The document instance to be filled.</param>
        internal XmlDomBuilder(Document document)
        {
            _tokenizer = new XmlTokenizer(document.Source, document.Entities);
            _document = document;
            _standalone = false;
            _openElements = new List<Element>();
            _internalElementModels = new Dictionary<String, String>(StringComparer.Ordinal);
            _internalAttributeDeclarations = new Dictionary<String, HashSet<String>>(StringComparer.Ordinal);
            _internalAttributeRules = new Dictionary<String, Dictionary<String, InternalAttributeRule>>(StringComparer.Ordinal);
            _internalGeneralEntities = new Dictionary<String, String>(StringComparer.Ordinal);
            _internalGeneralEntityContents = new Dictionary<String, String>(StringComparer.Ordinal);
            _fallbackUnparsedEntities = new Dictionary<String, String>(StringComparer.Ordinal);
            _fallbackNotations = new HashSet<String>(StringComparer.Ordinal);
            _currentMode = XmlTreeMode.Initial;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets if the document was detected to be standalone.
        /// </summary>
        public Boolean IsStandalone => _standalone;

        /// <summary>
        /// Gets the current node.
        /// </summary>
        public Node CurrentNode
        {
            get
            {
                if (_openElements.Count > 0)
                {
                    return _openElements[_openElements.Count - 1];
                }

                return _document;
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Parses the given source asynchronously and creates the document.
        /// </summary>
        /// <param name="options">The options to use for parsing.</param>
        /// <param name="cancelToken">The cancellation token to use.</param>
        public async Task<Document> ParseAsync(XmlParserOptions options, CancellationToken cancelToken)
        {
            var source = _document.Source;
            var token = default(XmlToken);
            _options = options;
            _tokenizer.IsSuppressingErrors = options.IsSuppressingErrors;
            _tokenizer.MaxCharactersFromEntities = options.MaxCharactersFromEntities;

            do
            {
                if (source.Length - source.Index < 1024)
                {
                    await source.PrefetchAsync(8192, cancelToken).ConfigureAwait(false);
                }

                token = _tokenizer.Get();
                Consume(token);
            }
            while (token.Type != XmlTokenType.EndOfFile);

            ApplyValidation();

            return _document;
        }

        /// <summary>
        /// Parses the given source and creates the document.
        /// </summary>
        /// <param name="options">The options to use for parsing.</param>
        public Document Parse(XmlParserOptions options)
        {
            var token = default(XmlToken);
            _options = options;
            _tokenizer.IsSuppressingErrors = options.IsSuppressingErrors;
            _tokenizer.MaxCharactersFromEntities = options.MaxCharactersFromEntities;

            do
            {
                token = _tokenizer.Get();
                Consume(token);
            }
            while (token.Type != XmlTokenType.EndOfFile);

            ApplyValidation();

            return _document;
        }

        /// <summary>
        /// Switches to the fragment algorithm with the specified context
        /// element. Then parses the given source and creates the document.
        /// </summary>
        /// <param name="options">The options to use for parsing.</param>
        /// <param name="context">
        /// The context element where the algorithm is applied to.
        /// </param>
        public Document ParseFragment(XmlParserOptions options, Element context)
        {
            context = context ?? throw new ArgumentNullException(nameof(context));
            var root = new XmlElement(_document, context.LocalName);
            _document.AddNode(root);
            _openElements.Add(root);
            return Parse(options);
        }

        #endregion

        #region States

        /// <summary>
        /// Consumes a token and processes it.
        /// </summary>
        /// <param name="token">The token to consume.</param>
        private void Consume(XmlToken token)
        {
            switch (_currentMode)
            {
                case XmlTreeMode.Initial:
                    Initial(token);
                    break;
                case XmlTreeMode.Prolog:
                    BeforeDoctype(token);
                    break;
                case XmlTreeMode.Misc:
                    InMisc(token);
                    break;
                case XmlTreeMode.Body:
                    InBody(token);
                    break;
                case XmlTreeMode.After:
                    AfterBody(token);
                    break;
            }
        }

        /// <summary>
        /// The initial state. Expects an XML declaration.
        /// </summary>
        /// <param name="token">The consumed token.</param>
        private void Initial(XmlToken token)
        {
            if (token.Type == XmlTokenType.Declaration)
            {
                var declarationToken = (XmlDeclarationToken)token;
                _standalone = declarationToken.Standalone;

                if (_document is XmlDocument xmlDocument)
                {
                    xmlDocument.SetDeclaration(
                        declarationToken.Version,
                        declarationToken.IsEncodingMissing ? null : declarationToken.Encoding,
                        declarationToken.Standalone);
                }

                if (!declarationToken.IsEncodingMissing)
                {
                    SetEncoding(declarationToken.Encoding);
                }

                if (!CheckVersion(declarationToken.Version) && !_options.IsSuppressingErrors)
                {
                    throw XmlParseError.XmlDeclarationVersionUnsupported.At(token.Position);
                }
            }
            else
            {
                _currentMode = XmlTreeMode.Prolog;
                BeforeDoctype(token);
            }
        }

        /// <summary>
        /// Before any doctype - still in the prolog. No declaration
        /// allowed.
        /// </summary>
        /// <param name="token">The consumed token.</param>
        private void BeforeDoctype(XmlToken token)
        {
            switch (token.Type)
            {
                case XmlTokenType.Doctype:
                {
                    var doctypeToken = (XmlDoctypeToken)token;
                    var doctypeNode = _document.Implementation.CreateDocumentType(doctypeToken.Name, doctypeToken.PublicIdentifier, doctypeToken.SystemIdentifier);
                    _document.AppendChild(doctypeNode);
                    _doctypeName = doctypeToken.Name;

                    ParseDoctypeSubset(doctypeToken);

                    _currentMode = XmlTreeMode.Misc;

                    break;
                }
                default:
                {
                    InMisc(token);
                    break;
                }
            }
        }

        /// <summary>
        /// In the body state - no doctypes and declarations allowed.
        /// </summary>
        /// <param name="token">The consumed token.</param>
        private void InMisc(XmlToken token)
        {
            switch (token.Type)
            {
                case XmlTokenType.Comment:
                {
                    var commenToken = (XmlCommentToken)token;
                    var commentNode = _document.CreateComment(commenToken.Data);
                    CurrentNode.AppendChild(commentNode);
                    break;
                }
                case XmlTokenType.ProcessingInstruction:
                {
                    var piToken = (XmlPIToken)token;
                    var piNode = _document.CreateProcessingInstruction(piToken.Target, piToken.Content);
                    CurrentNode.AppendChild(piNode);
                    break;
                }
                case XmlTokenType.StartTag:
                {
                    _currentMode = XmlTreeMode.Body;
                    InBody(token);
                    break;
                }
                case XmlTokenType.Character:
                {
                    var charToken = (XmlCharacterToken)token;

                    if (charToken.IsReferenceSource && !_options.IsSuppressingErrors)
                    {
                        throw XmlParseError.XmlMissingRoot.At(token.Position);
                    }

                    if (!charToken.IsIgnorable && !_options.IsSuppressingErrors)
                    {
                        throw XmlParseError.XmlMissingRoot.At(token.Position);
                    }

                    break;
                }
                default:
                {
                    if (!token.IsIgnorable && !_options.IsSuppressingErrors)
                    {
                        throw XmlParseError.XmlMissingRoot.At(token.Position);
                    }

                    break;
                }
            }
        }

        /// <summary>
        /// In the body state - no doctypes and declarations allowed.
        /// </summary>
        /// <param name="token">The consumed token.</param>
        private void InBody(XmlToken token)
        {
            switch (token.Type)
            {
                case XmlTokenType.StartTag:
                {
                    var tagToken = (XmlTagToken)token;
                    var element = CreateElement(tagToken.Name, tagToken.IsSelfClosing);
                    CurrentNode.AppendChild(element);

                    var namespaceDeclarations = tagToken.Attributes
                        .Where(attr => attr.Key.StartsWith("xmlns"))
                        .ToList();
                    var otherAttributes = tagToken.Attributes
                        .Where(attr => !attr.Key.StartsWith("xmlns"))
                        .ToList();

                    for (var i = 0; i < namespaceDeclarations.Count; i++)
                    {
                        var attr = namespaceDeclarations[i];
                        var item = CreateAttribute(attr.Key, attr.Value.Trim());
                        element.AddAttribute(item);
                    }

                    for (var i = 0; i < otherAttributes.Count; i++)
                    {
                        var attr = otherAttributes[i];
                        var item = CreateAttribute(attr.Key, attr.Value.Trim());

                        if (item.NamespaceUri == NamespaceNames.XmlUri && item.LocalName == "id")
                        {
                            item.Value = XmlElementExtensions.NormalizeXmlId(item.Value);
                        }

                        element.AddAttribute(item);
                    }

                    if (!tagToken.IsSelfClosing)
                    {
                        _openElements.Add(element);
                    }
                    else if (_openElements.Count == 0)
                    {
                        _currentMode = XmlTreeMode.After;
                    }

                    if (_options.IsKeepingSourceReferences)
                    {
                        element.SourceReference = tagToken;
                    }

                    if (_options.OnCreated != null)
                    {
                        _options.OnCreated.Invoke(element, tagToken.Position);
                    }

                    break;
                }
                case XmlTokenType.EndTag:
                {
                    var tagToken = (XmlTagToken)token;

                    if (!CurrentNode.NodeName.Is(tagToken.Name))
                    {
                        if (_options.IsSuppressingErrors)
                        {
                            break;
                        }

                        throw XmlParseError.TagClosingMismatch.At(token.Position);
                    }

                    _openElements.RemoveAt(_openElements.Count - 1);

                    if (_openElements.Count == 0)
                    {
                        _currentMode = XmlTreeMode.After;
                    }

                    break;
                }
                case XmlTokenType.ProcessingInstruction:
                case XmlTokenType.Comment:
                {
                    InMisc(token);
                    break;
                }
                case XmlTokenType.CData:
                {
                    var cdataToken = (XmlCDataToken)token;

                    if (_document is IXmlDocument xmlDocument)
                    {
                        CurrentNode.AppendChild(xmlDocument.CreateCDataSection(cdataToken.Data));
                    }
                    else
                    {
                        CurrentNode.AppendText(cdataToken.Data);
                    }

                    break;
                }
                case XmlTokenType.Character:
                {
                    var charToken = (XmlCharacterToken)token;
                    CurrentNode.AppendText(ExpandInternalEntities(charToken.Data, charToken.Position));
                    break;
                }
                case XmlTokenType.EndOfFile:
                {
                    if (_options.IsSuppressingErrors)
                    {
                        break;
                    }

                    throw XmlParseError.EOF.At(token.Position);
                }
                case XmlTokenType.Doctype:
                {
                    if (_options.IsSuppressingErrors)
                    {
                        break;
                    }

                    throw XmlParseError.XmlDoctypeAfterContent.At(token.Position);
                }
                case XmlTokenType.Declaration:
                {
                    if (_options.IsSuppressingErrors)
                    {
                        break;
                    }

                    throw XmlParseError.XmlDeclarationMisplaced.At(token.Position);
                }
            }
        }

        /// <summary>
        /// After the body state - nothing except Comment PI S allowed.
        /// </summary>
        /// <param name="token">The consumed token.</param>
        private void AfterBody(XmlToken token)
        {
            switch (token.Type)
            {
                case XmlTokenType.ProcessingInstruction:
                case XmlTokenType.Comment:
                {
                    InMisc(token);
                    break;
                }
                case XmlTokenType.EndOfFile:
                {
                    break;
                }
                default:
                {
                    if (!token.IsIgnorable && !_options.IsSuppressingErrors)
                    {
                        throw XmlParseError.XmlMissingRoot.At(token.Position);
                    }

                    break;
                }
            }
        }

        #endregion

        #region Helpers

        private Element CreateElement(String name, Boolean selfClosing)
        {
            var prefix = default(String);
            var colon = name.IndexOf(Symbols.Colon);
            var flags = selfClosing ? NodeFlags.SelfClosing : NodeFlags.None;

            if (colon > 0 && colon < name.Length - 1)
            {
                prefix = name.Substring(0, colon);
                name = name.Substring(colon + 1);
            }

            return _document.CreateElementFrom(name, prefix, flags);
        }

        private Attr CreateAttribute(String name, String value)
        {
            var colon = name.IndexOf(Symbols.Colon);

            if (colon > 0 && colon < name.Length - 1)
            {
                var prefix = name.Substring(0, colon);
                var ns = NamespaceNames.XmlNsUri;

                if (prefix.Is(NamespaceNames.XmlPrefix))
                {
                    ns = NamespaceNames.XmlUri;
                }
                else if (!prefix.Is(NamespaceNames.XmlNsPrefix))
                {
                    ns = CurrentNode.LookupNamespaceUri(prefix);
                }

                return new Attr(prefix, name.Substring(colon + 1), value, ns);
            }

            return new Attr(name, value);
        }

        private static Boolean CheckVersion(String ver)
        {
            var t = ver.ToDouble(0.0);
            return t >= 1.0 && t < 2.0;
        }

        private void ParseDoctypeSubset(XmlDoctypeToken doctypeToken)
        {
            var subset = doctypeToken.InternalSubset ?? String.Empty;

            _dtd = null;
            _internalElementModels.Clear();
            _internalAttributeDeclarations.Clear();
            _internalAttributeRules.Clear();
            _internalGeneralEntities.Clear();
            _internalGeneralEntityContents.Clear();
            _fallbackUnparsedEntities.Clear();
            _fallbackNotations.Clear();

            var hasExternalSubset = TryLoadExternalSubset(doctypeToken.SystemIdentifier, out var externalSubset, out var externalSubsetPath);

            if (hasExternalSubset || subset.Length > 0)
            {
                var container = new DtdContainer
                {
                    Parent = _document as XmlDocument,
                    Url = externalSubsetPath,
                };

                if (hasExternalSubset)
                {
                    try
                    {
                        var externalParser = new DtdParser(container, new TextSource(externalSubset));
                        externalParser.IsInternal = false;
                        externalParser.Parse();
                    }
                    catch
                    {
                    }
                }

                if (subset.Length > 0)
                {
                    try
                    {
                        var internalParser = new DtdParser(container, new TextSource(subset))
                        {
                            IsInternal = true,
                        };

                        internalParser.Parse();
                    }
                    catch
                    {
                    }
                }

                _dtd = container;

                foreach (var entity in container.Entities)
                {
                    if (!_internalGeneralEntities.ContainsKey(entity.NodeName) && !String.IsNullOrEmpty(entity.NodeValue))
                    {
                        _internalGeneralEntities[entity.NodeName] = entity.NodeValue;
                    }
                }
            }

            if (hasExternalSubset)
            {
                ParseSubsetFallbackDeclarations(externalSubset);
            }

            ParseSubsetFallbackDeclarations(subset);

            RegisterTokenizerEntities();
        }

        private void ParseSubsetFallbackDeclarations(String subset)
        {
            if (String.IsNullOrEmpty(subset))
            {
                return;
            }

            foreach (Match match in Regex.Matches(subset, "<!ELEMENT\\s+([A-Za-z_][A-Za-z0-9_:\\.-]*)\\s+([^>]+)>", RegexOptions.Singleline))
            {
                var name = match.Groups[1].Value;
                var model = match.Groups[2].Value.Trim();
                _internalElementModels[name] = model;
            }

            foreach (Match match in Regex.Matches(subset, "<!ATTLIST\\s+([A-Za-z_][A-Za-z0-9_:\\.-]*)\\s+(.+?)>", RegexOptions.Singleline))
            {
                var elementName = match.Groups[1].Value;
                var declarationBody = match.Groups[2].Value;

                if (!_internalAttributeDeclarations.TryGetValue(elementName, out var declared))
                {
                    declared = new HashSet<String>(StringComparer.Ordinal);
                    _internalAttributeDeclarations[elementName] = declared;
                }

                if (!_internalAttributeRules.TryGetValue(elementName, out var rules))
                {
                    rules = new Dictionary<String, InternalAttributeRule>(StringComparer.Ordinal);
                    _internalAttributeRules[elementName] = rules;
                }

                foreach (Match attr in Regex.Matches(
                    declarationBody,
                    "([A-Za-z_][A-Za-z0-9_:\\.-]*)\\s+(CDATA|IDREFS?|ID|NMTOKENS?|NMTOKEN|ENTITY|ENTITIES|NOTATION\\s*\\([^\\)]*\\)|\\([^\\)]*\\))\\s+(#REQUIRED|#IMPLIED|#FIXED\\s+((?:\"[^\"]*\"|'[^']*'))|(?:\"[^\"]*\"|'[^']*'))",
                    RegexOptions.Singleline))
                {
                    var attributeName = attr.Groups[1].Value;
                    var defaultDeclaration = attr.Groups[3].Value;

                    declared.Add(attributeName);

                    var rule = new InternalAttributeRule
                    {
                        IsRequired = defaultDeclaration.StartsWith("#REQUIRED", StringComparison.Ordinal),
                        Type = GetIdentityType(attr.Groups[2].Value),
                        IsDefaultAllowedForId = defaultDeclaration.StartsWith("#REQUIRED", StringComparison.Ordinal) ||
                            defaultDeclaration.StartsWith("#IMPLIED", StringComparison.Ordinal),
                        NotationNames = GetNotationNames(attr.Groups[2].Value),
                    };

                    if (defaultDeclaration.StartsWith("#FIXED", StringComparison.Ordinal))
                    {
                        var fixedValue = attr.Groups[4].Success ? attr.Groups[4].Value : String.Empty;
                        rule.FixedValue = Unquote(fixedValue);
                    }

                    rules[attributeName] = rule;
                }
            }

            foreach (Match match in Regex.Matches(subset, "<!ENTITY\\s+([A-Za-z_][A-Za-z0-9_:\\.-]*)\\s+(\"[^\"]*\"|'[^']*')\\s*>", RegexOptions.Singleline))
            {
                var entityName = match.Groups[1].Value;

                if (_internalGeneralEntities.ContainsKey(entityName))
                {
                    continue;
                }

                var quoted = match.Groups[2].Value;
                _internalGeneralEntities[entityName] = quoted.Substring(1, quoted.Length - 2);
            }

            foreach (Match match in Regex.Matches(
                subset,
                "<!ENTITY\\s+([A-Za-z_][A-Za-z0-9_:\\.-]*)\\s+(?:SYSTEM\\s+(?:\"[^\"]*\"|'[^']*')|PUBLIC\\s+(?:\"[^\"]*\"|'[^']*')\\s+(?:\"[^\"]*\"|'[^']*'))\\s+NDATA\\s+([A-Za-z_][A-Za-z0-9_:\\.-]*)\\s*>",
                RegexOptions.Singleline))
            {
                _fallbackUnparsedEntities[match.Groups[1].Value] = match.Groups[2].Value;
            }

            foreach (Match match in Regex.Matches(subset, "<!NOTATION\\s+([A-Za-z_][A-Za-z0-9_:\\.-]*)\\s+", RegexOptions.Singleline))
            {
                _fallbackNotations.Add(match.Groups[1].Value);
            }
        }

        private void RegisterTokenizerEntities()
        {
            foreach (var entry in _internalGeneralEntities)
            {
                _tokenizer.RegisterGeneralEntity(entry.Key, entry.Value);

                // In element content an entity's CDATA sections contribute their content; unwrapping
                // the declared text instead of the expanded text keeps escaped text such as
                // "&lt;![CDATA[" as it is, and costs one pass per declaration
                _internalGeneralEntityContents[entry.Key] = XmlTokenizer.UnwrapCDataSections(entry.Value);
            }
        }

        private Boolean TryLoadExternalSubset(String systemIdentifier, out String content, out String fullPath)
        {
            content = String.Empty;
            fullPath = String.Empty;

            if (String.IsNullOrWhiteSpace(systemIdentifier))
            {
                return false;
            }

            if (!TryResolveSystemPath(systemIdentifier, out fullPath))
            {
                return false;
            }

            try
            {
                content = File.ReadAllText(fullPath);
                return true;
            }
            catch
            {
                content = String.Empty;
                fullPath = String.Empty;
                return false;
            }
        }

        private Boolean TryResolveSystemPath(String systemIdentifier, out String fullPath)
        {
            fullPath = String.Empty;

            try
            {
                if (Path.IsPathRooted(systemIdentifier) && File.Exists(systemIdentifier))
                {
                    fullPath = Path.GetFullPath(systemIdentifier);
                    return true;
                }

                if (Uri.TryCreate(systemIdentifier, UriKind.Absolute, out var uri) && uri.IsFile)
                {
                    var localPath = uri.LocalPath;

                    if (File.Exists(localPath))
                    {
                        fullPath = Path.GetFullPath(localPath);
                        return true;
                    }
                }

                var baseDirectory = Environment.CurrentDirectory;
                var combinedPath = Path.Combine(baseDirectory, systemIdentifier);

                if (File.Exists(combinedPath))
                {
                    fullPath = Path.GetFullPath(combinedPath);
                    return true;
                }
            }
            catch
            {
            }

            return false;
        }

        private void ApplyValidation()
        {
            var xml = _document as XmlDocument;

            if (xml == null)
            {
                return;
            }

            var valid = true;
            var root = _document.DocumentElement as Element;

            if (valid && root != null && !String.IsNullOrEmpty(_doctypeName))
            {
                valid = String.Equals(root.NodeName, _doctypeName, StringComparison.Ordinal);
            }

            var hasDtdElementDeclarations = _dtd != null && _dtd.Elements.Any();

            if (valid && root != null && hasDtdElementDeclarations)
            {
                valid = !_dtd.IsInvalid && ValidateElementsAgainstDtd(root);
            }

            if (valid && root != null && _internalElementModels.Count > 0)
            {
                var contentModels = new Dictionary<String, ContentModel>(StringComparer.Ordinal);

                foreach (var pair in _internalElementModels)
                {
                    contentModels.Add(pair.Key, ContentModel.Parse(pair.Value));
                }

                valid = ValidateElements(root, element => ValidateElementAgainstInternalSubset(element, contentModels));
            }

            if (valid && root != null)
            {
                valid = ValidateIdentityAndReferences(root);
            }

            xml.SetValidity(valid);
        }

        private Boolean ValidateIdentityAndReferences(Element root)
        {
            var declarations = GetIdentityDeclarations();
            var notationNames = new HashSet<String>(_fallbackNotations, StringComparer.Ordinal);
            var unparsedEntities = new Dictionary<String, String>(_fallbackUnparsedEntities, StringComparer.Ordinal);

            if (_dtd != null)
            {
                foreach (var notation in _dtd.Notations)
                {
                    notationNames.Add(notation.NodeName);
                }

                foreach (var entity in _dtd.Entities)
                {
                    if (!String.IsNullOrEmpty(entity.NotationName))
                    {
                        unparsedEntities[entity.NodeName] = entity.NotationName;
                    }
                }
            }

            foreach (var notationName in unparsedEntities.Values)
            {
                if (!notationNames.Contains(notationName))
                {
                    return false;
                }
            }

            foreach (var elementDeclarations in declarations.Values)
            {
                var idDeclarations = elementDeclarations.Count(m => m.Type == IdentityAttributeType.ID);
                var notationDeclarations = elementDeclarations.Count(m => m.Type == IdentityAttributeType.NOTATION);

                if (idDeclarations > 1 || notationDeclarations > 1 ||
                    elementDeclarations.Any(m => m.Type == IdentityAttributeType.ID && !m.IsDefaultAllowedForId) ||
                    elementDeclarations.Any(m => m.Type == IdentityAttributeType.NOTATION && m.NotationNames.Any(n => !notationNames.Contains(n))))
                {
                    return false;
                }
            }

            // Without identity declarations no element has anything to check, rewrite or reference,
            // so walking the tree (as every document without a DOCTYPE would) cannot change the result
            if (declarations.Count == 0)
            {
                return true;
            }

            var ids = new HashSet<String>(StringComparer.Ordinal);
            var references = new List<String>();

            if (!ValidateElements(root, element => ValidateIdentityAttributes(element, declarations, ids, references, unparsedEntities)))
            {
                return false;
            }

            return references.All(ids.Contains);
        }

        /// <summary>
        /// Applies the validation to the root and each descendant element in document order,
        /// stopping at the first failure. Walks with an explicit stack, so deeply nested
        /// documents cannot overflow the call stack.
        /// </summary>
        private static Boolean ValidateElements(Element root, Func<Element, Boolean> validate)
        {
            var pending = new Stack<Element>();
            pending.Push(root);

            while (pending.Count > 0)
            {
                var element = pending.Pop();

                if (!validate(element))
                {
                    return false;
                }

                var children = ((INode)element).ChildNodes;

                for (var i = children.Length - 1; i >= 0; i--)
                {
                    if (children[i] is Element nested)
                    {
                        pending.Push(nested);
                    }
                }
            }

            return true;
        }

        private Boolean ValidateIdentityAttributes(
            Element element,
            Dictionary<String, List<IdentityAttributeDeclaration>> declarations,
            HashSet<String> ids,
            List<String> references,
            Dictionary<String, String> unparsedEntities)
        {
            if (declarations.TryGetValue(element.NodeName, out var elementDeclarations))
            {
                foreach (var declaration in elementDeclarations)
                {
                    var value = element.GetAttribute(declaration.Name);

                    if (value == null)
                    {
                        continue;
                    }

                    var tokens = SplitTokens(value);

                    if (tokens.Count == 0 || tokens.Any(token => !IsXmlName(token)))
                    {
                        return false;
                    }

                    var normalizedValue = String.Join(" ", tokens);

                    if (!String.Equals(value, normalizedValue, StringComparison.Ordinal))
                    {
                        ((IElement)element).Attributes[declaration.Name].Value = normalizedValue;
                    }

                    switch (declaration.Type)
                    {
                        case IdentityAttributeType.ID:
                            if (tokens.Count != 1 || !ids.Add(tokens[0]))
                            {
                                return false;
                            }

                            ((XmlElement)element).IdAttribute = declaration.Name;
                            break;
                        case IdentityAttributeType.IDREF:
                            if (tokens.Count != 1)
                            {
                                return false;
                            }

                            references.Add(tokens[0]);
                            break;
                        case IdentityAttributeType.IDREFS:
                            references.AddRange(tokens);
                            break;
                        case IdentityAttributeType.ENTITY:
                            if (tokens.Count != 1 || !unparsedEntities.ContainsKey(tokens[0]))
                            {
                                return false;
                            }

                            break;
                        case IdentityAttributeType.ENTITIES:
                            if (tokens.Any(token => !unparsedEntities.ContainsKey(token)))
                            {
                                return false;
                            }

                            break;
                    }
                }
            }

            return true;
        }

        private Dictionary<String, List<IdentityAttributeDeclaration>> GetIdentityDeclarations()
        {
            var result = new Dictionary<String, List<IdentityAttributeDeclaration>>(StringComparer.Ordinal);

            if (_dtd != null)
            {
                foreach (var declaration in _dtd.Attributes)
                {
                    foreach (var entry in declaration.Declarations)
                    {
                        var item = CreateIdentityDeclaration(entry);

                        if (item != null)
                        {
                            AddIdentityDeclaration(result, declaration.Name, item);
                        }
                    }
                }
            }

            foreach (var elementRules in _internalAttributeRules)
            {
                foreach (var rule in elementRules.Value)
                {
                    if (rule.Value.Type != IdentityAttributeType.None &&
                        !ContainsIdentityDeclaration(result, elementRules.Key, rule.Key))
                    {
                        AddIdentityDeclaration(result, elementRules.Key, new IdentityAttributeDeclaration
                        {
                            Name = rule.Key,
                            Type = rule.Value.Type,
                            IsDefaultAllowedForId = rule.Value.IsDefaultAllowedForId,
                            NotationNames = rule.Value.NotationNames,
                        });
                    }
                }
            }

            return result;
        }

        private static IdentityAttributeDeclaration CreateIdentityDeclaration(AttributeDeclarationEntry entry)
        {
            if (entry.Type is AttributeTokenizedType tokenized)
            {
                return new IdentityAttributeDeclaration
                {
                    Name = entry.Name,
                    Type = (IdentityAttributeType)Enum.Parse(typeof(IdentityAttributeType), tokenized.Value.ToString()),
                    IsDefaultAllowedForId = entry.Default is AttributeImpliedValue || entry.Default is AttributeRequiredValue,
                };
            }

            if (entry.Type is AttributeEnumeratedType enumerated && enumerated.IsNotation)
            {
                return new IdentityAttributeDeclaration
                {
                    Name = entry.Name,
                    Type = IdentityAttributeType.NOTATION,
                    NotationNames = enumerated.Names,
                };
            }

            return null;
        }

        private static void AddIdentityDeclaration(
            Dictionary<String, List<IdentityAttributeDeclaration>> declarations,
            String elementName,
            IdentityAttributeDeclaration declaration)
        {
            if (!declarations.TryGetValue(elementName, out var items))
            {
                items = new List<IdentityAttributeDeclaration>();
                declarations[elementName] = items;
            }

            items.Add(declaration);
        }

        private static Boolean ContainsIdentityDeclaration(
            Dictionary<String, List<IdentityAttributeDeclaration>> declarations,
            String elementName,
            String attributeName) =>
            declarations.TryGetValue(elementName, out var items) && items.Any(m => m.Name == attributeName);

        private static List<String> SplitTokens(String value) => value
            .Split((Char[])null, StringSplitOptions.RemoveEmptyEntries)
            .ToList();

        private static Boolean IsXmlName(String value)
        {
            if (String.IsNullOrEmpty(value) || !value[0].IsXmlNameStart())
            {
                return false;
            }

            for (var i = 1; i < value.Length; i++)
            {
                if (!value[i].IsXmlName())
                {
                    return false;
                }
            }

            return true;
        }

        private static IdentityAttributeType GetIdentityType(String value)
        {
            var type = value.Trim();

            if (type.StartsWith("NOTATION", StringComparison.Ordinal))
            {
                return IdentityAttributeType.NOTATION;
            }

            return Enum.TryParse(type, out IdentityAttributeType result) ? result : IdentityAttributeType.None;
        }

        private static IEnumerable<String> GetNotationNames(String value)
        {
            var match = Regex.Match(value, "^NOTATION\\s*\\(([^\\)]*)\\)$");
            return match.Success ?
                match.Groups[1].Value.Split('|').Select(m => m.Trim()).Where(m => m.Length > 0).ToList() :
                Enumerable.Empty<String>();
        }

        private Boolean ValidateElementsAgainstDtd(Element root)
        {
            // Declarations are looked up by name once per document instead of searched on every
            // element. The first element declaration of a name wins, as the search found it first;
            // attribute declarations keep their order. A declaration without a name matched nothing.
            var elementDeclarations = new Dictionary<String, ElementDeclaration>(StringComparer.Ordinal);

            foreach (var declaration in _dtd.Elements)
            {
                if (declaration.Name != null && !elementDeclarations.ContainsKey(declaration.Name))
                {
                    elementDeclarations.Add(declaration.Name, declaration);
                }
            }

            var attributeDeclarations = new Dictionary<String, List<AttributeDeclaration>>(StringComparer.Ordinal);

            foreach (var declaration in _dtd.Attributes)
            {
                if (declaration.Name != null)
                {
                    if (!attributeDeclarations.TryGetValue(declaration.Name, out var list))
                    {
                        list = new List<AttributeDeclaration>();
                        attributeDeclarations.Add(declaration.Name, list);
                    }

                    list.Add(declaration);
                }
            }

            var declaredAttributes = new Dictionary<String, HashSet<String>>(StringComparer.Ordinal);

            foreach (var pair in attributeDeclarations)
            {
                var declared = new HashSet<String>(StringComparer.Ordinal);

                foreach (var declaration in pair.Value)
                {
                    foreach (var entry in declaration.Declarations)
                    {
                        declared.Add(entry.Name);
                    }
                }

                declaredAttributes.Add(pair.Key, declared);
            }

            return ValidateElements(root, element => ValidateElementAgainstDtd(element, elementDeclarations, attributeDeclarations, declaredAttributes));
        }

        private static Boolean ValidateElementAgainstDtd(
            Element element,
            Dictionary<String, ElementDeclaration> elementDeclarations,
            Dictionary<String, List<AttributeDeclaration>> attributeDeclarations,
            Dictionary<String, HashSet<String>> declaredAttributes)
        {
            if (!elementDeclarations.TryGetValue(element.NodeName, out var declaration) || !declaration.Check(element))
            {
                return false;
            }

            if (attributeDeclarations.TryGetValue(element.NodeName, out var elementAttributes))
            {
                for (var i = 0; i < elementAttributes.Count; i++)
                {
                    if (!elementAttributes[i].Check(element))
                    {
                        return false;
                    }
                }

                var declared = declaredAttributes[element.NodeName];
                var elementNode = (IElement)element;

                foreach (var attr in elementNode.Attributes)
                {
                    var name = attr.Name;

                    if (name.StartsWith("xmlns", StringComparison.Ordinal))
                    {
                        continue;
                    }

                    if (!declared.Contains(name))
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        private Boolean ValidateElementAgainstInternalSubset(Element element, Dictionary<String, ContentModel> contentModels)
        {
            if (!contentModels.TryGetValue(element.NodeName, out var model))
            {
                return false;
            }

            var hasDeclarations = _internalAttributeDeclarations.TryGetValue(element.NodeName, out var declaredAttributes) && declaredAttributes.Count > 0;
            _internalAttributeRules.TryGetValue(element.NodeName, out var attributeRules);

            foreach (var attr in ((IElement)element).Attributes)
            {
                var name = attr.Name;

                if (name.StartsWith("xmlns", StringComparison.Ordinal))
                {
                    continue;
                }

                if (hasDeclarations && !declaredAttributes.Contains(name))
                {
                    return false;
                }

                if (attributeRules != null && attributeRules.TryGetValue(name, out var rule) && rule.HasFixedValue)
                {
                    if (!String.Equals(attr.Value, rule.FixedValue, StringComparison.Ordinal))
                    {
                        return false;
                    }
                }

                if (!hasDeclarations && name.StartsWith("xml:", StringComparison.Ordinal))
                {
                    return false;
                }
            }

            if (attributeRules != null)
            {
                foreach (var pair in attributeRules)
                {
                    if (pair.Value.IsRequired && ((IElement)element).Attributes[pair.Key] == null)
                    {
                        return false;
                    }
                }
            }

            return ValidateContentModel(element, model);
        }

        private static Boolean ValidateContentModel(Element element, ContentModel model)
        {
            if (model.Kind == ContentModelKind.Any)
            {
                return true;
            }

            // Significant children are elements and texts that are not only whitespace
            var children = ((INode)element).ChildNodes;
            var matched = 0;

            for (var i = 0; i < children.Length; i++)
            {
                var child = children[i];

                if (child is IElement childElement)
                {
                    switch (model.Kind)
                    {
                        case ContentModelKind.Empty:
                            return false;
                        case ContentModelKind.Mixed when !model.Names.Contains(childElement.NodeName):
                            return false;
                        case ContentModelKind.Sequence:
                            if (matched == model.Sequence.Count || !String.Equals(model.Sequence[matched], childElement.NodeName, StringComparison.Ordinal))
                            {
                                return false;
                            }

                            matched++;
                            break;
                    }
                }
                else if (child is IText text && !String.IsNullOrWhiteSpace(text.Data))
                {
                    if (model.Kind == ContentModelKind.Empty || model.Kind == ContentModelKind.Sequence ||
                        (model.Kind == ContentModelKind.Other && !model.AllowsText))
                    {
                        return false;
                    }
                }
            }

            return model.Kind != ContentModelKind.Sequence || matched == model.Sequence.Count;
        }

        private enum ContentModelKind
        {
            Any,
            Empty,
            Mixed,
            Sequence,
            Other,
        }

        /// <summary>
        /// An element's declared content model, classified once per document instead of matched
        /// against the regular expressions on every element it applies to.
        /// </summary>
        private sealed class ContentModel
        {
            private ContentModel(ContentModelKind kind)
            {
                Kind = kind;
            }

            public ContentModelKind Kind { get; }

            /// <summary>
            /// The element names a mixed model allows.
            /// </summary>
            public HashSet<String> Names { get; private set; }

            /// <summary>
            /// The element names a sequence model requires, in order.
            /// </summary>
            public List<String> Sequence { get; private set; }

            /// <summary>
            /// Whether any other model allows text.
            /// </summary>
            public Boolean AllowsText { get; private set; }

            public static ContentModel Parse(String model)
            {
                if (String.Equals(model, "ANY", StringComparison.Ordinal))
                {
                    return new ContentModel(ContentModelKind.Any);
                }

                if (String.Equals(model, "EMPTY", StringComparison.Ordinal))
                {
                    return new ContentModel(ContentModelKind.Empty);
                }

                if (Regex.IsMatch(model, "^\\(#PCDATA(\\|[A-Za-z_][A-Za-z0-9_:\\.-]*)*\\)\\*$"))
                {
                    return new ContentModel(ContentModelKind.Mixed)
                    {
                        Names = new HashSet<String>(
                            model.Substring(1, model.Length - 3)
                                .Split('|')
                                .Where(m => !String.Equals(m, "#PCDATA", StringComparison.Ordinal)),
                            StringComparer.Ordinal),
                    };
                }

                var elementContentModel = Regex.Match(model, "^\\(([A-Za-z_][A-Za-z0-9_:\\.-]*(?:\\s*,\\s*[A-Za-z_][A-Za-z0-9_:\\.-]*)*)\\)$");

                if (elementContentModel.Success)
                {
                    return new ContentModel(ContentModelKind.Sequence)
                    {
                        Sequence = elementContentModel.Groups[1].Value
                            .Split(',')
                            .Select(m => m.Trim())
                            .Where(m => m.Length > 0)
                            .ToList(),
                    };
                }

                return new ContentModel(ContentModelKind.Other)
                {
                    AllowsText = model.Contains("#PCDATA", StringComparison.Ordinal),
                };
            }
        }

        private static String Unquote(String value)
        {
            if (String.IsNullOrEmpty(value) || value.Length < 2)
            {
                return value;
            }

            var first = value[0];
            var last = value[value.Length - 1];

            if ((first == Symbols.DoubleQuote && last == Symbols.DoubleQuote) || (first == Symbols.SingleQuote && last == Symbols.SingleQuote))
            {
                return value.Substring(1, value.Length - 2);
            }

            return value;
        }

        private String ExpandInternalEntities(String data, TextPosition position)
        {
            if (String.IsNullOrEmpty(data) || _internalGeneralEntities.Count == 0)
            {
                return data;
            }

            var value = data;

            for (var i = 0; i < 5; i++)
            {
                var changed = false;

                value = Regex.Replace(value, "&([A-Za-z_][A-Za-z0-9_:\\.-]*);", match =>
                {
                    var name = match.Groups[1].Value;

                    // Every round can multiply the text by the references an entity holds, so each
                    // replacement counts against the expansion limit before the text grows;
                    // beyond it, with errors suppressed, the reference stays text
                    if (_internalGeneralEntityContents.TryGetValue(name, out var replacement) &&
                        _tokenizer.ClaimEntityCharacters(replacement.Length, position))
                    {
                        changed = true;
                        return replacement;
                    }

                    return match.Value;
                });

                if (!changed)
                {
                    break;
                }
            }

            return value;
        }

        private void SetEncoding(String charSet)
        {
            if (TextEncoding.IsSupported(charSet))
            {
                var encoding = TextEncoding.Resolve(charSet);

                if (encoding != null)
                {
                    try
                    {
                        _document.Source.CurrentEncoding = encoding;
                    }
                    catch (NotSupportedException)
                    {
                        _currentMode = XmlTreeMode.Initial;
                        _document.Clear();
                        _openElements.Clear();
                    }
                }
            }
        }

        #endregion

        #region Nested types

        private sealed class InternalAttributeRule
        {
            public Boolean IsRequired { get; set; }

            public String FixedValue { get; set; }

            public IdentityAttributeType Type { get; set; }

            public Boolean IsDefaultAllowedForId { get; set; }

            public IEnumerable<String> NotationNames { get; set; } = Enumerable.Empty<String>();

            public Boolean HasFixedValue => FixedValue != null;
        }

        private sealed class IdentityAttributeDeclaration
        {
            public String Name { get; set; }

            public IdentityAttributeType Type { get; set; }

            public Boolean IsDefaultAllowedForId { get; set; }

            public IEnumerable<String> NotationNames { get; set; } = Enumerable.Empty<String>();
        }

        private enum IdentityAttributeType
        {
            None,
            ID,
            IDREF,
            IDREFS,
            ENTITY,
            ENTITIES,
            NMTOKEN,
            NMTOKENS,
            NOTATION,
        }

        #endregion
    }
}
