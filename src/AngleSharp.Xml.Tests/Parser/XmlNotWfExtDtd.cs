namespace AngleSharp.Xml.Tests.Parser
{
    using NUnit.Framework;
    using System;

    [TestFixture]
    public class XmlNotWfExtDtd
    {
        /// <summary>
        /// Text declarations (which optionally begin any external entity) are
        /// required to have "encoding=...". Here the section(s) 4.3.1 [77] apply.
        /// This test is taken from the collection Sun Microsystems XML Tests.
        /// </summary>
        [Test]
        public void XmlNotWfDtd07()
        {
            Assert.Throws<Exception>(() =>
            {
                var document = @"<!DOCTYPE root SYSTEM ""dtd07.dtd"" [
    <!ELEMENT root EMPTY>
]>
<root/>
".ToXmlDocument();
            });
        }

        /// <summary>
        /// Text declarations (which optionally begin any external entity) are required
        /// to have "encoding=...". Here the section(s) 4.3.1 [77] apply. This test is taken
        /// from the collection Sun Microsystems XML Tests.
        /// </summary>
        [Test]
        public void XmlNotWfEncoding07()
        {
            Assert.Throws<Exception>(() =>
            {
                var document = @"<!DOCTYPE root [
    <!ELEMENT root EMPTY>

    <!--
	reusing this entity; it's got no markup decls,
	so it's legal except for a missing ""encoding=..."".
    -->
    <!ENTITY empty SYSTEM ""dtd07.dtd"">
]>
<root>&empty;</root>
".ToXmlDocument();
            });
        }

        /// <summary>
        /// Only INCLUDE and IGNORE are conditional section keywords. Here the section(s) 3.4 [61] apply.
        /// This test is taken from the collection Sun Microsystems XML Tests.
        /// </summary>
        [Test]
        public void XmlNotWfCond01()
        {
            Assert.Throws<Exception>(() =>
            {
                var document = @"<!DOCTYPE root SYSTEM ""cond.dtd"" [
    <!ENTITY % MAYBE ""CDATA"">
]>

<root/>
".ToXmlDocument();
            });
        }

        /// <summary>
        /// no other types, including TEMP, which is valid in SGML Here the section(s) 3.4 [61] apply.
        /// This test is taken from the collection OASIS/NIST TESTS, 1-Nov-1998.
        /// </summary>
        [Test]
        public void XmlNotWfOP61fail1()
        {
            Assert.Throws<Exception>(() =>
            {
                var document = @"<!DOCTYPE doc SYSTEM ""p61fail1.dtd"">
<doc/>".ToXmlDocument();
            });
        }

        /// <summary>
        /// INCLUDE must be upper case Here the section(s) 3.4 [62] apply. This
        /// test is taken from the collection OASIS/NIST TESTS, 1-Nov-1998.
        /// </summary>
        [Test]
        public void XmlNotWfOP62fail1()
        {
            Assert.Throws<Exception>(() =>
            {
                var document = @"<!DOCTYPE doc SYSTEM ""p62fail1.dtd"">
<doc/>".ToXmlDocument();
            });
        }

        /// <summary>
        /// no spaces in terminating delimiter Here the section(s) 3.4 [62] apply. This test
        /// is taken from the collection OASIS/NIST TESTS, 1-Nov-1998.
        /// </summary>
        [Test]
        public void XmlNotWfOP62fail2()
        {
            Assert.Throws<Exception>(() =>
            {
                var document = @"<!DOCTYPE doc SYSTEM ""p62fail2.dtd"">
<doc/>".ToXmlDocument();
            });
        }

        /// <summary>
        /// IGNORE must be upper case Here the section(s) 3.4 [63] apply. This test is taken
        /// from the collection OASIS/NIST TESTS, 1-Nov-1998.
        /// </summary>
        [Test]
        public void XmlNotWfOP63fail1()
        {
            Assert.Throws<Exception>(() =>
            {
                var document = @"<!DOCTYPE doc SYSTEM ""p63fail1.dtd"">
<doc/>".ToXmlDocument();
            });
        }

        /// <summary>
        /// delimiters must be balanced Here the section(s) 3.4 [63] apply. This test is taken
        /// from the collection OASIS/NIST TESTS, 1-Nov-1998.
        /// </summary>
        [Test]
        public void XmlNotWfOP63fail2()
        {
            Assert.Throws<Exception>(() =>
            {
                var document = @"<!DOCTYPE doc SYSTEM ""p63fail2.dtd"">
<doc/>".ToXmlDocument();
            });
        }

        /// <summary>
        /// section delimiters must balance Here the section(s) 3.4 [64] apply.
        /// This test is taken from the collection OASIS/NIST TESTS, 1-Nov-1998.
        /// </summary>
        [Test]
        public void XmlNotWfOP64fail1()
        {
            Assert.Throws<Exception>(() =>
            {
                var document = @"<!DOCTYPE doc SYSTEM ""p64fail1.dtd"">
<doc/>
".ToXmlDocument();
            });
        }

        /// <summary>
        /// section delimiters must balance Here the section(s) 3.4 [64] apply.
        /// This test is taken from the collection OASIS/NIST TESTS, 1-Nov-1998.
        /// </summary>
        [Test]
        public void XmlNotWfOP64fail2()
        {
            Assert.Throws<Exception>(() =>
            {
                var document = @"<!DOCTYPE doc SYSTEM ""p64fail2.dtd"">
<doc/>
".ToXmlDocument();
            });
        }
    }
}
