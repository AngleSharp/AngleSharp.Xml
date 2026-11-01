namespace AngleSharp.Xml.Tests.Parser
{
    using AngleSharp.Xml.Parser;
    using NUnit.Framework;
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Entities declared in the DTD expand within <see cref="XmlParserOptions.MaxCharactersFromEntities"/>.
    /// </summary>
    [TestFixture]
    public class XmlEntityExpansion
    {
        private const Int32 EntityExpansionLimitExceeded = 0x313;

        [Test]
        public void MaxCharactersFromEntitiesDefaultsToTenMillion()
        {
            var options = new XmlParserOptions();

            Assert.AreEqual(10_000_000, options.MaxCharactersFromEntities);
            Assert.AreEqual(XmlParserOptions.DefaultMaxCharactersFromEntities, default(XmlParserOptions).MaxCharactersFromEntities);
        }

        [Test]
        public void NegativeMaxCharactersFromEntitiesIsRejected()
        {
            var options = new XmlParserOptions();

            Assert.Throws<ArgumentOutOfRangeException>(() => options.MaxCharactersFromEntities = -1);
        }

        [Test]
        public void ExponentialEntityExpansionThrowsAtTheDefaultLimit()
        {
            // Six levels of twenty references each pass 10^7 characters in the fourth expansion round
            var parser = new XmlParser();

            var exception = Assert.Throws<XmlParseException>(() => parser.ParseDocument(NestedEntities(6, 20)));

            Assert.AreEqual(EntityExpansionLimitExceeded, exception.Code);
        }

        [Test]
        public void ExponentialEntityExpansionThrowsWhenParsingAsynchronously()
        {
            var parser = new XmlParser();

            var exception = Assert.ThrowsAsync<XmlParseException>(() => parser.ParseDocumentAsync(NestedEntities(6, 20), CancellationToken.None));

            Assert.AreEqual(EntityExpansionLimitExceeded, exception.Code);
        }

        [Test]
        public void EntityExpansionWithinTheLimitIsKept()
        {
            var document = new XmlParser().ParseDocument(NestedEntities(3, 10));

            Assert.AreEqual(10 * 10 * 10 * 10, document.DocumentElement.TextContent.Length);
        }

        [TestCase(6, false)]
        [TestCase(5, true)]
        public void TextExpansionCountsAgainstTheLimit(Int64 limit, Boolean throws)
        {
            var parser = new XmlParser(new XmlParserOptions { MaxCharactersFromEntities = limit });
            const String Source = "<!DOCTYPE r [<!ENTITY e \"abc\">]><r>&e;&e;</r>";

            if (throws)
            {
                var exception = Assert.Throws<XmlParseException>(() => parser.ParseDocument(Source));
                Assert.AreEqual(EntityExpansionLimitExceeded, exception.Code);
            }
            else
            {
                Assert.AreEqual("abcabc", parser.ParseDocument(Source).DocumentElement.TextContent);
            }
        }

        [TestCase(6, false)]
        [TestCase(5, true)]
        public void AttributeExpansionCountsAgainstTheLimit(Int64 limit, Boolean throws)
        {
            var parser = new XmlParser(new XmlParserOptions { MaxCharactersFromEntities = limit });
            const String Source = "<!DOCTYPE r [<!ENTITY e \"abc\">]><r a=\"&e;&e;\"/>";

            if (throws)
            {
                var exception = Assert.Throws<XmlParseException>(() => parser.ParseDocument(Source));
                Assert.AreEqual(EntityExpansionLimitExceeded, exception.Code);
            }
            else
            {
                Assert.AreEqual("abcabc", parser.ParseDocument(Source).DocumentElement.GetAttribute("a"));
            }
        }

        [Test]
        public void ZeroMeansNoLimit()
        {
            var parser = new XmlParser(new XmlParserOptions { MaxCharactersFromEntities = 0 });

            var document = parser.ParseDocument(NestedEntities(4, 10));

            Assert.AreEqual(100_000, document.DocumentElement.TextContent.Length);
        }

        [Test]
        public void BuiltInEntitiesDoNotCountAgainstTheLimit()
        {
            var parser = new XmlParser(new XmlParserOptions { MaxCharactersFromEntities = 1 });

            var document = parser.ParseDocument("<r>&amp;&lt;&gt;&apos;&quot;&#65;</r>");

            Assert.AreEqual("&<>'\"A", document.DocumentElement.TextContent);
        }

        [Test]
        public void SuppressedErrorsKeepReferencesBeyondTheLimitAsText()
        {
            var parser = new XmlParser(new XmlParserOptions { MaxCharactersFromEntities = 5, IsSuppressingErrors = true });

            var document = parser.ParseDocument("<!DOCTYPE r [<!ENTITY e \"abc\">]><r>&e;&e;</r>");

            Assert.AreEqual("abc&e;", document.DocumentElement.TextContent);
        }

        [Test]
        public void SuppressedErrorsStopExponentialExpansionAtTheLimit()
        {
            var parser = new XmlParser(new XmlParserOptions { IsSuppressingErrors = true });

            var document = parser.ParseDocument(NestedEntities(6, 20));

            Assert.LessOrEqual(document.DocumentElement.TextContent.Length, XmlParserOptions.DefaultMaxCharactersFromEntities);
        }

        /// <summary>
        /// Entity e0 holds ten characters; every further level holds <paramref name="fanOut"/>
        /// references to the one below, and the root references the top level once.
        /// </summary>
        private static String NestedEntities(Int32 levels, Int32 fanOut)
        {
            var declarations = "<!ENTITY e0 \"0123456789\">" + String.Concat(Enumerable.Range(1, levels).Select(level =>
                $"<!ENTITY e{level} \"{String.Concat(Enumerable.Repeat($"&e{level - 1};", fanOut))}\">"));
            return $"<!DOCTYPE r [{declarations}]><r>&e{levels};</r>";
        }
    }
}
