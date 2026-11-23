namespace AngleSharp.Xml.Tests.Dom
{
    using AngleSharp.Dom;
    using AngleSharp.Io;
    using AngleSharp.Xml.Parser;
    using NUnit.Framework;
    using System;
    using System.Linq;
    using System.Threading.Tasks;

    /// <summary>
    /// Behavior the Acid3 test (acid3.acidtests.org) relies on.
    /// </summary>
    [TestFixture]
    public class Acid3Tests
    {
        private const String XhtmlNamespace = "http://www.w3.org/1999/xhtml";

        [Test]
        public void TitleOfAnXhtmlDocumentUpdatesWithItsElement()
        {
            var document = BrowsingContext.New(Configuration.Default.WithXml()).OpenNewAsync().Result;
            var doctype = document.Implementation.CreateDocumentType("html", "-//W3C//DTD XHTML 1.0 Strict//EN", "http://www.w3.org/TR/xhtml1/DTD/xhtml1-strict.dtd");
            var xhtml = document.Implementation.CreateDocument(XhtmlNamespace, "html", doctype);
            xhtml.DocumentElement.AppendChild(xhtml.CreateElement(XhtmlNamespace, "head"));
            xhtml.DocumentElement.AppendChild(xhtml.CreateElement(XhtmlNamespace, "body"));
            var title = xhtml.CreateElement(XhtmlNamespace, "title");
            xhtml.DocumentElement.FirstChild.AppendChild(title);

            Assert.AreEqual(String.Empty, xhtml.Title);

            title.TextContent = " Sparrow  ";

            Assert.AreEqual("Sparrow", xhtml.Title);
        }

        [Test]
        public async Task DocumentThatIsNotWellFormedFailsToOpen()
        {
            // showing the error instead is up to the application (IDocumentErrorHandler)
            var context = BrowsingContext.New(Configuration.Default.WithXml());

            try
            {
                await context.OpenAsync(res => res
                    .Address("http://localhost/xhtml.2")
                    .Content("<html xmlns='http://www.w3.org/1999/xhtml'><body><p><strong/> Parsing Test </strong></p></body></html>")
                    .Header(HeaderNames.ContentType, MimeTypeNames.Xml));
                Assert.Fail("The parse error should propagate.");
            }
            catch (XmlParseException)
            {
            }
        }
    }
}
