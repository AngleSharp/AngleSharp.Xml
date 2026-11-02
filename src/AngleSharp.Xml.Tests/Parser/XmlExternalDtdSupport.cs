namespace AngleSharp.Xml.Tests.Parser
{
    using AngleSharp.Io;
    using AngleSharp.Xml.Dom;
    using AngleSharp.Xml.Parser;
    using AngleSharp.Xml.Tests.Mocks;
    using NUnit.Framework;
    using System;
    using System.IO;
    using System.Threading.Tasks;

    /// <summary>
    /// The external DTD subset is fetched through the browsing context's resource loader and its
    /// requesters, so it only loads with resource loading enabled and a requester for its protocol.
    /// </summary>
    [TestFixture]
    public class XmlExternalDtdSupport
    {
        private const String SchemaUrl = "mock://dtd.test/schema.dtd";
        private const String EntitiesUrl = "mock://dtd.test/entities.dtd";
        private const String Schema = "<!ELEMENT root (item)><!ELEMENT item (#PCDATA)><!ATTLIST item code CDATA #REQUIRED>";
        private const String Entities = "<!ELEMENT root (#PCDATA)><!ENTITY greet \"Hello from DTD\">";

        [Test]
        public void LoadsExternalSubsetFromSystemIdentifierAndValidates()
        {
            var requester = new MockRequester().Serve(SchemaUrl, Schema);
            var document = Parse(requester.CreateConfiguration(), $@"<!DOCTYPE root SYSTEM ""{SchemaUrl}""><root><item>missing-code</item></root>");

            Assert.IsFalse(document.IsValid);
            CollectionAssert.AreEqual(new[] { SchemaUrl }, requester.RequestedAddresses);
        }

        [Test]
        public void LoadsExternalEntityFromExternalSubset()
        {
            var requester = new MockRequester().Serve(EntitiesUrl, Entities);
            var document = Parse(requester.CreateConfiguration(), $@"<!DOCTYPE root SYSTEM ""{EntitiesUrl}""><root>&greet;</root>");

            Assert.AreEqual("Hello from DTD", document.DocumentElement.TextContent);
        }

        [Test]
        public async Task LoadsExternalSubsetWhenParsedAsynchronously()
        {
            var requester = new MockRequester().Serve(EntitiesUrl, Entities);
            var context = BrowsingContext.New(requester.CreateConfiguration());
            var document = await context.GetService<IXmlParser>().ParseDocumentAsync($@"<!DOCTYPE root SYSTEM ""{EntitiesUrl}""><root>&greet;</root>");

            Assert.AreEqual("Hello from DTD", document.DocumentElement.TextContent);
        }

        [Test]
        public async Task ResolvesRelativeSystemIdentifierAgainstDocumentUrl()
        {
            // http: rather than mock:, as AngleSharp resolves relative references against special schemes
            var requester = new MockRequester().Serve("http://dtd.test/docs/entities.dtd", Entities);
            var context = BrowsingContext.New(requester.CreateConfiguration());
            var document = await context.OpenAsync(response => response
                .Address("http://dtd.test/docs/document.xml")
                .Header(HeaderNames.ContentType, MimeTypeNames.Xml)
                .Content(@"<!DOCTYPE root SYSTEM ""entities.dtd""><root>&greet;</root>"));

            Assert.AreEqual("Hello from DTD", document.DocumentElement.TextContent);
        }

        [Test]
        public void DoesNotLoadExternalSubsetWithoutResourceLoading()
        {
            var requester = new MockRequester().Serve(SchemaUrl, Schema);
            var document = Parse(requester.CreateConfiguration(isResourceLoadingEnabled: false), $@"<!DOCTYPE root SYSTEM ""{SchemaUrl}""><root><item>missing-code</item></root>");

            Assert.IsTrue(document.IsValid);
            CollectionAssert.IsEmpty(requester.RequestedAddresses);
        }

        [Test]
        public void DoesNotLoadExternalSubsetWithoutLoader()
        {
            var document = Parse(Configuration.Default.WithXml(), $@"<!DOCTYPE root SYSTEM ""{SchemaUrl}""><root><item>missing-code</item></root>");

            Assert.IsTrue(document.IsValid);
        }

        [Test]
        public void DoesNotReadLocalFilesWithoutFileRequester()
        {
            var directory = Path.Combine(Path.GetTempPath(), "anglesharp-xml-tests-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(directory);

            try
            {
                var dtdPath = Path.Combine(directory, "entities.dtd");
                File.WriteAllText(dtdPath, Entities);
                var requester = new MockRequester();
                var xml = $@"<!DOCTYPE root SYSTEM ""{new Uri(dtdPath).AbsoluteUri}""><root>&greet;</root>";
                var lenient = new XmlParser(new XmlParserOptions { IsSuppressingErrors = true }, BrowsingContext.New(requester.CreateConfiguration()));

                // Only the requesters decide what is fetched, and none serves file:
                Assert.AreEqual("&greet;", lenient.ParseDocument(xml).DocumentElement.TextContent);
                CollectionAssert.IsEmpty(requester.RequestedAddresses);
            }
            finally
            {
                Directory.Delete(directory, true);
            }
        }

        [Test]
        public void IgnoresExternalSubsetThatCannotBeLoaded()
        {
            var requester = new MockRequester();
            var document = Parse(requester.CreateConfiguration(), $@"<!DOCTYPE root SYSTEM ""{SchemaUrl}""><root><item>missing-code</item></root>");

            Assert.IsTrue(document.IsValid);
            CollectionAssert.AreEqual(new[] { SchemaUrl }, requester.RequestedAddresses);
        }

        private static IXmlDocument Parse(IConfiguration configuration, String source) =>
            BrowsingContext.New(configuration).GetService<IXmlParser>().ParseDocument(source);
    }
}
