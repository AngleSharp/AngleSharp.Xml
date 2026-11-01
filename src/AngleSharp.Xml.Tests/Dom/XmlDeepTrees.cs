namespace AngleSharp.Xml.Tests.Dom
{
    using AngleSharp.Dom;
    using NUnit.Framework;
    using System;
    using System.IO;
    using System.Linq;
    using System.Text;

    /// <summary>
    /// The XML convenience APIs on trees nested deeper than a recursive walk survives: each of
    /// these overflowed the call stack and ended the process (canonicalization at about 1750
    /// levels, the ID lookups at about 7500 on net8.0).
    /// </summary>
    [TestFixture]
    public class XmlDeepTrees
    {
        private const Int32 Depth = 20000;

        [TestCase(XmlCanonicalizationMode.CanonicalXml11)]
        [TestCase(XmlCanonicalizationMode.ExclusiveXml10)]
        public void CanonicalizesDeeplyNestedDocument(XmlCanonicalizationMode mode)
        {
            var document = "<a xmlns='urn:x' xmlns:p='urn:p'><a p:v='1' /></a>".ToXmlDocument();
            var root = document.DocumentElement;
            var template = root.FirstElementChild;
            root.RemoveChild(template);
            root.AppendChild(Chain(template, Depth - 1));
            // Inclusive canonicalization declares both namespaces on the root; exclusive declares
            // each where it is first used, so the prefix only on the first child
            var exclusive = mode == XmlCanonicalizationMode.ExclusiveXml10;
            var expected = new StringBuilder(exclusive ? "<a xmlns=\"urn:x\">" : "<a xmlns=\"urn:x\" xmlns:p=\"urn:p\">");
            expected.Append(exclusive ? "<a xmlns:p=\"urn:p\" p:v=\"1\">" : "<a p:v=\"1\">");

            for (var i = 2; i < Depth; i++)
            {
                expected.Append("<a p:v=\"1\">");
            }

            expected.Append(String.Concat(Enumerable.Repeat("</a>", Depth)));

            using (var stream = new MemoryStream())
            {
                document.ToCanonicalXml(stream, new XmlCanonicalizationOptions { Mode = mode });
                Assert.AreEqual(expected.ToString(), Encoding.UTF8.GetString(stream.ToArray()));
            }
        }

        [Test]
        public void FindsDeepestElementByXmlId()
        {
            var document = "<a><a /></a>".ToXmlDocument();
            var root = document.DocumentElement;
            var template = root.FirstElementChild;
            root.RemoveChild(template);
            var chain = Chain(template, Depth - 1);
            var deepest = Deepest(chain);
            deepest.SetAttribute("http://www.w3.org/XML/1998/namespace", "xml:id", "deepest");
            root.AppendChild(chain);

            Assert.AreSame(deepest, document.GetElementByXmlId("deepest"));
            Assert.IsNull(document.GetElementByXmlId("missing"));
        }

        [Test]
        public void FindsDeepestElementByDtdId()
        {
            // The template carries the ID attribute, so its clones keep knowing which attribute it is
            var document = "<!DOCTYPE a [<!ELEMENT a ANY><!ATTLIST a id ID #IMPLIED>]><a><a id='template' /></a>".ToXmlDocument();
            var root = document.DocumentElement;
            var template = root.FirstElementChild;
            root.RemoveChild(template);
            var chain = Chain(template, Depth - 1);
            var deepest = Deepest(chain);
            deepest.SetAttribute("id", "deepest");
            root.AppendChild(chain);

            Assert.AreSame(deepest, document.GetElementByDtdId("deepest"));
            Assert.IsNull(document.GetElementByDtdId("missing"));
        }

        [Test]
        public void ResolvesXmlBaseOfDeeplyNestedElement()
        {
            const Int32 depth = 5000;
            var document = "<a xml:base='http://example.com/'><a xml:base='d/' /></a>".ToXmlDocument();
            var root = document.DocumentElement;
            var template = root.FirstElementChild;
            root.RemoveChild(template);
            var chain = Chain(template, depth);
            root.AppendChild(chain);
            var expected = "http://example.com/" + String.Concat(Enumerable.Repeat("d/", depth));

            Assert.AreEqual(expected, Deepest(chain).GetXmlBaseUri());
        }

        /// <summary>
        /// Builds a chain of clones of the template from the bottom up, so each append happens on
        /// a detached element and does not walk the ancestors of a deep tree.
        /// </summary>
        private static IElement Chain(IElement template, Int32 depth)
        {
            var current = (IElement)template.Clone(false);

            for (var i = 1; i < depth; i++)
            {
                var parent = (IElement)template.Clone(false);
                parent.AppendChild(current);
                current = parent;
            }

            return current;
        }

        private static IElement Deepest(IElement element)
        {
            while (element.FirstElementChild != null)
            {
                element = element.FirstElementChild;
            }

            return element;
        }
    }
}
