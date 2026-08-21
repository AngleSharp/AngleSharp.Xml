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