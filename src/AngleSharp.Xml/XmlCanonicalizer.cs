namespace AngleSharp.Xml
{
    using AngleSharp.Dom;
    using AngleSharp.Xml.Dom;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    sealed class XmlCanonicalizer
    {
        private const String XmlNamespace = "http://www.w3.org/XML/1998/namespace";
        private readonly XmlCanonicalizationOptions _options;
        private readonly HashSet<String> _inclusivePrefixes;
        private readonly StringBuilder _output;

        public XmlCanonicalizer(XmlCanonicalizationOptions options)
        {
            _options = options ?? new XmlCanonicalizationOptions();

            if (_options.Mode != XmlCanonicalizationMode.CanonicalXml11 &&
                _options.Mode != XmlCanonicalizationMode.ExclusiveXml10)
            {
                throw new ArgumentOutOfRangeException(nameof(options), "The canonicalization mode is not supported.");
            }

            _inclusivePrefixes = new HashSet<String>(StringComparer.Ordinal);
            _output = new StringBuilder();

            if (_options.InclusiveNamespacePrefixes != null)
            {
                foreach (var prefix in _options.InclusiveNamespacePrefixes)
                {
                    _inclusivePrefixes.Add(prefix == "#default" ? String.Empty : prefix ?? String.Empty);
                }
            }
        }

        public Byte[] Canonicalize(INode node)
        {
            if (node == null)
            {
                throw new ArgumentNullException(nameof(node));
            }

            if (node is IDocument document)
            {
                WriteDocument(document);
            }
            else if (node is IElement element)
            {
                WriteElement(element, new Dictionary<String, String>(StringComparer.Ordinal), true);
            }
            else
            {
                throw new ArgumentException("Only XML documents and element subtrees can be canonicalized.", nameof(node));
            }

            return new UTF8Encoding(false).GetBytes(_output.ToString());
        }

        private void WriteDocument(IDocument document)
        {
            var root = document.DocumentElement;

            if (root == null)
            {
                throw new ArgumentException("The document must have a document element.", nameof(document));
            }

            var afterRoot = false;

            foreach (var child in document.ChildNodes)
            {
                if (child == root)
                {
                    WriteElement(root, new Dictionary<String, String>(StringComparer.Ordinal), false);
                    afterRoot = true;
                }
                else if (child is IProcessingInstruction processing)
                {
                    WriteOutsideDocumentElement(() => WriteProcessing(processing), afterRoot);
                }
                else if (_options.IncludeComments && child is IComment comment)
                {
                    WriteOutsideDocumentElement(() => WriteComment(comment), afterRoot);
                }
            }
        }

        private void WriteOutsideDocumentElement(Action write, Boolean afterRoot)
        {
            if (afterRoot)
            {
                _output.Append('\n');
            }

            write();

            if (!afterRoot)
            {
                _output.Append('\n');
            }
        }

        private void WriteElement(IElement element, Dictionary<String, String> renderedNamespaces, Boolean isSubtreeRoot)
        {
            var inScopeNamespaces = GetInScopeNamespaces(element);
            ValidateNamespaces(inScopeNamespaces);
            var namespaces = GetNamespacesToRender(element, inScopeNamespaces, renderedNamespaces);
            var attributes = GetAttributes(element, isSubtreeRoot);
            var qualifiedName = GetQualifiedName(element.Prefix, element.LocalName);

            _output.Append('<').Append(qualifiedName);

            foreach (var declaration in namespaces.OrderBy(m => m.Key, StringComparer.Ordinal))
            {
                _output.Append(' ').Append(String.IsNullOrEmpty(declaration.Key) ? "xmlns" : "xmlns:" + declaration.Key);
                _output.Append("=\"");
                WriteAttributeValue(declaration.Value);
                _output.Append('"');
            }

            foreach (var attribute in attributes
                .OrderBy(m => m.NamespaceUri ?? String.Empty, StringComparer.Ordinal)
                .ThenBy(m => m.LocalName, StringComparer.Ordinal))
            {
                _output.Append(' ').Append(attribute.QualifiedName).Append("=\"");
                WriteAttributeValue(attribute.Value);
                _output.Append('"');
            }

            _output.Append('>');
            var childNamespaces = new Dictionary<String, String>(renderedNamespaces, StringComparer.Ordinal);

            foreach (var declaration in namespaces)
            {
                childNamespaces[declaration.Key] = declaration.Value;
            }

            foreach (var child in element.ChildNodes)
            {
                WriteNode(child, childNamespaces);
            }

            _output.Append("</").Append(qualifiedName).Append('>');
        }

        private void WriteNode(INode node, Dictionary<String, String> renderedNamespaces)
        {
            if (node is IElement element)
            {
                WriteElement(element, renderedNamespaces, false);
            }
            else if (node is IProcessingInstruction processing)
            {
                WriteProcessing(processing);
            }
            else if (_options.IncludeComments && node is IComment comment)
            {
                WriteComment(comment);
            }
            else if (node is ICharacterData characterData && !(node is IComment))
            {
                WriteText(characterData.Data);
            }
        }

        private IDictionary<String, String> GetNamespacesToRender(
            IElement element,
            Dictionary<String, String> inScope,
            Dictionary<String, String> rendered)
        {
            var result = new Dictionary<String, String>(StringComparer.Ordinal);

            if (_options.Mode == XmlCanonicalizationMode.CanonicalXml11)
            {
                foreach (var declaration in inScope)
                {
                    if (declaration.Key == "xml")
                    {
                        continue;
                    }

                    if (!rendered.TryGetValue(declaration.Key, out var value) || value != declaration.Value)
                    {
                        if (!String.IsNullOrEmpty(declaration.Value) || rendered.ContainsKey(declaration.Key))
                        {
                            result[declaration.Key] = declaration.Value;
                        }
                    }
                }
            }
            else
            {
                var visiblePrefixes = new HashSet<String>(_inclusivePrefixes, StringComparer.Ordinal);
                visiblePrefixes.Add(element.Prefix ?? String.Empty);

                foreach (var attribute in element.Attributes.Where(m => !IsNamespaceDeclaration(m)))
                {
                    if (!String.IsNullOrEmpty(attribute.Prefix) && attribute.Prefix != "xml")
                    {
                        visiblePrefixes.Add(attribute.Prefix);
                    }
                }

                foreach (var prefix in visiblePrefixes)
                {
                    if (prefix == "xml")
                    {
                        continue;
                    }

                    var value = inScope.TryGetValue(prefix, out var namespaceUri) ? namespaceUri : String.Empty;

                    if (!rendered.TryGetValue(prefix, out var renderedValue) || renderedValue != value)
                    {
                        if (!String.IsNullOrEmpty(value) || String.IsNullOrEmpty(prefix) && rendered.ContainsKey(prefix))
                        {
                            result[prefix] = value;
                        }
                    }
                }
            }

            return result;
        }

        private List<CanonicalAttribute> GetAttributes(IElement element, Boolean isSubtreeRoot)
        {
            var attributes = element.Attributes
                .Where(m => !IsNamespaceDeclaration(m))
                .Select(m => new CanonicalAttribute(m.Prefix, m.LocalName, m.NamespaceUri, m.Value))
                .ToList();

            if (isSubtreeRoot && _options.Mode == XmlCanonicalizationMode.CanonicalXml11)
            {
                AddInheritedXmlAttribute(element, attributes, "lang");
                AddInheritedXmlAttribute(element, attributes, "space");
                AddFixedUpXmlBase(element, attributes);
            }

            return attributes;
        }

        private static void AddInheritedXmlAttribute(IElement element, List<CanonicalAttribute> attributes, String localName)
        {
            if (attributes.Any(m => m.NamespaceUri == XmlNamespace && m.LocalName == localName))
            {
                return;
            }

            for (var ancestor = element.ParentElement; ancestor != null; ancestor = ancestor.ParentElement)
            {
                var attribute = ancestor.Attributes.FirstOrDefault(m => m.NamespaceUri == XmlNamespace && m.LocalName == localName);

                if (attribute != null)
                {
                    attributes.Add(new CanonicalAttribute("xml", localName, XmlNamespace, attribute.Value));
                    return;
                }
            }
        }

        private static void AddFixedUpXmlBase(IElement element, List<CanonicalAttribute> attributes)
        {
            var bases = new Stack<String>();
            var hasAncestorBase = false;

            for (var current = element; current != null; current = current.ParentElement)
            {
                var attribute = current.Attributes.FirstOrDefault(m => m.NamespaceUri == XmlNamespace && m.LocalName == "base");

                if (attribute != null)
                {
                    bases.Push(attribute.Value);
                    hasAncestorBase |= current != element;
                }
            }

            if (!hasAncestorBase)
            {
                return;
            }

            var value = default(String);

            while (bases.Count > 0)
            {
                value = JoinUriReferences(value, bases.Pop());
            }

            attributes.RemoveAll(m => m.NamespaceUri == XmlNamespace && m.LocalName == "base");

            if (!String.IsNullOrEmpty(value))
            {
                attributes.Add(new CanonicalAttribute("xml", "base", XmlNamespace, value));
            }
        }

        private static String JoinUriReferences(String baseUri, String reference)
        {
            reference = RemoveFragment(reference ?? String.Empty);

            if (String.IsNullOrEmpty(baseUri) || HasScheme(reference))
            {
                return reference;
            }

            baseUri = RemoveFragment(baseUri);
            var referenceQuery = GetQuery(reference);
            var referencePath = RemoveQuery(reference);
            var baseQuery = GetQuery(baseUri);
            var basePath = RemoveQuery(baseUri);
            var schemeEnd = basePath.IndexOf(':');
            var scheme = schemeEnd > 0 ? basePath.Substring(0, schemeEnd + 1) : String.Empty;
            var afterScheme = schemeEnd > 0 ? basePath.Substring(schemeEnd + 1) : basePath;
            var authority = String.Empty;

            if (referencePath.StartsWith("//", StringComparison.Ordinal))
            {
                return scheme + RemoveDotSegments(referencePath) + referenceQuery;
            }

            if (afterScheme.StartsWith("//", StringComparison.Ordinal))
            {
                var pathStart = afterScheme.IndexOf('/', 2);
                authority = pathStart < 0 ? afterScheme : afterScheme.Substring(0, pathStart);
                afterScheme = pathStart < 0 ? String.Empty : afterScheme.Substring(pathStart);
            }

            if (referencePath.Length == 0)
            {
                return scheme + authority + afterScheme + (referenceQuery.Length > 0 ? referenceQuery : baseQuery);
            }

            var path = referencePath.StartsWith("/", StringComparison.Ordinal) ?
                referencePath :
                MergePaths(afterScheme, referencePath, authority.Length > 0);
            return scheme + authority + RemoveDotSegments(path) + referenceQuery;
        }

        private static String MergePaths(String basePath, String referencePath, Boolean hasAuthority)
        {
            if (hasAuthority && basePath.Length == 0)
            {
                return "/" + referencePath;
            }

            var slash = basePath.LastIndexOf('/');
            return slash < 0 ? referencePath : basePath.Substring(0, slash + 1) + referencePath;
        }

        private static String RemoveDotSegments(String path)
        {
            var absolute = path.StartsWith("/", StringComparison.Ordinal);
            var trailingSlash = path.EndsWith("/", StringComparison.Ordinal) || path.EndsWith("/.", StringComparison.Ordinal);
            var segments = path.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
            var output = new List<String>();

            foreach (var segment in segments)
            {
                if (segment == ".")
                {
                    continue;
                }

                if (segment == "..")
                {
                    if (output.Count > 0 && output[output.Count - 1] != "..")
                    {
                        output.RemoveAt(output.Count - 1);
                    }
                    else if (!absolute)
                    {
                        output.Add("..");
                    }

                    trailingSlash = true;
                }
                else
                {
                    output.Add(segment);
                }
            }

            var result = (absolute ? "/" : String.Empty) + String.Join("/", output);

            if (trailingSlash && result.Length > 0 && !result.EndsWith("/", StringComparison.Ordinal))
            {
                result += "/";
            }

            return result;
        }

        private static Boolean HasScheme(String value)
        {
            var colon = value.IndexOf(':');

            if (colon <= 0 || !Char.IsLetter(value[0]))
            {
                return false;
            }

            for (var i = 1; i < colon; i++)
            {
                if (!Char.IsLetterOrDigit(value[i]) && value[i] != '+' && value[i] != '-' && value[i] != '.')
                {
                    return false;
                }
            }

            return true;
        }

        private static String RemoveFragment(String value)
        {
            var hash = value.IndexOf('#');
            return hash < 0 ? value : value.Substring(0, hash);
        }

        private static String RemoveQuery(String value)
        {
            var query = value.IndexOf('?');
            return query < 0 ? value : value.Substring(0, query);
        }

        private static String GetQuery(String value)
        {
            var query = value.IndexOf('?');
            return query < 0 ? String.Empty : value.Substring(query);
        }

        private static Dictionary<String, String> GetInScopeNamespaces(IElement element)
        {
            var ancestors = new Stack<IElement>();
            var result = new Dictionary<String, String>(StringComparer.Ordinal);

            for (var current = element; current != null; current = current.ParentElement)
            {
                ancestors.Push(current);
            }

            while (ancestors.Count > 0)
            {
                foreach (var attribute in ancestors.Pop().Attributes.Where(IsNamespaceDeclaration))
                {
                    var prefix = attribute.Name == "xmlns" ? String.Empty : attribute.LocalName;
                    result[prefix] = attribute.Value;
                }
            }

            return result;
        }

        private static void ValidateNamespaces(Dictionary<String, String> namespaces)
        {
            foreach (var namespaceUri in namespaces.Values)
            {
                if (!String.IsNullOrEmpty(namespaceUri) && !Uri.TryCreate(namespaceUri, UriKind.Absolute, out _))
                {
                    throw new InvalidOperationException("Canonical XML does not permit relative namespace URIs.");
                }
            }
        }

        private static Boolean IsNamespaceDeclaration(IAttr attribute) =>
            attribute.Name == "xmlns" || attribute.Prefix == "xmlns";

        private void WriteAttributeValue(String value)
        {
            foreach (var character in value ?? String.Empty)
            {
                switch (character)
                {
                    case '&': _output.Append("&amp;"); break;
                    case '<': _output.Append("&lt;"); break;
                    case '"': _output.Append("&quot;"); break;
                    case '\t': _output.Append("&#x9;"); break;
                    case '\n': _output.Append("&#xA;"); break;
                    case '\r': _output.Append("&#xD;"); break;
                    default: _output.Append(character); break;
                }
            }
        }

        private void WriteText(String value)
        {
            foreach (var character in value ?? String.Empty)
            {
                switch (character)
                {
                    case '&': _output.Append("&amp;"); break;
                    case '<': _output.Append("&lt;"); break;
                    case '>': _output.Append("&gt;"); break;
                    case '\r': _output.Append("&#xD;"); break;
                    default: _output.Append(character); break;
                }
            }
        }

        private void WriteProcessing(IProcessingInstruction processing)
        {
            _output.Append("<?").Append(processing.Target);

            if (!String.IsNullOrEmpty(processing.Data))
            {
                _output.Append(' ').Append(EscapeCarriageReturns(processing.Data));
            }

            _output.Append("?>");
        }

        private void WriteComment(IComment comment)
        {
            _output.Append("<!--").Append(EscapeCarriageReturns(comment.Data)).Append("-->");
        }

        private static String EscapeCarriageReturns(String value) => value?.Replace("\r", "&#xD;") ?? String.Empty;

        private static String GetQualifiedName(String prefix, String localName) =>
            String.IsNullOrEmpty(prefix) ? localName : prefix + ":" + localName;

        sealed class CanonicalAttribute
        {
            public CanonicalAttribute(String prefix, String localName, String namespaceUri, String value)
            {
                Prefix = prefix;
                LocalName = localName;
                NamespaceUri = namespaceUri;
                Value = value;
            }

            public String Prefix { get; }

            public String LocalName { get; }

            public String NamespaceUri { get; }

            public String Value { get; }

            public String QualifiedName => GetQualifiedName(Prefix, LocalName);
        }
    }
}