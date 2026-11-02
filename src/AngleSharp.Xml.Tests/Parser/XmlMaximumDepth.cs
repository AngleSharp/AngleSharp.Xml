namespace AngleSharp.Xml.Tests.Parser
{
    using AngleSharp.Dom;
    using AngleSharp.Xml.Parser;
    using NUnit.Framework;
    using System;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    /// <summary>
    /// The parser nests elements up to <see cref="XmlParserOptions.MaximumDepth"/>, counting the
    /// root element as level 1, as the HTML parser does with its option of the same name.
    /// </summary>
    [TestFixture]
    public class XmlMaximumDepth
    {
        [Test]
        public void DefaultIs512()
        {
            Assert.AreEqual(512, XmlParserOptions.DefaultMaximumDepth);
            Assert.AreEqual(512, new XmlParserOptions().MaximumDepth);
            Assert.AreEqual(512, default(XmlParserOptions).MaximumDepth);
        }

        [TestCase(0)]
        [TestCase(-1)]
        public void ZeroOrNegativeRestoresTheDefault(Int32 value)
        {
            var options = new XmlParserOptions { MaximumDepth = value };

            Assert.AreEqual(XmlParserOptions.DefaultMaximumDepth, options.MaximumDepth);
        }

        [Test]
        public void DocumentAtTheDefaultDepthParses()
        {
            var document = new XmlParser().ParseDocument(Nest(512));

            Assert.AreEqual(512, Depth(document.DocumentElement));
        }

        [Test]
        public void DocumentBeyondTheDefaultDepthThrows()
        {
            var exception = Assert.Throws<XmlParseException>(() => new XmlParser().ParseDocument(Nest(513)));

            Assert.AreEqual((Int32)XmlParseError.MaximumDepthExceeded, exception.Code);
        }

        [Test]
        public void DocumentBeyondTheDefaultDepthThrowsWhenParsedAsynchronously()
        {
            var exception = Assert.ThrowsAsync<XmlParseException>(async () => await new XmlParser().ParseDocumentAsync(Nest(513)));

            Assert.AreEqual((Int32)XmlParseError.MaximumDepthExceeded, exception.Code);
        }

        [TestCase(3, 3, false)]
        [TestCase(3, 4, true)]
        [TestCase(1, 1, false)]
        [TestCase(1, 2, true)]
        public void CustomLimitIsEnforced(Int32 maximumDepth, Int32 depth, Boolean throws)
        {
            var parser = new XmlParser(new XmlParserOptions { MaximumDepth = maximumDepth });

            if (throws)
            {
                Assert.Throws<XmlParseException>(() => parser.ParseDocument(Nest(depth)));
            }
            else
            {
                Assert.AreEqual(depth, Depth(parser.ParseDocument(Nest(depth)).DocumentElement));
            }
        }

        [Test]
        public void MaxValueLiftsTheLimit()
        {
            var parser = new XmlParser(new XmlParserOptions { MaximumDepth = Int32.MaxValue });

            Assert.AreEqual(2000, Depth(parser.ParseDocument(Nest(2000)).DocumentElement));
        }

        [Test]
        public void SelfClosingElementBeyondTheLimitThrows()
        {
            var parser = new XmlParser(new XmlParserOptions { MaximumDepth = 2 });

            Assert.Throws<XmlParseException>(() => parser.ParseDocument("<a><b><c/></b></a>"));
        }

        [Test]
        public async Task ElementsBeyondTheLimitAreAttachedAtTheLimitWhenErrorsAreSuppressed()
        {
            var parser = new XmlParser(new XmlParserOptions { MaximumDepth = 3, IsSuppressingErrors = true });
            var document = await parser.ParseDocumentAsync("<a><b><c><d>x<e/></d>y</c></b></a>");
            var b = document.DocumentElement.FirstElementChild;

            // d and e would be levels 4 and 5, so they become siblings of c; the text after the
            // closed d stays in c, as in the HTML parser
            Assert.AreEqual(3, Depth(document.DocumentElement));
            Assert.AreEqual("c,d,e", String.Join(",", b.Children.Select(m => m.LocalName)));
            Assert.AreEqual("y", b.Children[0].TextContent);
            Assert.AreEqual("x", b.Children[1].TextContent);
        }

        [Test]
        public void RootStaysTheParentAtTheLeastWhenErrorsAreSuppressed()
        {
            var parser = new XmlParser(new XmlParserOptions { MaximumDepth = 1, IsSuppressingErrors = true });
            var document = parser.ParseDocument("<a><b><c/></b></a>");

            Assert.AreEqual("b,c", String.Join(",", document.DocumentElement.Children.Select(m => m.LocalName)));
        }

        private static String Nest(Int32 depth)
        {
            var source = new StringBuilder(depth * 7);

            for (var i = 0; i < depth; i++)
            {
                source.Append("<a>");
            }

            for (var i = 0; i < depth; i++)
            {
                source.Append("</a>");
            }

            return source.ToString();
        }

        private static Int32 Depth(IElement root)
        {
            var depth = 0;
            var pending = new System.Collections.Generic.Stack<(IElement Element, Int32 Level)>();
            pending.Push((root, 1));

            while (pending.Count > 0)
            {
                var (element, level) = pending.Pop();
                depth = Math.Max(depth, level);

                foreach (var child in element.Children)
                {
                    pending.Push((child, level + 1));
                }
            }

            return depth;
        }
    }
}
