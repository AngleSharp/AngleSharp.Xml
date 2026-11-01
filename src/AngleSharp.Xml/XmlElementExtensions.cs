namespace AngleSharp.Xml
{
    using AngleSharp.Dom;
    using System;
    using System.Collections.Generic;
    using System.Text;

    /// <summary>
    /// Provides convenience semantics for attributes in the XML namespace.
    /// </summary>
    public static class XmlElementExtensions
    {
        /// <summary>
        /// Gets the effective base URL after applying inherited <c>xml:base</c> values.
        /// </summary>
        /// <param name="element">The element to inspect.</param>
        /// <returns>The effective URL, or null if no base is available.</returns>
        public static Url GetXmlBaseUrl(this IElement element)
        {
            if (element == null)
            {
                throw new ArgumentNullException(nameof(element));
            }

            var ancestors = new Stack<IElement>();

            for (var current = element; current != null; current = current.ParentElement)
            {
                ancestors.Push(current);
            }

            var baseUrl = element.Owner?.BaseUrl;

            while (ancestors.Count > 0)
            {
                var value = GetXmlAttribute(ancestors.Pop(), "base");

                if (value != null)
                {
                    baseUrl = baseUrl == null || baseUrl.IsInvalid ? new Url(value) : new Url(baseUrl, value);
                }
            }

            return baseUrl;
        }

        /// <summary>
        /// Gets the effective base URI after applying inherited <c>xml:base</c> values.
        /// </summary>
        /// <param name="element">The element to inspect.</param>
        /// <returns>The effective URI string, or an empty string if no base is available.</returns>
        public static String GetXmlBaseUri(this IElement element)
        {
            if (element == null)
            {
                throw new ArgumentNullException(nameof(element));
            }

            var ancestors = new Stack<IElement>();

            for (var current = element; current != null; current = current.ParentElement)
            {
                ancestors.Push(current);
            }

            var baseUri = element.Owner?.BaseUri ?? String.Empty;

            while (ancestors.Count > 0)
            {
                var value = GetXmlAttribute(ancestors.Pop(), "base");

                if (value != null)
                {
                    baseUri = ResolveUriReference(baseUri, value);
                }
            }

            return baseUri;
        }

        /// <summary>
        /// Gets the normalized <c>xml:id</c> value declared on the element.
        /// </summary>
        /// <param name="element">The element to inspect.</param>
        /// <returns>The normalized identifier, or null if none is declared.</returns>
        public static String GetXmlId(this IElement element)
        {
            if (element == null)
            {
                throw new ArgumentNullException(nameof(element));
            }

            var value = GetXmlAttribute(element, "id");
            return value == null ? null : NormalizeXmlId(value);
        }

        /// <summary>
        /// Finds the first element in document order with the given <c>xml:id</c> value.
        /// </summary>
        /// <param name="document">The document to search.</param>
        /// <param name="elementId">The normalized identifier to find.</param>
        /// <returns>The matching element, or null.</returns>
        public static IElement GetElementByXmlId(this IDocument document, String elementId)
        {
            if (document == null)
            {
                throw new ArgumentNullException(nameof(document));
            }

            if (elementId == null)
            {
                throw new ArgumentNullException(nameof(elementId));
            }

            return FindByXmlId(document.DocumentElement, elementId);
        }

        /// <summary>
        /// Gets the value of the DTD-declared ID attribute on an element.
        /// </summary>
        /// <param name="element">The element to inspect.</param>
        /// <returns>The declared ID value, or null.</returns>
        public static String GetDtdId(this IElement element)
        {
            if (element == null)
            {
                throw new ArgumentNullException(nameof(element));
            }

            return element is Dom.XmlElement xmlElement && xmlElement.IdAttribute != null ?
                element.GetAttribute(xmlElement.IdAttribute) :
                null;
        }

        /// <summary>
        /// Finds the first element in document order with the given DTD-declared ID.
        /// </summary>
        /// <param name="document">The document to search.</param>
        /// <param name="elementId">The ID to find.</param>
        /// <returns>The matching element, or null.</returns>
        public static IElement GetElementByDtdId(this IDocument document, String elementId)
        {
            if (document == null)
            {
                throw new ArgumentNullException(nameof(document));
            }

            if (elementId == null)
            {
                throw new ArgumentNullException(nameof(elementId));
            }

            return FindByDtdId(document.DocumentElement, elementId);
        }

        /// <summary>
        /// Gets the effective language from the nearest <c>xml:lang</c> declaration.
        /// </summary>
        /// <param name="element">The element to inspect.</param>
        /// <returns>The effective language, an empty string when reset, or null when undeclared.</returns>
        public static String GetXmlLanguage(this IElement element)
        {
            if (element == null)
            {
                throw new ArgumentNullException(nameof(element));
            }

            for (var current = element; current != null; current = current.ParentElement)
            {
                var value = GetXmlAttribute(current, "lang");

                if (value != null)
                {
                    return value;
                }
            }

            return null;
        }

        private static IElement FindByXmlId(IElement root, String elementId) =>
            FindFirst(root, element => element.GetXmlId() == elementId);

        private static IElement FindByDtdId(IElement root, String elementId) =>
            FindFirst(root, element => element.GetDtdId() == elementId);

        /// <summary>
        /// Finds the first element in document order, starting with the root, that matches. Walks
        /// with an explicit stack, so deeply nested documents cannot overflow the call stack.
        /// </summary>
        private static IElement FindFirst(IElement root, Func<IElement, Boolean> matches)
        {
            if (root == null)
            {
                return null;
            }

            var pending = new Stack<IElement>();
            pending.Push(root);

            while (pending.Count > 0)
            {
                var element = pending.Pop();

                if (matches(element))
                {
                    return element;
                }

                var children = element.ChildNodes;

                for (var i = children.Length - 1; i >= 0; i--)
                {
                    if (children[i] is IElement child)
                    {
                        pending.Push(child);
                    }
                }
            }

            return null;
        }

        private static String GetXmlAttribute(IElement element, String localName)
        {
            foreach (var attribute in element.Attributes)
            {
                if (attribute.NamespaceUri == NamespaceNames.XmlUri && attribute.LocalName == localName)
                {
                    return attribute.Value;
                }
            }

            return null;
        }

        internal static String NormalizeXmlId(String value)
        {
            var result = new StringBuilder();
            var pendingSpace = false;

            foreach (var character in value)
            {
                if (character == ' ' || character == '\t' || character == '\n' || character == '\r')
                {
                    pendingSpace = result.Length > 0;
                }
                else
                {
                    if (pendingSpace)
                    {
                        result.Append(' ');
                        pendingSpace = false;
                    }

                    result.Append(character);
                }
            }

            return result.ToString();
        }

        private static String ResolveUriReference(String baseUri, String reference)
        {
            var baseParts = UriReference.Parse(baseUri);
            var referenceParts = UriReference.Parse(reference);
            var result = new UriReference
            {
                Fragment = referenceParts.Fragment,
            };

            if (referenceParts.Scheme != null)
            {
                result.Scheme = referenceParts.Scheme;
                result.Authority = referenceParts.Authority;
                result.Path = RemoveDotSegments(referenceParts.Path);
                result.Query = referenceParts.Query;
                return result.ToString();
            }

            result.Scheme = baseParts.Scheme;

            if (referenceParts.Authority != null)
            {
                result.Authority = referenceParts.Authority;
                result.Path = RemoveDotSegments(referenceParts.Path);
                result.Query = referenceParts.Query;
            }
            else
            {
                result.Authority = baseParts.Authority;

                if (referenceParts.Path.Length == 0)
                {
                    result.Path = baseParts.Path;
                    result.Query = referenceParts.Query ?? baseParts.Query;
                }
                else
                {
                    result.Path = referenceParts.Path[0] == '/' ?
                        RemoveDotSegments(referenceParts.Path) :
                        RemoveDotSegments(MergePaths(baseParts, referenceParts.Path));
                    result.Query = referenceParts.Query;
                }
            }

            return result.ToString();
        }

        private static String MergePaths(UriReference baseParts, String referencePath)
        {
            if (baseParts.Authority != null && baseParts.Path.Length == 0)
            {
                return "/" + referencePath;
            }

            var slash = baseParts.Path.LastIndexOf('/');
            return slash < 0 ? referencePath : baseParts.Path.Substring(0, slash + 1) + referencePath;
        }

        /// <summary>
        /// The remove_dot_segments algorithm of RFC 3986, section 5.2.4. Reads the input through an
        /// index and builds the output in place, so a path is processed in linear time; the
        /// <c>xml:base</c> chain of a deep document resolves one such path per level.
        /// </summary>
        private static String RemoveDotSegments(String value)
        {
            var input = value;
            var position = 0;
            var output = new StringBuilder(value.Length);

            while (position < input.Length)
            {
                var remaining = input.Length - position;

                if (StartsWith(input, position, "../"))
                {
                    position += 3;
                }
                else if (StartsWith(input, position, "./"))
                {
                    position += 2;
                }
                else if (StartsWith(input, position, "/./"))
                {
                    position += 2;
                }
                else if (remaining == 2 && StartsWith(input, position, "/."))
                {
                    input = "/";
                    position = 0;
                }
                else if (StartsWith(input, position, "/../"))
                {
                    position += 3;
                    RemoveLastSegment(output);
                }
                else if (remaining == 3 && StartsWith(input, position, "/.."))
                {
                    input = "/";
                    position = 0;
                    RemoveLastSegment(output);
                }
                else if (remaining == 1 && input[position] == '.' || remaining == 2 && StartsWith(input, position, ".."))
                {
                    position = input.Length;
                }
                else
                {
                    var slash = input.IndexOf('/', input[position] == '/' ? position + 1 : position);
                    var end = slash < 0 ? input.Length : slash;
                    output.Append(input, position, end - position);
                    position = end;
                }
            }

            return output.ToString();
        }

        private static Boolean StartsWith(String value, Int32 position, String prefix) =>
            value.Length - position >= prefix.Length &&
            String.CompareOrdinal(value, position, prefix, 0, prefix.Length) == 0;

        private static void RemoveLastSegment(StringBuilder value)
        {
            var slash = value.Length - 1;

            while (slash >= 0 && value[slash] != '/')
            {
                slash--;
            }

            value.Length = Math.Max(slash, 0);
        }

        sealed class UriReference
        {
            public String Scheme { get; set; }

            public String Authority { get; set; }

            public String Path { get; set; }

            public String Query { get; set; }

            public String Fragment { get; set; }

            public static UriReference Parse(String value)
            {
                value = value ?? String.Empty;
                var result = new UriReference();
                var fragment = value.IndexOf('#');

                if (fragment >= 0)
                {
                    result.Fragment = value.Substring(fragment + 1);
                    value = value.Substring(0, fragment);
                }

                var query = value.IndexOf('?');

                if (query >= 0)
                {
                    result.Query = value.Substring(query + 1);
                    value = value.Substring(0, query);
                }

                var colon = value.IndexOf(':');
                var slash = value.IndexOf('/');

                if (colon > 0 && (slash < 0 || colon < slash))
                {
                    result.Scheme = value.Substring(0, colon);
                    value = value.Substring(colon + 1);
                }

                if (value.StartsWith("//", StringComparison.Ordinal))
                {
                    var path = value.IndexOf('/', 2);
                    result.Authority = path < 0 ? value.Substring(2) : value.Substring(2, path - 2);
                    value = path < 0 ? String.Empty : value.Substring(path);
                }

                result.Path = value;
                return result;
            }

            public override String ToString()
            {
                var result = new StringBuilder();

                if (Scheme != null)
                {
                    result.Append(Scheme).Append(':');
                }

                if (Authority != null)
                {
                    result.Append("//").Append(Authority);
                }

                result.Append(Path);

                if (Query != null)
                {
                    result.Append('?').Append(Query);
                }

                if (Fragment != null)
                {
                    result.Append('#').Append(Fragment);
                }

                return result.ToString();
            }
        }
    }
}