namespace AngleSharp.Xml.Benchmarks
{
    using AngleSharp.Xml.Parser;
    using System;
    using System.Globalization;
    using System.Text;

    /// <summary>
    /// The document shapes the benchmarks share. Every shape stays valid against its DTD, so a
    /// validation benchmark walks the whole tree instead of stopping at a first failure.
    /// </summary>
    static class Documents
    {
        /// <summary>
        /// The marker line naming the AngleSharp.Xml build under test, written by every setup, so
        /// a comparison of two builds can prove from the logs which one each run measured.
        /// </summary>
        public static String LibraryMarker => "// AngleSharp.Xml module " + typeof(XmlParser).Assembly.ManifestModule.ModuleVersionId;

        /// <summary>
        /// Declarations for <see cref="Catalog"/>: element sequences, mixed content, a required
        /// attribute, and ID / IDREF pairs, so every validation walk has work on every element.
        /// </summary>
        public const String CatalogDeclarations =
            "<!ELEMENT catalog (item)*>" +
            "<!ELEMENT item (name, description, price)>" +
            "<!ELEMENT name (#PCDATA)>" +
            "<!ELEMENT description (#PCDATA)>" +
            "<!ELEMENT price (#PCDATA)>" +
            "<!ATTLIST item id ID #REQUIRED ref IDREF #IMPLIED>";

        /// <summary>
        /// Declarations for <see cref="Deep"/>.
        /// </summary>
        public const String DeepDeclarations =
            "<!ELEMENT a ANY>" +
            "<!ELEMENT b EMPTY>" +
            "<!ATTLIST a id ID #REQUIRED>";

        /// <summary>
        /// A flat record list: the root holds <paramref name="items"/> items of three children
        /// each, with text, CDATA and a comment, every item referencing the one before it.
        /// </summary>
        public static String Catalog(String doctype, Int32 items)
        {
            var source = new StringBuilder(doctype, doctype.Length + items * 160);
            source.Append("<catalog>");

            for (var i = 0; i < items; i++)
            {
                var id = i.ToString(CultureInfo.InvariantCulture);
                source.Append("<item id=\"i").Append(id).Append('"');

                if (i > 0)
                {
                    source.Append(" ref=\"i").Append((i - 1).ToString(CultureInfo.InvariantCulture)).Append('"');
                }

                source.Append("><name>Name ").Append(id).Append("</name>")
                    .Append("<description><![CDATA[Description <").Append(id).Append(">]]></description>")
                    .Append("<!-- note -->")
                    .Append("<price>9.99</price></item>");
            }

            source.Append("</catalog>");
            return source.ToString();
        }

        /// <summary>
        /// A chain nested <paramref name="depth"/> levels deep, where every level has an ID, a
        /// text, a CDATA section and an empty sibling element.
        /// </summary>
        public static String Deep(String doctype, Int32 depth)
        {
            var source = new StringBuilder(doctype, doctype.Length + depth * 48);

            for (var i = 0; i < depth; i++)
            {
                source.Append("<a id=\"l").Append(i.ToString(CultureInfo.InvariantCulture)).Append("\">t<![CDATA[c]]><b/>");
            }

            for (var i = 0; i < depth; i++)
            {
                source.Append("</a>");
            }

            return source.ToString();
        }

        /// <summary>
        /// The DOCTYPE carrying <paramref name="declarations"/> as internal subset.
        /// </summary>
        public static String InternalDoctype(String root, String declarations) =>
            "<!DOCTYPE " + root + " [" + declarations + "]>";

        /// <summary>
        /// The DOCTYPE referencing the local external subset at <paramref name="path"/>.
        /// </summary>
        public static String ExternalDoctype(String root, String path) =>
            "<!DOCTYPE " + root + " SYSTEM \"" + new Uri(path).AbsoluteUri + "\">";
    }
}
