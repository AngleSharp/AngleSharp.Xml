namespace AngleSharp.Xml.Tests.Dom
{
    using AngleSharp.Io;
    using NUnit.Framework;
    using System;
    using System.Threading.Tasks;

    [TestFixture]
    public class XmlAttributeSemantics
    {
        [Test]
        public void XmlBaseResolvesAcrossAncestorChain()
        {
            var document = "<root xml:base='https://example.com/a/b/'><child xml:base='../assets/'><item /></child></root>".ToXmlDocument();
            var item = document.QuerySelector("item");

            Assert.AreEqual("https://example.com/a/assets/", item.GetXmlBaseUri());
            Assert.AreEqual("https://example.com/a/assets/", item.GetXmlBaseUrl().Href);
        }

        [TestCase("g", "http://a/b/c/g")]
        [TestCase("./g", "http://a/b/c/g")]
        [TestCase("g/", "http://a/b/c/g/")]
        [TestCase("/g", "http://a/g")]
        [TestCase("//g", "http://g")]
        [TestCase("g?y", "http://a/b/c/g?y")]
        [TestCase("g#s", "http://a/b/c/g#s")]
        [TestCase(";x", "http://a/b/c/;x")]
        [TestCase(".", "http://a/b/c/")]
        [TestCase("./", "http://a/b/c/")]
        [TestCase("..", "http://a/b/")]
        [TestCase("../", "http://a/b/")]
        [TestCase("../g", "http://a/b/g")]
        [TestCase("../..", "http://a/")]
        [TestCase("../../", "http://a/")]
        [TestCase("../../g", "http://a/g")]
        [TestCase("../../../g", "http://a/g")]
        [TestCase("../../../../g", "http://a/g")]
        [TestCase("/./g", "http://a/g")]
        [TestCase("/../g", "http://a/g")]
        [TestCase("g.", "http://a/b/c/g.")]
        [TestCase(".g", "http://a/b/c/.g")]
        [TestCase("g..", "http://a/b/c/g..")]
        [TestCase("..g", "http://a/b/c/..g")]
        [TestCase("./../g", "http://a/b/g")]
        [TestCase("./g/.", "http://a/b/c/g/")]
        [TestCase("g/./h", "http://a/b/c/g/h")]
        [TestCase("g/../h", "http://a/b/c/h")]
        [TestCase("g;x=1/./y", "http://a/b/c/g;x=1/y")]
        [TestCase("g;x=1/../y", "http://a/b/c/y")]
        public void XmlBaseResolvesReferencesAsInRfc3986(String reference, String expected)
        {
            // The reference resolution examples of RFC 3986, sections 5.4.1 and 5.4.2
            var document = $"<root xml:base='http://a/b/c/d;p?q'><item xml:base='{reference}' /></root>".ToXmlDocument();

            Assert.AreEqual(expected, document.QuerySelector("item").GetXmlBaseUri());
        }

        [Test]
        public void XmlBasePreservesNonAsciiCharactersAndSameDocumentReferences()
        {
            var document = "<root xml:base='http://example.org/wine/'><item xml:base='cellar%20one/rosé/%C3%A9'><child xml:base='' /></item></root>".ToXmlDocument();
            var item = document.QuerySelector("item");
            var child = document.QuerySelector("child");

            Assert.AreEqual("http://example.org/wine/cellar%20one/rosé/%C3%A9", item.GetXmlBaseUri());
            Assert.AreEqual(item.GetXmlBaseUri(), child.GetXmlBaseUri());
        }

        [Test]
        public async Task XmlBaseResolvesAgainstDocumentUrl()
        {
            var document = await BrowsingContext.New(Configuration.Default.WithXml()).OpenAsync(request =>
                request.Address("https://example.com/documents/source.xml")
                    .Content("<root xml:base='../assets/'><item /></root>")
                    .Header(HeaderNames.ContentType, MimeTypeNames.Xml));

            Assert.AreEqual("https://example.com/assets/", document.QuerySelector("item").GetXmlBaseUri());
        }

        [Test]
        public void XmlLanguageUsesNearestDeclarationAndSupportsReset()
        {
            var document = "<root xml:lang='en'><first><item /></first><second xml:lang='fr'><item /></second><third xml:lang=''><item /></third></root>".ToXmlDocument();
            var items = document.QuerySelectorAll("item");

            Assert.AreEqual("en", items[0].GetXmlLanguage());
            Assert.AreEqual("fr", items[1].GetXmlLanguage());
            Assert.AreEqual(String.Empty, items[2].GetXmlLanguage());
        }

        [Test]
        public void XmlIdIsNormalizedAndFoundInDocumentOrder()
        {
            var document = "<root><first xml:id='  item  one  ' /><second xml:id='item one' /></root>".ToXmlDocument();

            Assert.AreEqual("item one", document.DocumentElement.FirstElementChild.Attributes["xml:id"].Value);
            Assert.AreEqual("item one", document.DocumentElement.FirstElementChild.GetXmlId());
            Assert.AreSame(document.DocumentElement.FirstElementChild, document.GetElementByXmlId("item one"));
        }

        [Test]
        public void XmlIdLookupReflectsDomMutations()
        {
            var document = "<root><item xml:id='before' /></root>".ToXmlDocument();
            var item = document.DocumentElement.FirstElementChild;

            item.Attributes["xml:id"].Value = "after";

            Assert.IsNull(document.GetElementByXmlId("before"));
            Assert.AreSame(item, document.GetElementByXmlId("after"));
        }

        [Test]
        public void XmlConvenienceSemanticsIgnoreSameNamedUnqualifiedAttributes()
        {
            var document = "<root base='wrong' id='wrong' lang='wrong'><item /></root>".ToXmlDocument();
            var item = document.DocumentElement.FirstElementChild;

            Assert.AreEqual(item.BaseUri, item.GetXmlBaseUri());
            Assert.IsNull(document.DocumentElement.GetXmlId());
            Assert.IsNull(item.GetXmlLanguage());
            Assert.IsNull(document.GetElementByXmlId("wrong"));
        }
    }
}