namespace AngleSharp.Xml.Tests.Parser
{
    using NUnit.Framework;
    using System;
    using System.IO;

    [TestFixture]
    public class XmlExternalDtdSupport
    {
        [Test]
        public void LoadsExternalSubsetFromSystemIdentifierAndValidates()
        {
            var directory = CreateTempDirectory();

            try
            {
                var dtdPath = Path.Combine(directory, "schema.dtd");
                File.WriteAllText(dtdPath, "<!ELEMENT root (item)><!ELEMENT item (#PCDATA)><!ATTLIST item code CDATA #REQUIRED>");

                var xml = $@"<!DOCTYPE root SYSTEM ""{dtdPath}"">
<root><item>missing-code</item></root>";
                var document = xml.ToXmlDocument();

                Assert.IsNotNull(document);
                Assert.IsFalse(document.IsValid);
            }
            finally
            {
                DeleteDirectory(directory);
            }
        }

        [Test]
        public void LoadsExternalEntityFromExternalSubset()
        {
            var directory = CreateTempDirectory();

            try
            {
                var dtdPath = Path.Combine(directory, "entities.dtd");
                File.WriteAllText(dtdPath, "<!ELEMENT root (#PCDATA)><!ENTITY greet \"Hello from DTD\">");

                var xml = $@"<!DOCTYPE root SYSTEM ""{dtdPath}"">
<root>&greet;</root>";
                var document = xml.ToXmlDocument();

                Assert.IsNotNull(document);
                Assert.IsNotNull(document.DocumentElement);
                Assert.AreEqual("Hello from DTD", document.DocumentElement.TextContent);
            }
            finally
            {
                DeleteDirectory(directory);
            }
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
