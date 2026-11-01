namespace AngleSharp.Xml.Tests.Parser
{
    using NUnit.Framework;
    using System;
    using System.IO;
    using System.Text;

    /// <summary>
    /// Deeply nested documents must not overflow the call stack, which would end the process.
    /// </summary>
    [TestFixture]
    public class XmlDeepNesting
    {
        // Recursive tree walks overflowed from about 5000 levels on a 1 MB stack; building the tree
        // itself is quadratic in depth (AngleSharp's AppendChild walks the ancestors), so going much
        // deeper only makes the tests slow
        private const Int32 Depth = 20000;

        [Test]
        public void DeeplyNestedDocumentWithoutDoctypeParses()
        {
            var document = Nest(String.Empty, "x").ToXmlDocument();

            Assert.IsNotNull(document.DocumentElement);
            Assert.IsTrue(document.IsValid);
        }

        [Test]
        public void DeeplyNestedDocumentIsValidAgainstInternalSubset()
        {
            var document = Nest("<!DOCTYPE a [<!ELEMENT a ANY>]>", "x").ToXmlDocument();

            Assert.IsTrue(document.IsValid);
        }

        [Test]
        public void UndeclaredElementAtDeepestLevelIsInvalidAgainstInternalSubset()
        {
            var document = Nest("<!DOCTYPE a [<!ELEMENT a ANY>]>", "<b/>").ToXmlDocument();

            Assert.IsFalse(document.IsValid);
        }

        [Test]
        public void DuplicateIdAtDeepestLevelIsInvalid()
        {
            var doctype = "<!DOCTYPE a [<!ELEMENT a ANY><!ELEMENT b EMPTY><!ATTLIST b id ID #IMPLIED>]>";
            var document = Nest(doctype, "<b id=\"one\"/><b id=\"one\"/>").ToXmlDocument();

            Assert.IsFalse(document.IsValid);
        }

        [Test]
        public void DeeplyNestedDocumentIsValidAgainstExternalSubset()
        {
            var directory = CreateTempDirectory();

            try
            {
                var dtdPath = Path.Combine(directory, "deep.dtd");
                File.WriteAllText(dtdPath, "<!ELEMENT a ANY>");

                var valid = Nest($@"<!DOCTYPE a SYSTEM ""{dtdPath}"">", "x").ToXmlDocument();
                var invalid = Nest($@"<!DOCTYPE a SYSTEM ""{dtdPath}"">", "<b/>").ToXmlDocument();

                Assert.IsTrue(valid.IsValid);
                Assert.IsFalse(invalid.IsValid);
            }
            finally
            {
                DeleteDirectory(directory);
            }
        }

        [Test]
        public void TextContentOfDeeplyNestedElementIncludesTextAndCData()
        {
            var document = Nest(String.Empty, "x<![CDATA[y]]>").ToXmlDocument();

            Assert.AreEqual("xy", document.DocumentElement.TextContent);
        }

        [Test]
        public void TextContentKeepsDocumentOrderAndSkipsCommentsAndProcessingInstructions()
        {
            var document = "<r>a<!--c--><?pi x?><b>b<![CDATA[c]]><c>d</c></b>e<d/></r>".ToXmlDocument();

            Assert.AreEqual("abcde", document.DocumentElement.TextContent);
        }

        private static String Nest(String prolog, String innermost)
        {
            var source = new StringBuilder(prolog.Length + innermost.Length + Depth * 7);
            source.Append(prolog);

            for (var i = 0; i < Depth; i++)
            {
                source.Append("<a>");
            }

            source.Append(innermost);

            for (var i = 0; i < Depth; i++)
            {
                source.Append("</a>");
            }

            return source.ToString();
        }

        private static String CreateTempDirectory()
        {
            var path = Path.Combine(Path.GetTempPath(), "anglesharp-xml-tests-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(path);
            return path;
        }

        private static void DeleteDirectory(String path)
        {
            if (Directory.Exists(path))
            {
                Directory.Delete(path, true);
            }
        }
    }
}
