namespace AngleSharp.Xml.Tests.Canonicalization
{
    using AngleSharp.Dom;
    using AngleSharp.Io;
    using NUnit.Framework;
    using System;
    using System.IO;
    using System.Text;
    using System.Threading.Tasks;

    [TestFixture]
    public class XmlCanonicalization
    {
        [Test]
        public void CanonicalXml11NormalizesCoreSyntax()
        {
            var document = @"<?xml version=""1.0""?>
<!DOCTYPE doc>
<doc><e1/><e2 b=""2"" a=""1""><![CDATA[value>0 && value<10]]></e2></doc>".ToXmlDocument();

            var result = Encoding.UTF8.GetString(document.ToCanonicalXml());

            Assert.AreEqual("<doc><e1></e1><e2 a=\"1\" b=\"2\">value&gt;0 &amp;&amp; value&lt;10</e2></doc>", result);
        }

        [Test]
        public void CanonicalXml11OrdersNamespacesAndAttributes()
        {
            var document = @"<e5 a:attr=""out"" b:attr=""sorted"" attr2=""all"" attr=""I'm""
                xmlns:b=""http://www.ietf.org"" xmlns:a=""http://www.w3.org"" xmlns=""http://example.org""/>".ToXmlDocument();

            var result = Encoding.UTF8.GetString(document.ToCanonicalXml());

            Assert.AreEqual("<e5 xmlns=\"http://example.org\" xmlns:a=\"http://www.w3.org\" xmlns:b=\"http://www.ietf.org\" attr=\"I'm\" attr2=\"all\" b:attr=\"sorted\" a:attr=\"out\"></e5>", result);
        }

        [Test]
        public void CanonicalXml11SubtreeIncludesAncestorContext()
        {
            var document = @"<root xmlns=""urn:root"" xmlns:u=""urn:used"" xmlns:z=""urn:unused""
                xml:lang=""en"" xml:id=""root"" xml:base=""base/""><middle xml:base=""part/""><u:item /></middle></root>".ToXmlDocument();
            var item = document.QuerySelector("item");

            var result = Encoding.UTF8.GetString(item.ToCanonicalXml());

            Assert.AreEqual("<u:item xmlns=\"urn:root\" xmlns:u=\"urn:used\" xmlns:z=\"urn:unused\" xml:base=\"base/part/\" xml:lang=\"en\"></u:item>", result);
        }

        [Test]
        public void CanonicalXml11ResolvesOmittedAncestorXmlBases()
        {
            var document = @"<a xml:base=""foo/bar""><b xml:base=""..""><c xml:base=""..""><d xml:base=""x"" /></c></b></a>".ToXmlDocument();
            var element = document.QuerySelector("d");

            var result = Encoding.UTF8.GetString(element.ToCanonicalXml());

            Assert.AreEqual("<d xml:base=\"x\"></d>", result);
        }

        [Test]
        public void ExclusiveXml10OnlyIncludesVisibleAndRequestedNamespaces()
        {
            var document = @"<root xmlns=""urn:root"" xmlns:u=""urn:used"" xmlns:z=""urn:requested"" xml:lang=""en""><u:item /></root>".ToXmlDocument();
            var item = document.QuerySelector("item");
            var options = new XmlCanonicalizationOptions
            {
                Mode = XmlCanonicalizationMode.ExclusiveXml10,
                InclusiveNamespacePrefixes = new[] { "z" },
            };

            var result = Encoding.UTF8.GetString(item.ToCanonicalXml(options));

            Assert.AreEqual("<u:item xmlns:u=\"urn:used\" xmlns:z=\"urn:requested\"></u:item>", result);
        }

        [Test]
        public void CanonicalXml11ControlsCommentsAndTopLevelLineBreaks()
        {
            var document = "<?before data?><!--before--><root><!--inside--></root><!--after--><?after?>".ToXmlDocument();
            var withoutComments = Encoding.UTF8.GetString(document.ToCanonicalXml());
            var withComments = Encoding.UTF8.GetString(document.ToCanonicalXml(new XmlCanonicalizationOptions { IncludeComments = true }));

            Assert.AreEqual("<?before data?>\n<root></root>\n<?after?>", withoutComments);
            Assert.AreEqual("<?before data?>\n<!--before-->\n<root><!--inside--></root>\n<!--after-->\n<?after?>", withComments);
        }

        [Test]
        public void CanonicalXml11RejectsRelativeNamespaceUris()
        {
            var document = "<root xmlns='relative/path' />".ToXmlDocument();

            Assert.Throws<InvalidOperationException>(() => document.ToCanonicalXml());
        }

        [Test]
        public void CanonicalXmlWritesUtf8WithoutClosingStream()
        {
            var document = "<root>©</root>".ToXmlDocument();
            var stream = new MemoryStream();

            document.ToCanonicalXml(stream);
            stream.WriteByte(0x21);

            Assert.AreEqual("<root>©</root>!", Encoding.UTF8.GetString(stream.ToArray()));
        }

        [Test]
        public void CanonicalXmlRejectsUnsupportedNodeRoots()
        {
            var document = "<root>text</root>".ToXmlDocument();

            Assert.Throws<ArgumentException>(() => document.DocumentElement.FirstChild.ToCanonicalXml());
        }

        [Test]
        public async Task CanonicalXmlSupportsSvgDocuments()
        {
            var document = await BrowsingContext.New(Configuration.Default.WithXml()).OpenAsync(request =>
                request.Content("<svg xmlns='http://www.w3.org/2000/svg'><path /></svg>")
                    .Header(HeaderNames.ContentType, MimeTypeNames.Svg));

            var result = Encoding.UTF8.GetString(document.ToCanonicalXml());

            Assert.AreEqual("<svg xmlns=\"http://www.w3.org/2000/svg\"><path></path></svg>", result);
        }
    }
}