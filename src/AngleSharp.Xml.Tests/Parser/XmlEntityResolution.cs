namespace AngleSharp.Xml.Tests.Parser
{
    using AngleSharp.Dom;
    using AngleSharp.Io;
    using AngleSharp.Xml.Parser;
    using NUnit.Framework;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Net;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// References to DTD-declared entities are resolved at the end of the parse, once the whole
    /// DTD is known, so the parse never waits for the external subset.
    /// </summary>
    [TestFixture]
    public class XmlEntityResolution
    {
        [Test]
        public async Task ParsingGoesOnWhileTheExternalSubsetLoads()
        {
            var requester = new HeldRequester("<!ENTITY greet \"Hello\">");
            var configuration = Configuration.Default
                .WithXml()
                .With(requester)
                .WithDefaultLoader(new LoaderOptions { IsResourceLoadingEnabled = true });
            var created = new List<String>();
            var parser = new XmlParser(new XmlParserOptions { OnCreated = (element, position) => created.Add(element.LocalName) }, BrowsingContext.New(configuration));

            var parsing = parser.ParseDocumentAsync(@"<!DOCTYPE r SYSTEM ""http://dtd.test/r.dtd""><r><a>&greet;</a><b/></r>");

            // The DTD is still held back, yet the whole body has been parsed
            CollectionAssert.AreEqual(new[] { "r", "a", "b" }, created);
            Assert.IsFalse(parsing.IsCompleted);

            requester.Release();
            var document = await parsing;

            Assert.AreEqual("Hello", document.QuerySelector("a").TextContent);
        }

        [Test]
        public void EntitiesAreResolvedInTextAndAttributes()
        {
            var document = @"<!DOCTYPE r [<!ENTITY e ""x"">]><r v=""1&e;2"">a&e;b<i>&e;</i>c&e;</r>".ToXmlDocument();

            Assert.AreEqual("1x2", document.DocumentElement.GetAttribute("v"));
            Assert.AreEqual("axbxcx", document.DocumentElement.TextContent);
        }

        [Test]
        public void ElementCallbackSeesTheReferencesAsWritten()
        {
            var seen = default(String);
            var parser = new XmlParser(new XmlParserOptions { OnCreated = (element, position) => seen = element.GetAttribute("v") });
            var document = parser.ParseDocument(@"<!DOCTYPE r [<!ENTITY e ""x"">]><r v=""&e;""/>");

            Assert.AreEqual("&e;", seen);
            Assert.AreEqual("x", document.DocumentElement.GetAttribute("v"));
        }

        [Test]
        public void UnknownEntityThrowsWithThePositionOfItsText()
        {
            var exception = Assert.Throws<XmlParseException>(() => new XmlParser().ParseDocument("<r>\n  a&unknown;</r>"));

            // Reported at the end of the parse, with the position the tokenizer reported it at:
            // where the text holding the reference starts
            Assert.AreEqual((Int32)XmlParseError.CharacterReferenceInvalidCode, exception.Code);
            Assert.AreEqual(1, exception.Position.Line);
            Assert.AreEqual(4, exception.Position.Column);
        }

        [Test]
        public void UnknownEntityStaysAsWrittenWhenErrorsAreSuppressed()
        {
            var parser = new XmlParser(new XmlParserOptions { IsSuppressingErrors = true });
            var document = parser.ParseDocument(@"<r v=""&unknown;"">&unknown;</r>");

            Assert.AreEqual("&unknown;", document.DocumentElement.GetAttribute("v"));
            Assert.AreEqual("&unknown;", document.DocumentElement.TextContent);
        }

        [Test]
        public void EmptyEntityResolvesToNothing()
        {
            var document = @"<!DOCTYPE r [<!ENTITY e """">]><r v=""a&e;b"">a&e;b</r>".ToXmlDocument();

            Assert.AreEqual("ab", document.DocumentElement.GetAttribute("v"));
            Assert.AreEqual("ab", document.DocumentElement.TextContent);
        }

        [Test]
        public void EscapedReferenceStaysText()
        {
            var document = @"<!DOCTYPE r [<!ENTITY e ""x"">]><r v=""&amp;e;"">&amp;e;</r>".ToXmlDocument();

            Assert.AreEqual("&e;", document.DocumentElement.GetAttribute("v"));
            Assert.AreEqual("&e;", document.DocumentElement.TextContent);
        }

        [Test]
        public void XmlIdIsTrimmedAndNormalizedAfterItsEntitiesAreResolved()
        {
            var document = @"<!DOCTYPE r [<!ENTITY e "" a   b "">]><r xml:id="" &e; ""/>".ToXmlDocument();

            Assert.AreEqual("a b", document.DocumentElement.GetXmlId());
        }

        /// <summary>
        /// Serves one DTD for http://dtd.test/, but only once the test releases it.
        /// </summary>
        private sealed class HeldRequester : BaseRequester
        {
            private readonly TaskCompletionSource<IResponse> _response = new TaskCompletionSource<IResponse>(TaskCreationOptions.RunContinuationsAsynchronously);
            private readonly String _content;

            public HeldRequester(String content) => _content = content;

            public void Release() => _response.SetResult(new DefaultResponse
            {
                Address = new Url("http://dtd.test/r.dtd"),
                StatusCode = HttpStatusCode.OK,
                Content = new MemoryStream(Encoding.UTF8.GetBytes(_content)),
            });

            public override Boolean SupportsProtocol(String protocol) => protocol == "http";

            protected override Task<IResponse> PerformRequestAsync(Request request, CancellationToken cancel) => _response.Task;
        }
    }
}
