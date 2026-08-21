namespace AngleSharp.Xml.Tests.Validation
{
    using NUnit.Framework;
    using System;
    using System.IO;
    using System.Linq;
    using System.Xml;
    using System.Xml.Schema;

    [TestFixture]
    public class XsdValidation
    {
        private const string Schema = @"<xs:schema xmlns:xs='http://www.w3.org/2001/XMLSchema' targetNamespace='urn:books' xmlns='urn:books' elementFormDefault='qualified'>
  <xs:element name='book'>
    <xs:complexType>
      <xs:sequence><xs:element name='title' type='xs:string'/></xs:sequence>
      <xs:attribute name='isbn' type='xs:string' use='required'/>
    </xs:complexType>
  </xs:element>
</xs:schema>";

        [Test]
        public void ExistingDocumentValidatesAgainstInlineSchema()
        {
            var document = "<book xmlns='urn:books' isbn='123'><title>XML</title></book>".ToXmlDocument();

            var result = document.ValidateXsd(Schema);

            Assert.IsTrue(result.IsValid);
            Assert.IsEmpty(result.Diagnostics);
        }

        [Test]
        public void InvalidDocumentReturnsDetailedDiagnostics()
        {
            var document = "<book xmlns='urn:books'><unexpected /></book>".ToXmlDocument();

            var result = document.ValidateXsd(Schema);

            Assert.IsFalse(result.IsValid);
            Assert.IsTrue(result.Diagnostics.Count >= 2);
            Assert.IsTrue(result.Diagnostics.All(m => m.Severity == XsdValidationSeverity.Error));
            Assert.IsTrue(result.Diagnostics.All(m => m.LineNumber > 0));
        }

        [Test]
        public void FailFastStopsAfterFirstValidationError()
        {
            var document = "<book xmlns='urn:books'><unexpected /></book>".ToXmlDocument();
            var options = new XsdValidationOptions { IsFailFast = true };

            var result = document.ValidateXsd(new[] { Schema }, options);

            Assert.IsFalse(result.IsValid);
            Assert.AreEqual(1, result.Diagnostics.Count);
        }

        [Test]
        public void ValidationWarningsCanBeCollectedOrSuppressed()
        {
            var document = "<unknown />".ToXmlDocument();

            var withWarnings = document.ValidateXsd(new[] { Schema }, new XsdValidationOptions());
            var withoutWarnings = document.ValidateXsd(new[] { Schema }, new XsdValidationOptions { IsReportingWarnings = false });

            Assert.IsTrue(withWarnings.IsValid);
            Assert.IsTrue(withWarnings.Diagnostics.Any(m => m.Severity == XsdValidationSeverity.Warning));
            Assert.IsTrue(withoutWarnings.IsValid);
            Assert.IsEmpty(withoutWarnings.Diagnostics);
        }

        [Test]
        public void MultipleSchemasSupportNamespaceAwareValidation()
        {
            var common = @"<xs:schema xmlns:xs='http://www.w3.org/2001/XMLSchema' targetNamespace='urn:common'><xs:simpleType name='Code'><xs:restriction base='xs:string'><xs:pattern value='[A-Z]+'/></xs:restriction></xs:simpleType></xs:schema>";
            var root = @"<xs:schema xmlns:xs='http://www.w3.org/2001/XMLSchema' xmlns:c='urn:common' targetNamespace='urn:root' xmlns='urn:root' elementFormDefault='qualified'><xs:import namespace='urn:common'/><xs:element name='root' type='c:Code'/></xs:schema>";
            var document = "<root xmlns='urn:root'>ABC</root>".ToXmlDocument();

            var result = document.ValidateXsd(new[] { common, root }, null);

            Assert.IsTrue(result.IsValid);
        }

        [Test]
        public void InvalidSchemaReturnsCompilationDiagnostic()
        {
            var document = "<root />".ToXmlDocument();
            var invalidSchema = "<xs:schema xmlns:xs='http://www.w3.org/2001/XMLSchema'><xs:element name='root' type='xs:missing'/></xs:schema>";

            var result = document.ValidateXsd(invalidSchema);

            Assert.IsFalse(result.IsValid);
            Assert.IsNotEmpty(result.Diagnostics);
        }

        [Test]
        public void MalformedSchemaReturnsParsingDiagnostic()
        {
            var document = "<root />".ToXmlDocument();

            var result = document.ValidateXsd("<xs:schema xmlns:xs='http://www.w3.org/2001/XMLSchema'>");

            Assert.IsFalse(result.IsValid);
            Assert.AreEqual(1, result.Diagnostics.Count);
            Assert.Greater(result.Diagnostics[0].LineNumber, 0);
        }

        [Test]
        public void InlineValidationRequiresAtLeastOneSchema()
        {
            var document = "<root />".ToXmlDocument();

            Assert.Throws<ArgumentException>(() => document.ValidateXsd(new string[0], null));
        }

        [Test]
        public void ExistingDoctypeIsNotReprocessedDuringXsdValidation()
        {
            var document = "<!DOCTYPE book><book xmlns='urn:books' isbn='123'><title>XML</title></book>".ToXmlDocument();

            var result = document.ValidateXsd(Schema);

            Assert.IsTrue(result.IsValid);
        }

        [Test]
        public void ConfiguredSchemaSetResolvesIncludes()
        {
            var directory = Path.Combine(Path.GetTempPath(), "anglesharp-xml-xsd-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(directory);

            try
            {
                var includedPath = Path.Combine(directory, "types.xsd");
                var rootPath = Path.Combine(directory, "root.xsd");
                File.WriteAllText(includedPath, "<xs:schema xmlns:xs='http://www.w3.org/2001/XMLSchema' targetNamespace='urn:include'><xs:simpleType name='Code'><xs:restriction base='xs:string'><xs:pattern value='[A-Z]+'/></xs:restriction></xs:simpleType></xs:schema>");
                File.WriteAllText(rootPath, "<xs:schema xmlns:xs='http://www.w3.org/2001/XMLSchema' targetNamespace='urn:include' xmlns='urn:include' elementFormDefault='qualified'><xs:include schemaLocation='types.xsd'/><xs:element name='root' type='Code'/></xs:schema>");
                var schemas = new XmlSchemaSet { XmlResolver = new XmlUrlResolver() };
                schemas.Add(null, rootPath);
                var document = "<root xmlns='urn:include'>ABC</root>".ToXmlDocument();

                var result = document.ValidateXsd(schemas);

                Assert.IsTrue(result.IsValid);
            }
            finally
            {
                Directory.Delete(directory, true);
            }
        }

        [Test]
        public void ConfiguredSchemaSetMustNotBeEmpty()
        {
            var document = "<root />".ToXmlDocument();

            Assert.Throws<ArgumentException>(() => document.ValidateXsd(new XmlSchemaSet()));
        }
    }
}