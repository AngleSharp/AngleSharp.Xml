namespace AngleSharp.Xml.Tests
{
    using AngleSharp.Dom;
    using AngleSharp.Io;
    using AngleSharp.Svg.Dom;
    using AngleSharp.Text;
    using AngleSharp.Xml.Dom;
    using AngleSharp.Xml.Parser;
    using NUnit.Framework;
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    [TestFixture]
    public class VariousTests
    {
        private static readonly String XmlContent = @"<note>
<to>Tove</to>
<from>Jani</from>
<heading>Reminder</heading>
<body>Don't forget me this weekend!</body>
</note>";

        private static readonly String SvgContent = @"
<svg xmlns=""http://www.w3.org/2000/svg"" viewBox=""0 0 100 100"">
  <path d=""M34,93l11,-29a15,15 0,1,1 9,0l11,29a45,45 0,1,0 -31,0z"" stroke=""#142"" stroke-width=""2"" fill=""#4a5"" />
</svg>";

        [Test]
        public void ObtainElementPositionsFromXml()
        {
            var positions = new Dictionary<IElement, TextPosition>();
            var source = @"<hello>
   <foo />
   <bar>
      <test></test><test></test><test></test>
   </bar>
</hello>";
            var parser = new XmlParser(new XmlParserOptions
            {
                OnCreated = (element, position) => positions[element] = position
            });
            parser.ParseDocument(source);
            Assert.AreEqual(6, positions.Count);
        }

        [Test]
        public async Task GenerateDocumentFromXmlWithXmlContentType()
        {
            var document = await GenerateDocument(XmlContent, "text/xml");

            Assert.IsInstanceOf<XmlDocument>(document);
            Assert.AreEqual("note", document.DocumentElement.NodeName);
        }

        [Test]
        public async Task GenerateDocumentFromSvgWithSvgContentType()
        {
            var document = await GenerateDocument(SvgContent, "image/svg+xml");

            Assert.IsInstanceOf<SvgDocument>(document);
            Assert.AreEqual("svg", document.DocumentElement.NodeName);
            Assert.AreEqual("path", document.DocumentElement.FirstElementChild.NodeName);
        }

        [Test]
        public void SelfClosingTagsAreSerializedCorrectlyByDefaultFormatter_Issue11()
        {
            var parser = new XmlParser();
            var xmlDoc = parser.ParseDocument(
                @"<Project Sdk=""Microsoft.NET.Sdk"">
            <ItemGroup>
                <PackageReference Include=""AngleSharp"" Version=""0.12.1"" />
                <PackageReference></PackageReference>
            </ItemGroup>
        </Project>");
            var xml = xmlDoc.ToXml();
            Assert.AreEqual("<Project Sdk=\"Microsoft.NET.Sdk\">\n            <ItemGroup>\n                <PackageReference Include=\"AngleSharp\" Version=\"0.12.1\" />\n                <PackageReference></PackageReference>\n            </ItemGroup>\n        </Project>", xml);
        }

        [Test]
        public void EmptyTagsAreSerializedCorrectlyWithStandardFormatterWithOption_Issue11()
        {
            var parser = new XmlParser();
            var xmlDoc = parser.ParseDocument(
                @"<Project Sdk=""Microsoft.NET.Sdk"">
            <ItemGroup>
                <PackageReference Include=""AngleSharp"" Version=""0.12.1"" />
                <PackageReference></PackageReference>
            </ItemGroup>
        </Project>");
            var formatter = new XmlMarkupFormatter
            {
                IsAlwaysSelfClosing = true,
            };
            var xml = xmlDoc.ToHtml(formatter);
            Assert.AreEqual("<Project Sdk=\"Microsoft.NET.Sdk\">\n            <ItemGroup>\n                <PackageReference Include=\"AngleSharp\" Version=\"0.12.1\" />\n                <PackageReference />\n            </ItemGroup>\n        </Project>", xml);
        }

        private static Task<IDocument> GenerateDocument(String content, String contentType)
        {
            var config = Configuration.Default.WithDefaultLoader().WithXml();
            var context = BrowsingContext.New(config);
            return context.OpenAsync(res =>
            {
                res.Content(content);

                if (!String.IsNullOrEmpty(contentType))
                {
                    res.Header(HeaderNames.ContentType, contentType);
                }
            });
        }
    }
}
