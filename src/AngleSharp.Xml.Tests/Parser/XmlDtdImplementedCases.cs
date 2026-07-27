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
    }
}
