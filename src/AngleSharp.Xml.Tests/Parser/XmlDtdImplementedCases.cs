namespace AngleSharp.Xml.Tests.Parser
{
    using NUnit.Framework;

    /// <summary>
    /// DTD-related cases that are known to work with the current implementation.
    /// The broader conformance fixtures remain ignored until full DTD support is available.
    /// </summary>
    [TestFixture]
    public class XmlDtdImplementedCases
    {
        [Test]
        public void XmlIbmValidP12Ibm12v03()
        {
            var document = @"<?xml version=""1.0""?>
<!DOCTYPE student PUBLIC ""The big ' in it"" ""student.dtd"">

<!-- testing Pubid Literal with a string with ""'"" inside -->
<student>My Name is SnowMan. </student>
".ToXmlDocument(validating: true);

            Assert.IsNotNull(document);
            Assert.IsTrue(document.IsValid);
        }

        [Test]
        public void XmlIbmValidP13Ibm13v01()
        {
            var document = @"<?xml version=""1.0""?>
<!DOCTYPE student PUBLIC ""#x20 #xD #xA abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ -'()+,./:=?;!*#@$_% "" ""student.dtd"">

<!-- testing Pubid char with all legal pubidchar in a string -->
<student>My Name is SnowMan. </student>








 ".ToXmlDocument(validating: true);

            Assert.IsNotNull(document);
            Assert.IsTrue(document.IsValid);
        }

        [Test]
        public void XmlIbmValidP30Ibm30v02()
        {
            var document = @"<!DOCTYPE animal SYSTEM ""ibm30v02.dtd""><animal/>
<!-- tests extSubset with TextDecl and extSubsetDecl in the dtd file -->
".ToXmlDocument(validating: true);

            Assert.IsNotNull(document);
            Assert.IsTrue(document.IsValid);
        }

        [Test]
        public void XmlIbmValidP12Ibm12v04()
        {
            var document = @"<?xml version=""1.0""?>
<!DOCTYPE student PUBLIC 'The latest version' 'student.dtd'[
]>

<!-- testing Pubid Literal with a string without  ""'"" inside -->
<student>My Name is SnowMan. </student>
".ToXmlDocument(validating: true);

            Assert.IsNotNull(document);
            Assert.IsTrue(document.IsValid);
        }

        [Test]
        public void XmlValidOP09pass1()
        {
            var document = @"<!DOCTYPE doc SYSTEM ""p09pass1.dtd"">
<doc/>".ToXmlDocument(validating: true);

            Assert.IsNotNull(document);
            Assert.IsTrue(document.IsValid);
        }

        [Test]
        public void XmlValidOP28pass4()
        {
            var document = @"<!DOCTYPE doc SYSTEM ""p28pass4.dtd"">
<doc/>".ToXmlDocument(validating: true);

            Assert.IsNotNull(document);
            Assert.IsTrue(document.IsValid);
        }

        [Test]
        public void XmlValidOP30pass1()
        {
            var document = @"<!DOCTYPE doc SYSTEM ""p30pass1.dtd"">
<doc/>".ToXmlDocument(validating: true);

            Assert.IsNotNull(document);
            Assert.IsTrue(document.IsValid);
        }

        [Test]
        public void XmlValidOP30pass2()
        {
            var document = @"<!DOCTYPE doc SYSTEM ""p30pass2.dtd"">
<doc/>".ToXmlDocument(validating: true);

            Assert.IsNotNull(document);
            Assert.IsTrue(document.IsValid);
        }

        [Test]
        public void XmlValidOP31pass2()
        {
            var document = @"<!DOCTYPE doc SYSTEM ""p31pass2.dtd"">
<doc/>".ToXmlDocument(validating: true);

            Assert.IsNotNull(document);
            Assert.IsTrue(document.IsValid);
        }

        [Test]
        public void XmlValidOP31pass1()
        {
            var document = @"<!DOCTYPE doc SYSTEM ""p31pass1.dtd"" [<!ELEMENT doc EMPTY>]>
<doc/>
".ToXmlDocument(validating: true);

            Assert.IsNotNull(document);
            Assert.IsTrue(document.IsValid);
        }

        [Test]
        public void XmlValidOP28pass5()
        {
            var document = @"<!DOCTYPE doc SYSTEM ""p28pass5.dtd""[
<!--comment-->
<!ENTITY % rootdecl ""<!ELEMENT doc (a)>"">
<!ELEMENT a EMPTY>
]>
<doc><a/></doc>
".ToXmlDocument(validating: true);

            Assert.IsNotNull(document);
            Assert.IsTrue(document.IsValid);
        }

        [Test]
        public void XmlInvalidEl01_InternalSubsetValidation()
        {
            var document = (@"<!DOCTYPE root [
<!ELEMENT root ANY>
]>
<root> <undeclared/> </root>

").ToXmlDocument(validating: true);

            Assert.IsNotNull(document);
            Assert.IsFalse(document.IsValid);
        }

        [Test]
        public void XmlInvalidEl02_InternalSubsetValidation()
        {
            var document = (@"<!DOCTYPE root [
<!ELEMENT root EMPTY>
]>
<root><root/></root>
").ToXmlDocument(validating: true);

            Assert.IsNotNull(document);
            Assert.IsFalse(document.IsValid);
        }

        [Test]
        public void XmlInvalidEl03_InternalSubsetValidation()
        {
            var document = (@"<!DOCTYPE root [
<!ELEMENT root (#PCDATA|root)*>
<!ELEMENT exception (#PCDATA)>
]>
<root>this is ok <exception>this isn't</exception> </root>
").ToXmlDocument(validating: true);

            Assert.IsNotNull(document);
            Assert.IsFalse(document.IsValid);
        }

        [Test]
        public void XmlInvalidEl06_InternalSubsetValidation()
        {
            var document = (@"<!DOCTYPE root [
<!ELEMENT root EMPTY>
    <!-- in case parsers special-case builtin entities incorrectly -->
]>
<root>&amp;</root>

").ToXmlDocument(validating: true);

            Assert.IsNotNull(document);
            Assert.IsFalse(document.IsValid);
        }

        [Test]
        public void XmlInvalidInvRequired01_InternalSubsetValidation()
        {
            var document = (@"<!DOCTYPE root [
    <!ELEMENT root EMPTY>
]>

<root xml:space='preserve'/>

    <!-- all attributes must be declared -->
").ToXmlDocument(validating: true);

            Assert.IsNotNull(document);
            Assert.IsFalse(document.IsValid);
        }

        [Test]
        public void XmlInvalidInvRequired02_InternalSubsetValidation()
        {
            var document = (@"<!DOCTYPE root [
    <!ELEMENT root EMPTY>
]>

<root xml:lang='en'/>

    <!-- all attributes must be declared -->

").ToXmlDocument(validating: true);

            Assert.IsNotNull(document);
            Assert.IsFalse(document.IsValid);
        }

        [Test]
        public void XmlValidSa084_InternalSubsetValidation()
        {
            var document = @"<!DOCTYPE doc [<!ELEMENT doc (#PCDATA)>]><doc></doc>
".ToXmlDocument(validating: true);

            Assert.IsNotNull(document);
            Assert.IsTrue(document.IsValid);
        }

        [Test]
        public void XmlValidSa093_InternalSubsetValidation()
        {
            var document = @"<!DOCTYPE doc [
<!ELEMENT doc (#PCDATA)>
]>
<doc>


</doc>
".ToXmlDocument(validating: true);

            Assert.IsNotNull(document);
            Assert.IsTrue(document.IsValid);
        }

        [Test]
        public void XmlValidSa116_InternalSubsetValidation()
        {
            var document = @"<!DOCTYPE doc [
<!ELEMENT doc (#PCDATA)>
]>
<doc><![CDATA[
]]></doc>
".ToXmlDocument(validating: true);

            Assert.IsNotNull(document);
            Assert.IsTrue(document.IsValid);
        }

                [Test]
                public void XmlIbmValidP39Ibm39v01_InternalSubsetValidation()
                {
                        var document = @"<?xml version=""1.0""?>
<!DOCTYPE root [
    <!ELEMENT root (a,b)>
    <!ELEMENT a EMPTY>
    <!ELEMENT b (#PCDATA|c)* >
    <!ELEMENT c ANY>
    <!ELEMENT d ((e,e)|f)+ >
    <!ELEMENT e ANY>
    <!ELEMENT f EMPTY>
]>
<root><a/><b>
     <c></c>
     content of b element
     <c>
            <d><e>no more children</e><e><f/></e><f/></d>
     </c>
</b></root>
".ToXmlDocument(validating: true);

                        Assert.IsNotNull(document);
                        Assert.IsTrue(document.IsValid);
                }

                [Test]
                public void XmlIbmInvalidP39Ibm39i01_InternalSubsetValidation()
                {
                        var document = @"<?xml version=""1.0""?>
<!DOCTYPE root [
    <!ELEMENT root (a,b)>
    <!ELEMENT a EMPTY>
    <!ELEMENT b (#PCDATA|c)* >
    <!ELEMENT c ANY>
]>
<root><a>should not have content here</a><b>
     <c></c>
     content of b element
</b></root>
".ToXmlDocument(validating: true);

                        Assert.IsNotNull(document);
                        Assert.IsFalse(document.IsValid);
                }

                [Test]
                public void XmlIbmInvalidP39Ibm39i03_InternalSubsetValidation()
                {
                        var document = @"<?xml version=""1.0""?>
<!DOCTYPE root [
    <!ELEMENT root (a,b)>
    <!ELEMENT a EMPTY>
    <!ELEMENT b (#PCDATA|c)* >
    <!ELEMENT c ANY>
]>
<root><a/><b>
     <c></c>
     content of b element
     <a/>
     could not have 'a' as 'b's content
</b></root>
".ToXmlDocument(validating: true);

                        Assert.IsNotNull(document);
                        Assert.IsFalse(document.IsValid);
                }

                [Test]
                public void XmlIbmInvalidP39Ibm39i04_InternalSubsetValidation()
                {
                        var document = @"<?xml version=""1.0""?>
<!DOCTYPE root [
    <!ELEMENT root (a,b)>
    <!ELEMENT a EMPTY>
    <!ELEMENT b (#PCDATA|c)* >
    <!ELEMENT c ANY>
    <!ELEMENT f EMPTY>
]>
<root><a/><b>
     <c><f/></c>
     content of b element
     <c>
            <d>not declared in dtd</d>
     </c>
</b></root>
".ToXmlDocument(validating: true);

                        Assert.IsNotNull(document);
                        Assert.IsFalse(document.IsValid);
                }

                [Test]
                public void XmlIbmInvalidP41Ibm41i01_InternalSubsetValidation()
                {
                        var document = @"<?xml version=""1.0""?>
<!DOCTYPE root [
    <!ELEMENT root (#PCDATA|b)* >
    <!ELEMENT b (#PCDATA) >
    <!ATTLIST b attr2 (abc|def) ""abc"">
    <!ATTLIST b attr3 CDATA #FIXED ""fixed"">
]>
<root>
    <b attr1=""value1"" attr2=""def"" attr3=""fixed"">attr1 not declared</b>
</root>
".ToXmlDocument(validating: true);

                        Assert.IsNotNull(document);
                        Assert.IsFalse(document.IsValid);
                }
    }
}
