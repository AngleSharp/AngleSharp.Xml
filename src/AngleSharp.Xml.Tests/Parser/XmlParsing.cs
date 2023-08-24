namespace AngleSharp.Xml.Tests.Parser
{
    using AngleSharp.Xml.Parser;
    using NUnit.Framework;
    using System;

    [TestFixture]
    public class XmlParsing
    {
        [Test]
        public void ParseHtmlStructureInXmlShouldThrowError()
        {
            var source = @" <title>My Title</title>
 <p>My paragraph</p>";
            var parser = new XmlParser();
            Assert.Catch<XmlParseException>(() => parser.ParseDocument(source));
        }

        [Test]
        public void ParseHtmlStructureInXmlShouldNotThrowErrorIfSuppressed()
        {
            var source = @" <title>My Title</title>
 <p>My paragraph</p>";
            var parser = new XmlParser(new XmlParserOptions
            {
                IsSuppressingErrors = true
            });
            var document = parser.ParseDocument(source);
            Assert.AreEqual(1, document.Children.Length);
        }

        [Test]
        public void ParseHtmlEntityInXmlShouldThrowError()
        {
            var source = @" <title>&nbsp;</title>";
            var parser = new XmlParser();
            Assert.Catch<XmlParseException>(() => parser.ParseDocument(source));
        }

        [Test]
        public void ParseHtmlEntityInXmlShouldNotThrowErrorIfSuppressed()
        {
            var source = @" <title>&nbsp;</title>";
            var parser = new XmlParser(new XmlParserOptions
            {
                IsSuppressingErrors = true
            });
            var document = parser.ParseDocument(source);
            Assert.AreEqual(1, document.Children.Length);
        }

        [Test]
        public void ParseValidXmlEntityShouldBeRepresentedCorrectly()
        {
            var source = @" <title>&amp;</title>";
            var parser = new XmlParser(new XmlParserOptions
            {
                IsSuppressingErrors = true
            });
            var document = parser.ParseDocument(source);
            Assert.AreEqual("&", document.DocumentElement.TextContent);
        }

        [Test]
        public void ParseInvalidXmlEntityShouldBeSerialized()
        {
            var source = @" <title>&nbsp;</title>";
            var parser = new XmlParser(new XmlParserOptions
            {
                IsSuppressingErrors = true
            });
            var document = parser.ParseDocument(source);
            Assert.AreEqual("&nbsp;", document.DocumentElement.TextContent);
        }

        [Test]
        public void ParseInvalidXmlShouldThrowWhenNotSuppressingErrors_Issue14()
        {
            Assert.Throws<XmlParseException>(() =>
            {
                var source = @"<P>
    <P>
        <FONT FACE=""calibri"" SIZE=""14.666666666666666"" COLOR=""#000000""></FONT>
    </P>
    <P>
        <FONT FACE=""calibri"" SIZE=""14.666666666666666"" COLOR=""#000000""></FONT>
    </P>
    <P>
        <FONT FACE=""calibri"" SIZE=""14.666666666666666"" COLOR=""#0000ff"">
            <U>
                <https://some.url.example.com></U>
            </FONT>
            <FONT FACE=""calibri"" SIZE=""14.666666666666666"" COLOR=""#000000"">
                <B></B>
            </FONT>
            <FONT FACE=""calibri"" SIZE=""14.666666666666666"" COLOR=""#000000""></FONT>
        </P>
        <P>
            <FONT FACE=""calibri"" SIZE=""14.666666666666666"" COLOR=""#000000""></FONT>
        </P>
        <P>
            <FONT FACE=""calibri"" SIZE=""14.666666666666666"" COLOR=""#000000""></FONT>
        </P>
    </P>";
                var parser = new XmlParser(new XmlParserOptions
                {
                    IsSuppressingErrors = false
                });
                parser.ParseDocument(source);
            });
        }

        [Test]
        public void ParseInvalidXmlShouldNotThrowWhenSuppressingErrors_Issue14()
        {
            Assert.DoesNotThrow(() =>
            {
                var source = @"<P>
    <P>
        <FONT FACE=""calibri"" SIZE=""14.666666666666666"" COLOR=""#000000""></FONT>
    </P>
    <P>
        <FONT FACE=""calibri"" SIZE=""14.666666666666666"" COLOR=""#000000""></FONT>
    </P>
    <P>
        <FONT FACE=""calibri"" SIZE=""14.666666666666666"" COLOR=""#0000ff"">
            <U>
                <https://some.url.example.com></U>
            </FONT>
            <FONT FACE=""calibri"" SIZE=""14.666666666666666"" COLOR=""#000000"">
                <B></B>
            </FONT>
            <FONT FACE=""calibri"" SIZE=""14.666666666666666"" COLOR=""#000000""></FONT>
        </P>
        <P>
            <FONT FACE=""calibri"" SIZE=""14.666666666666666"" COLOR=""#000000""></FONT>
        </P>
        <P>
            <FONT FACE=""calibri"" SIZE=""14.666666666666666"" COLOR=""#000000""></FONT>
        </P>
    </P>";
                var parser = new XmlParser(new XmlParserOptions
                {
                    IsSuppressingErrors = true
                });
                parser.ParseDocument(source);
            });
        }

        [Test]
        public async Task XmlPrefixedAttributesShouldLocateXmlNamespaceWithoutDeclaration()
        {
            var document = @"<xml xml:lang=""en""></xml>".ToXmlDocument();
            var root = document.DocumentElement;
            Assert.AreEqual(NamespaceNames.XmlUri, root.Attributes.Single().NamespaceUri);
        }
    }
}
