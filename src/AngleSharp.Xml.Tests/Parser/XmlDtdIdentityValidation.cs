namespace AngleSharp.Xml.Tests.Parser
{
    using AngleSharp.Xml.Dom;
    using NUnit.Framework;
    using System;
    using System.IO;

    [TestFixture]
    public class XmlDtdIdentityValidation
    {
        [Test]
        public void UniqueIdsAndResolvedReferencesAreValid()
        {
            var document = @"<!DOCTYPE root [
<!ELEMENT root (item,item,refs)>
<!ELEMENT item EMPTY>
<!ELEMENT refs EMPTY>
<!ATTLIST item key ID #REQUIRED>
<!ATTLIST refs one IDREF #REQUIRED many IDREFS #REQUIRED>
]><root><item key='a'/><item key='b'/><refs one=' a ' many=' a   b '/></root>".ToXmlDocument(validating: true);

            Assert.IsTrue(document.IsValid);
            Assert.AreEqual("a", document.DocumentElement.FirstElementChild.GetDtdId());
            Assert.AreSame(document.DocumentElement.FirstElementChild, document.GetElementByDtdId("a"));
            Assert.AreEqual("a", document.QuerySelector("refs").GetAttribute("one"));
            Assert.AreEqual("a b", document.QuerySelector("refs").GetAttribute("many"));
        }

        [Test]
        public void DtdIdLookupReflectsDomMutations()
        {
            var document = CreateIdDocument("<item key='before'/><item key='other'/>");
            var item = document.DocumentElement.FirstElementChild;

            item.Attributes["key"].Value = "after";

            Assert.IsNull(document.GetElementByDtdId("before"));
            Assert.AreSame(item, document.GetElementByDtdId("after"));
        }

        [Test]
        public void DuplicateIdIsInvalid()
        {
            var document = CreateIdDocument("<item key='same'/><item key='same'/>");

            Assert.IsFalse(document.IsValid);
        }

        [Test]
        public void UnresolvedIdRefAndIdRefsAreInvalid()
        {
            var single = CreateReferenceDocument("IDREF", "missing");
            var multiple = CreateReferenceDocument("IDREFS", "known missing");

            Assert.IsFalse(single.IsValid);
            Assert.IsFalse(multiple.IsValid);
        }

        [Test]
        public void MultipleIdDeclarationsAndIdDefaultAreInvalid()
        {
            var multiple = @"<!DOCTYPE root [<!ELEMENT root EMPTY><!ATTLIST root first ID #IMPLIED second ID #IMPLIED>]><root/>".ToXmlDocument(validating: true);
            var defaulted = @"<!DOCTYPE root [<!ELEMENT root EMPTY><!ATTLIST root key ID 'value'>]><root/>".ToXmlDocument(validating: true);

            Assert.IsFalse(multiple.IsValid);
            Assert.IsFalse(defaulted.IsValid);
        }

        [Test]
        public void EntityAttributesRequireDeclaredUnparsedEntities()
        {
            const string declarations = "<!NOTATION png SYSTEM 'image/png'><!ENTITY logo SYSTEM 'logo.png' NDATA png>";
            var valid = CreateEntityDocument(declarations, "ENTITY", "logo");
            var invalidParsed = CreateEntityDocument("<!ENTITY logo 'text'>", "ENTITY", "logo");
            var invalidMissing = CreateEntityDocument(declarations, "ENTITIES", "logo missing");

            Assert.IsTrue(valid.IsValid);
            Assert.IsFalse(invalidParsed.IsValid);
            Assert.IsFalse(invalidMissing.IsValid);
        }

        [Test]
        public void UnparsedEntityAndNotationDeclarationsMustBeConsistent()
        {
            var missingNotation = CreateEntityDocument("<!ENTITY logo SYSTEM 'logo.png' NDATA missing>", "ENTITY", "logo");
            var invalidNotationAttribute = @"<!DOCTYPE root [<!ELEMENT root EMPTY><!NOTATION png SYSTEM 'image/png'><!ATTLIST root format NOTATION (png|missing) #REQUIRED>]><root format='png'/>".ToXmlDocument(validating: true);

            Assert.IsFalse(missingNotation.IsValid);
            Assert.IsFalse(invalidNotationAttribute.IsValid);
        }

        [Test]
        public void ExternalDtdIdentityDeclarationsAreValidated()
        {
            var directory = Path.Combine(Path.GetTempPath(), "anglesharp-xml-dtd-id-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(directory);

            try
            {
                var dtdPath = Path.Combine(directory, "identity.dtd");
                File.WriteAllText(dtdPath, "<!ELEMENT root (item,ref)><!ELEMENT item EMPTY><!ELEMENT ref EMPTY><!ATTLIST item key ID #REQUIRED><!ATTLIST ref target IDREF #REQUIRED>");
                var valid = $"<!DOCTYPE root SYSTEM '{dtdPath}'><root><item key='known'/><ref target='known'/></root>".ToXmlDocument(validating: true);
                var invalid = $"<!DOCTYPE root SYSTEM '{dtdPath}'><root><item key='known'/><ref target='missing'/></root>".ToXmlDocument(validating: true);

                Assert.IsTrue(valid.IsValid);
                Assert.IsFalse(invalid.IsValid);
            }
            finally
            {
                Directory.Delete(directory, true);
            }
        }

        private static IXmlDocument CreateIdDocument(string children) => $@"<!DOCTYPE root [
<!ELEMENT root (item,item)><!ELEMENT item EMPTY><!ATTLIST item key ID #REQUIRED>
]><root>{children}</root>".ToXmlDocument(validating: true);

        private static IXmlDocument CreateReferenceDocument(string type, string value) => $@"<!DOCTYPE root [
<!ELEMENT root (item,refs)><!ELEMENT item EMPTY><!ELEMENT refs EMPTY>
<!ATTLIST item key ID #REQUIRED><!ATTLIST refs target {type} #REQUIRED>
]><root><item key='known'/><refs target='{value}'/></root>".ToXmlDocument(validating: true);

        private static IXmlDocument CreateEntityDocument(string declarations, string type, string value) => $@"<!DOCTYPE root [
<!ELEMENT root EMPTY>{declarations}<!ATTLIST root entity {type} #REQUIRED>
]><root entity='{value}'/>".ToXmlDocument(validating: true);
    }
}