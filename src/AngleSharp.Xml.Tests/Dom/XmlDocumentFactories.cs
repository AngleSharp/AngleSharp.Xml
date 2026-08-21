namespace AngleSharp.Xml.Tests.Dom
{
    using AngleSharp.Dom;
    using AngleSharp.Xml.Dom;
    using NUnit.Framework;
    using System;

    [TestFixture]
    public class XmlDocumentFactories
    {
        [Test]
        public void DeclarationMetadataIsExposed()
        {
            var document = "<?xml version='1.0' encoding='ISO-8859-1' standalone='yes'?><root />".ToXmlDocument();

            Assert.AreEqual("1.0", document.XmlVersion);
            Assert.AreEqual("ISO-8859-1", document.XmlEncoding);
            Assert.IsTrue(document.XmlStandalone);
        }

        [Test]
        public void DeclarationMetadataUsesXmlDefaultsWhenOmitted()
        {
            var document = "<root />".ToXmlDocument();

            Assert.AreEqual("1.0", document.XmlVersion);
            Assert.IsNull(document.XmlEncoding);
            Assert.IsFalse(document.XmlStandalone);
        }

        [Test]
        public void ClonePreservesDeclarationMetadata()
        {
            var document = "<?xml version='1.0' encoding='utf-8' standalone='yes'?><root />".ToXmlDocument();
            var clone = (IXmlDocument)document.Clone();

            Assert.AreEqual(document.XmlVersion, clone.XmlVersion);
            Assert.AreEqual(document.XmlEncoding, clone.XmlEncoding);
            Assert.AreEqual(document.XmlStandalone, clone.XmlStandalone);
        }

        [Test]
        public void CreateCDataSectionPreservesLiteralMarkup()
        {
            var document = "<root />".ToXmlDocument();
            var section = document.CreateCDataSection("<message>&value</message>");

            document.DocumentElement.AppendChild(section);

            Assert.AreEqual(NodeType.CharacterData, section.NodeType);
            Assert.AreEqual("<root><![CDATA[<message>&value</message>]]></root>", document.ToXml());
        }

        [Test]
        public void ParsedCDataRemainsCData()
        {
            var document = "<root><![CDATA[<value>]]></root>".ToXmlDocument();

            Assert.IsInstanceOf<IXmlCDataSection>(document.DocumentElement.FirstChild);
            Assert.AreEqual("<value>", document.DocumentElement.TextContent);
            Assert.AreEqual("<root><![CDATA[<value>]]></root>", document.ToXml());
        }

        [Test]
        public void CDataLexicalBoundariesSurviveRoundTrip()
        {
            const String source = "<root><![CDATA[a]]>text<![CDATA[]]><![CDATA[b]]></root>";
            var document = source.ToXmlDocument();

            Assert.AreEqual(4, document.DocumentElement.ChildNodes.Length);
            Assert.IsInstanceOf<IXmlCDataSection>(document.DocumentElement.ChildNodes[0]);
            Assert.AreEqual(NodeType.Text, document.DocumentElement.ChildNodes[1].NodeType);
            Assert.IsInstanceOf<IXmlCDataSection>(document.DocumentElement.ChildNodes[2]);
            Assert.IsInstanceOf<IXmlCDataSection>(document.DocumentElement.ChildNodes[3]);
            Assert.AreEqual(source, document.ToXml());
        }

        [Test]
        public void ClonePreservesCDataSections()
        {
            var document = "<root><![CDATA[value]]></root>".ToXmlDocument();
            var clone = (IXmlDocument)document.Clone();

            Assert.IsInstanceOf<IXmlCDataSection>(clone.DocumentElement.FirstChild);
            Assert.AreEqual(document.ToXml(), clone.ToXml());
        }

        [Test]
        public void AutoSelectedFormatterPreservesCDataWithoutDoctype()
        {
            const String source = "<root><![CDATA[<value>]]></root>";
            var document = source.ToXmlDocument();

            Assert.AreEqual(source, document.ToMarkup());
            Assert.AreEqual("<![CDATA[<value>]]>", document.DocumentElement.FirstChild.ToMarkup());
        }

        [Test]
        public void CreateCDataSectionRejectsClosingDelimiter()
        {
            var document = "<root />".ToXmlDocument();

            Assert.Throws<DomException>(() => document.CreateCDataSection("]]>") );
        }

        [Test]
        public void CDataMutationsRejectClosingDelimiterAtomically()
        {
            var document = "<root />".ToXmlDocument();

            AssertMutationRejected(document.CreateCDataSection("safe"), section => section.Data = "]]>");
            AssertMutationRejected(document.CreateCDataSection("safe"), section => section.NodeValue = "]]>");
            AssertMutationRejected(document.CreateCDataSection("safe"), section => section.TextContent = "]]>");
            AssertMutationRejected(document.CreateCDataSection("]]"), section => section.Append(">"));
            AssertMutationRejected(document.CreateCDataSection("]]"), section => section.Insert(2, ">"));
            AssertMutationRejected(document.CreateCDataSection("]]x>"), section => section.Delete(2, 1));
            AssertMutationRejected(document.CreateCDataSection("safe"), section => section.Replace(0, 4, "]]>") );
        }

        [Test]
        public void CreateEntityReferenceIsExplicitlyUnsupported()
        {
            var document = "<root />".ToXmlDocument();

            Assert.Throws<NotSupportedException>(() => document.CreateEntityReference("entity"));
        }

        private static void AssertMutationRejected(IXmlCDataSection section, Action<IXmlCDataSection> mutation)
        {
            var original = section.Data;

            Assert.Throws<DomException>(() => mutation(section));
            Assert.AreEqual(original, section.Data);
        }
    }
}