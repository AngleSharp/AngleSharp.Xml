namespace AngleSharp.Xml.Tests.Parser
{
    using NUnit.Framework;
    using System;
    using System.Linq;

    /// <summary>
    /// A CDATA section in a declared entity's replacement text contributes its content to element
    /// content, while text that only looks like a CDATA section because it was escaped stays as
    /// written.
    /// </summary>
    [TestFixture]
    public class XmlEntityCDataSections
    {
        [TestCase("a<![CDATA[<b>]]>c", "a<b>c")]
        [TestCase("<![CDATA[x]]><![CDATA[y]]>", "xy")]
        [TestCase("<![CDATA[]]>", "")]
        [TestCase("<![CDATA[<![CDATA[z]]>", "<![CDATA[z")]
        [TestCase("<![CDATA[unterminated", "<![CDATA[unterminated")]
        public void CDataSectionsInEntityContributeTheirContent(String value, String expected)
        {
            var document = $"<!DOCTYPE r [<!ENTITY e \"{value}\">]><r>&e;</r>".ToXmlDocument();

            Assert.AreEqual(expected, document.DocumentElement.TextContent);
        }

        [Test]
        public void CDataSectionInNestedEntityContributesItsContent()
        {
            var document = "<!DOCTYPE r [<!ENTITY outer \"[&inner;]\"><!ENTITY inner \"<![CDATA[<i>]]>\">]><r>&outer;</r>".ToXmlDocument();

            Assert.AreEqual("[<i>]", document.DocumentElement.TextContent);
        }

        [Test]
        public void EscapedCDataLookingTextStaysAsWrittenWhenEntitiesAreDeclared()
        {
            var document = "<!DOCTYPE r [<!ENTITY e \"x\">]><r>a &lt;![CDATA[b]]&gt; c &e;</r>".ToXmlDocument();

            Assert.AreEqual("a <![CDATA[b]]> c x", document.DocumentElement.TextContent);
        }

        [Test]
        public void ManyEscapedCDataStartsStayAsWritten()
        {
            const Int32 count = 20000;
            var text = String.Concat(Enumerable.Repeat("&lt;![CDATA[", count));
            var document = $"<!DOCTYPE r [<!ENTITY e \"x\">]><r>{text}</r>".ToXmlDocument();

            Assert.AreEqual(String.Concat(Enumerable.Repeat("<![CDATA[", count)), document.DocumentElement.TextContent);
        }

        [Test]
        public void EntityInAttributeValueKeepsItsReplacementText()
        {
            var document = "<!DOCTYPE r [<!ENTITY e \"a<![CDATA[b]]>c\">]><r v=\"&e;\" />".ToXmlDocument();

            Assert.AreEqual("a<![CDATA[b]]>c", document.DocumentElement.GetAttribute("v"));
        }
    }
}
