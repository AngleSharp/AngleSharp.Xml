namespace AngleSharp.Xml.Tests.Parser
{
    using AngleSharp.Xml.Dom;
    using AngleSharp.Xml.Parser;
    using AngleSharp.Xml.Tests.Mocks;
    using NUnit.Framework;
    using System;
    using System.Text;

    /// <summary>
    /// Deeply nested documents must not overflow the call stack, which would end the process.
    /// </summary>
    [TestFixture]
    public class XmlDeepNesting
    {
        // Recursive tree walks overflowed from about 5000 levels on a 1 MB stack; building the tree
        // itself is quadratic in depth (AngleSharp's AppendChild walks the ancestors), so going much
        // deeper only makes the tests slow. The parser stops at XmlParserOptions.MaximumDepth (512 by
        // default), so these documents lift it to reach the walks.
        private const Int32 Depth = 20000;

        [Test]
        public void DeeplyNestedDocumentWithoutDoctypeParses()
        {
            var document = ParseWithoutDepthLimit(Nest(String.Empty, "x"));

            Assert.IsNotNull(document.DocumentElement);
            Assert.IsTrue(document.IsValid);
        }

        [Test]
        public void DeeplyNestedDocumentIsValidAgainstInternalSubset()
        {
            var document = ParseWithoutDepthLimit(Nest("<!DOCTYPE a [<!ELEMENT a ANY>]>", "x"));

            Assert.IsTrue(document.IsValid);
        }

        [Test]
        public void UndeclaredElementAtDeepestLevelIsInvalidAgainstInternalSubset()
        {
            var document = ParseWithoutDepthLimit(Nest("<!DOCTYPE a [<!ELEMENT a ANY>]>", "<b/>"));

            Assert.IsFalse(document.IsValid);
        }

        [Test]
        public void DuplicateIdAtDeepestLevelIsInvalid()
        {
            var doctype = "<!DOCTYPE a [<!ELEMENT a ANY><!ELEMENT b EMPTY><!ATTLIST b id ID #IMPLIED>]>";
            var document = ParseWithoutDepthLimit(Nest(doctype, "<b id=\"one\"/><b id=\"one\"/>"));

            Assert.IsFalse(document.IsValid);
        }

        [Test]
        public void DeeplyNestedDocumentIsValidAgainstExternalSubset()
        {
            const String dtdUrl = "mock://dtd.test/deep.dtd";
            var configuration = new MockRequester().Serve(dtdUrl, "<!ELEMENT a ANY>").CreateConfiguration();

            var valid = ParseWithoutDepthLimit(Nest($@"<!DOCTYPE a SYSTEM ""{dtdUrl}"">", "x"), configuration);
            var invalid = ParseWithoutDepthLimit(Nest($@"<!DOCTYPE a SYSTEM ""{dtdUrl}"">", "<b/>"), configuration);

            Assert.IsTrue(valid.IsValid);
            Assert.IsFalse(invalid.IsValid);
        }

        [Test]
        public void TextContentOfDeeplyNestedElementIncludesTextAndCData()
        {
            var document = ParseWithoutDepthLimit(Nest(String.Empty, "x<![CDATA[y]]>"));

            Assert.AreEqual("xy", document.DocumentElement.TextContent);
        }

        [Test]
        public void TextContentKeepsDocumentOrderAndSkipsCommentsAndProcessingInstructions()
        {
            var document = "<r>a<!--c--><?pi x?><b>b<![CDATA[c]]><c>d</c></b>e<d/></r>".ToXmlDocument();

            Assert.AreEqual("abcde", document.DocumentElement.TextContent);
        }

        private static String Nest(String prolog, String innermost)
        {
            var source = new StringBuilder(prolog.Length + innermost.Length + Depth * 7);
            source.Append(prolog);

            for (var i = 0; i < Depth; i++)
            {
                source.Append("<a>");
            }

            source.Append(innermost);

            for (var i = 0; i < Depth; i++)
            {
                source.Append("</a>");
            }

            return source.ToString();
        }

        private static IXmlDocument ParseWithoutDepthLimit(String source, IConfiguration configuration = null)
        {
            var context = BrowsingContext.New(configuration ?? Configuration.Default.WithXml());
            return new XmlParser(new XmlParserOptions { MaximumDepth = Int32.MaxValue }, context).ParseDocument(source);
        }
    }
}
