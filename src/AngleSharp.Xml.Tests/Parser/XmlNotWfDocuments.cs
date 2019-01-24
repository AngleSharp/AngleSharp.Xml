namespace AngleSharp.Xml.Tests.Parser
{
    using NUnit.Framework;
    using System;

    /// <summary>
    /// (Conformance) Tests taken from
    /// http://www.w3.org/XML/Test/xmlconf-20031210.html
    [TestFixture]
    public class XmlNotWfDocuments
    {
        /// <summary>
        /// Illegal character " " in encoding name Here the section(s) 4.3.3 [81] apply.
        /// This test is taken from the collection Sun Microsystems XML Tests.
        /// </summary>
        [Test]
        public void XmlNotWfEncoding01()
        {
            Assert.Throws<Exception>(() => { var document = @"<?xml version=""1.0"" encoding="" utf-8""?>
<root/>
".ToXmlDocument(); });
        }

        /// <summary>
        /// Illegal character "/" in encoding name Here the section(s) 4.3.3 [81] apply.
        /// This test is taken from the collection Sun Microsystems XML Tests.
        /// </summary>
        [Test]
        public void XmlNotWfEncoding02()
        {
            Assert.Throws<Exception>(() => { var document = @"<?xml version=""1.0"" encoding=""a/b""?>
<root/>

".ToXmlDocument(); });
        }

        /// <summary>
        /// Illegal character reference in encoding name Here the section(s) 4.3.3 [81] apply.
        /// This test is taken from the collection Sun Microsystems XML Tests.
        /// </summary>
        [Test]
        public void XmlNotWfEncoding03()
        {
            Assert.Throws<Exception>(() => { var document = @"<?xml version=""1.0"" encoding=""just&#41;word""?>
<root/>

".ToXmlDocument(); });
        }

        /// <summary>
        /// Illegal character ":" in encoding name Here the section(s) 4.3.3 [81] apply.
        /// This test is taken from the collection Sun Microsystems XML Tests.
        /// </summary>
        [Test]
        public void XmlNotWfEncoding04()
        {
            Assert.Throws<Exception>(() => { var document = @"<?xml version=""1.0"" encoding=""utf:8""?>
<root/>

".ToXmlDocument(); });
        }

        /// <summary>
        /// Illegal character "@" in encoding name Here the section(s) 4.3.3 [81] apply. 
        /// This test is taken from the collection Sun Microsystems XML Tests.
        /// </summary>
        [Test]
        public void XmlNotWfEncoding05()
        {
            Assert.Throws<Exception>(() => { var document = @"<?xml version=""1.0"" encoding=""@import(sys-encoding)""?>
<root/>

".ToXmlDocument(); });
        }

        /// <summary>
        /// Illegal character "+" in encoding name Here the section(s) 4.3.3 [81] apply.
        /// This test is taken from the collection Sun Microsystems XML Tests.
        /// </summary>
        [Test]
        public void XmlNotWfEncoding06()
        {
            Assert.Throws<Exception>(() => { var document = @"<?xml version=""1.0"" encoding=""XYZ+999""?>

<!-- WF ... but illegal encoding name, also a fatal error --> 

<root/>
".ToXmlDocument(); });
        }

        /// <summary>
        /// Value is required Here the section(s) 4.2 [73] apply. This test is
        /// taken from the collection OASIS/NIST TESTS, 1-Nov-1998.
        /// </summary>
        [Test]
        public void XmlNotWfOP73fail4()
        {
            Assert.Throws<Exception>(() => { var document = @"<!DOCTYPE doc
[
<!ELEMENT doc (#PCDATA)>
<!NOTATION unknot PUBLIC ""Unknown"">
<!ENTITY ge >
]>
<doc/>
".ToXmlDocument(); });
        }

        /// <summary>
        /// No NDataDecl without value Here the section(s) 4.2 [73] apply.
        /// This test is taken from the collection OASIS/NIST TESTS, 1-Nov-1998.
        /// </summary>
        [Test]
        public void XmlNotWfOP73fail5()
        {
            Assert.Throws<Exception>(() => { var document = @"<!DOCTYPE doc
[
<!ELEMENT doc (#PCDATA)>
<!NOTATION unknot PUBLIC ""Unknown"">
<!ENTITY ge NDATA unknot>
]>
<doc/>
".ToXmlDocument(); });
        }

        /// <summary>
        /// No NData Decls on parameter entities. Here the section(s) 4.2 [74] apply.
        /// This test is taken from the collection OASIS/NIST TESTS, 1-Nov-1998.
        /// </summary>
        [Test]
        public void XmlNotWfOP74fail1()
        {
            Assert.Throws<Exception>(() => { var document = @"<!DOCTYPE doc
[
<!NOTATION unknot PUBLIC ""Unknown"">
<!ENTITY % pe SYSTEM ""nop.ent"" NDATA unknot>
]>
<doc/>
".ToXmlDocument(); });
        }

        /// <summary>
        /// value is required Here the section(s) 4.2 [74] apply.
        /// This test is taken from the collection OASIS/NIST TESTS, 1-Nov-1998.
        /// </summary>
        [Test]

        public void XmlNotWfOP74fail2()
        {
            Assert.Throws<Exception>(() => { var document = @"<!DOCTYPE doc
[
<!NOTATION unknot PUBLIC ""Unknown"">
<!ENTITY % pe>
]>
<doc/>
".ToXmlDocument(); });
        }

        /// <summary>
        /// only one value Here the section(s) 4.2 [74] apply.
        /// This test is taken from the collection OASIS/NIST TESTS, 1-Nov-1998.
        /// </summary>
        [Test]
        public void XmlNotWfOP74fail3()
        {
            Assert.Throws<Exception>(() => { var document = @"<!DOCTYPE doc
[
<!ENTITY % pe ""<!--decl1-->"" SYSTEM ""nop.ent"">
]>
<doc/>
".ToXmlDocument(); });
        }

        /// <summary>
        /// S is required after name Here the section(s) 4.2 [72] apply.
        /// This test is taken from the collection OASIS/NIST TESTS, 1-Nov-1998.
        /// </summary>
        [Test]
        public void XmlNotWfOP72fail3()
        {
            Assert.Throws<Exception>(() => { var document = @"<!DOCTYPE doc
[
<!ELEMENT doc (#PCDATA)>
<!ENTITY % pe""<!--replacement decl-->"">
]>
<doc/>
".ToXmlDocument(); });
        }

        /// <summary>
        /// Entity name is a name, not an NMToken Here the section(s) 4.2 [72] apply.
        /// This test is taken from the collection OASIS/NIST TESTS, 1-Nov-1998.
        /// </summary>
        [Test]
        public void XmlNotWfOP72fail4()
        {
            Assert.Throws<Exception>(() => { var document = @"<!DOCTYPE doc
[
<!ELEMENT doc (#PCDATA)>
<!ENTITY % .pe ""<!--replacement decl-->"">
]>
<doc/>
".ToXmlDocument(); });
        }

        /// <summary>
        /// No typed replacement text. Here the section(s) 4.2 [73] apply.
        /// This test is taken from the collection OASIS/NIST TESTS, 1-Nov-1998.
        /// </summary>
        [Test]
        public void XmlNotWfOP73fail1()
        {
            Assert.Throws<Exception>(() => { var document = @"<!DOCTYPE doc
[
<!ELEMENT doc (#PCDATA)>
<!NOTATION unknot PUBLIC ""Unknown"">
<!ENTITY ge CDATA ""replacement text"">
]>
<doc/>
".ToXmlDocument(); });
        }

        /// <summary>
        /// Only one replacement value. Here the section(s) 4.2 [73] apply.
        /// This test is taken from the collection OASIS/NIST TESTS, 1-Nov-1998.
        /// </summary>
        [Test]
        public void XmlNotWfOP73fail2()
        {
            Assert.Throws<Exception>(() => { var document = @"<!DOCTYPE doc
[
<!ELEMENT doc (#PCDATA)>
<!NOTATION unknot PUBLIC ""Unknown"">
<!ENTITY ge ""replacement text"" ""more text"">
]>
<doc/>
".ToXmlDocument(); });
        }

        /// <summary>
        /// No NDataDecl on replacement text. Here the section(s) 4.2 [73] apply.
        /// This test is taken from the collection OASIS/NIST TESTS, 1-Nov-1998.
        /// </summary>
        [Test]
        public void XmlNotWfOP73fail3()
        {
            Assert.Throws<Exception>(() => { var document = @"<!DOCTYPE doc
[
<!ELEMENT doc (#PCDATA)>
<!NOTATION unknot PUBLIC ""Unknown"">
<!ENTITY ge ""replacement text"" NDATA unknot>
]>
<doc/>
".ToXmlDocument(); });
        }

        /// <summary>
        /// S is required after '%'. Here the section(s) 4.2 [72] apply.
        /// This test is taken from the collection OASIS/NIST TESTS, 1-Nov-1998.
        /// </summary>
        [Test]
        public void XmlNotWfOP72fail2()
        {
            Assert.Throws<Exception>(() => { var document = @"<!DOCTYPE doc
[
<!ELEMENT doc (#PCDATA)>
<!ENTITY %pe ""<!--replacement decl-->"">
]>
<doc/>
".ToXmlDocument(); });
        }

        /// <summary>
        /// Notation name is required. Here the section(s) 4.2.2 [76] apply.
        /// This test is taken from the collection OASIS/NIST TESTS, 1-Nov-1998.
        /// </summary>
        [Test]

        public void XmlNotWfOP76fail3()
        {
            Assert.Throws<Exception>(() => { var document = @"<!DOCTYPE doc
[
<!ELEMENT doc (#PCDATA)>
<!NOTATION unknot PUBLIC ""Unknown"">
<!ENTITY ge SYSTEM ""nop.ent"" NDATA>
]>
<doc/>
".ToXmlDocument(); });
        }

        /// <summary>
        /// notation names are Names Here the section(s) 4.2.2 [76] apply.
        /// This test is taken from the collection OASIS/NIST TESTS, 1-Nov-1998.
        /// </summary>
        [Test]

        public void XmlNotWfOP76fail4()
        {
            Assert.Throws<Exception>(() => { var document = @"<!DOCTYPE doc
[
<!ELEMENT doc (#PCDATA)>
<!NOTATION unknot PUBLIC ""Unknown"">
<!--error should be reported here, not at <!Notation-->
<!ENTITY ge SYSTEM ""nop.ent"" NDATA -unknot>
<!NOTATION -unknot PUBLIC ""Unknown"">
]>
<doc/>
".ToXmlDocument(); });
        }

        /// <summary>
        /// This is neither Here the section(s) 4.2 [70] apply.
        /// This test is taken from the collection OASIS/NIST TESTS, 1-Nov-1998.
        /// </summary>
        [Test]

        public void XmlNotWfOP70fail1()
        {
            Assert.Throws<Exception>(() => { var document = @"<!DOCTYPE doc
[
<!ELEMENT doc (#PCDATA)>
<!ENTITY & bad ""replacement text"">
]>
<doc/>
".ToXmlDocument(); });
        }

        /// <summary>
        /// S is required before EntityDef Here the section(s) 4.2 [71] apply.
        /// This test is taken from the collection OASIS/NIST TESTS, 1-Nov-1998.
        /// </summary>
        [Test]

        public void XmlNotWfOP71fail1()
        {
            Assert.Throws<Exception>(() => { var document = @"<!DOCTYPE doc
[
<!ELEMENT doc (#PCDATA)>
<!ENTITY ge""replacement text"">
]>
<doc/>
".ToXmlDocument(); });
        }

        /// <summary>
        /// Entity name is a Name, not an NMToken Here the section(s) 4.2 [71] apply.
        /// This test is taken from the collection OASIS/NIST TESTS, 1-Nov-1998.
        /// </summary>
        [Test]

        public void XmlNotWfOP71fail2()
        {
            Assert.Throws<Exception>(() => { var document = @"<!DOCTYPE doc
[
<!ELEMENT doc (#PCDATA)>
<!ENTITY -ge ""replacement text"">
]>
<doc/>
".ToXmlDocument(); });
        }

        /// <summary>
        /// "SYSTEM" implies only one literal Here the section(s) 4.2.2 [75] apply.
        /// This test is taken from the collection OASIS/NIST TESTS, 1-Nov-1998.
        /// </summary>
        [Test]

        public void XmlNotWfOP75fail4()
        {
            Assert.Throws<Exception>(() => { var document = @"<!DOCTYPE doc
[
<!ENTITY ent SYSTEM ""PublicID"" ""nop.ent"">
]>
<doc/>
".ToXmlDocument(); });
        }

        /// <summary>
        /// only one keyword Here the section(s) 4.2.2 [75] apply.
        /// This test is taken from the collection OASIS/NIST TESTS, 1-Nov-1998.
        /// </summary>
        [Test]

        public void XmlNotWfOP75fail5()
        {
            Assert.Throws<Exception>(() => { var document = @"<!DOCTYPE doc
[
<!ENTITY ent PUBLIC ""PublicID"" SYSTEM ""nop.ent"">
]>
<doc/>
".ToXmlDocument(); });
        }

        /// <summary>
        /// "PUBLIC" requires two literals (contrast with SGML) Here the section(s)
        /// 4.2.2 [75] apply. This test is taken from the collection OASIS/NIST TESTS, 1-Nov-1998.
        /// </summary>
        [Test]

        public void XmlNotWfOP75fail6()
        {
            Assert.Throws<Exception>(() => { var document = @"<!DOCTYPE doc
[
<!ENTITY ent PUBLIC ""PublicID"">
]>
<doc/>
".ToXmlDocument(); });
        }

        /// <summary>
        /// S is required before "NDATA" Here the section(s) 4.2.2 [76] apply.
        /// This test is taken from the collection OASIS/NIST TESTS, 1-Nov-1998.
        /// </summary>
        [Test]

        public void XmlNotWfOP76fail1()
        {
            Assert.Throws<Exception>(() => { var document = @"<!DOCTYPE doc
[
<!ELEMENT doc (#PCDATA)>
<!NOTATION unknot PUBLIC ""Unknown"">
<!ENTITY ge SYSTEM ""nop.ent""NDATA unknot>
]>
<doc/>
".ToXmlDocument(); });
        }

        /// <summary>
        /// "NDATA" is upper-case Here the section(s) 4.2.2 [76] apply.
        /// This test is taken from the collection OASIS/NIST TESTS, 1-Nov-1998.
        /// </summary>
        [Test]

        public void XmlNotWfOP76fail2()
        {
            Assert.Throws<Exception>(() => { var document = @"<!DOCTYPE doc
[
<!ELEMENT doc (#PCDATA)>
<!NOTATION unknot PUBLIC ""Unknown"">
<!ENTITY ge SYSTEM ""nop.ent"" ndata unknot>
]>
<doc/>
".ToXmlDocument(); });
        }

        /// <summary>
        /// S required after "PUBLIC". Here the section(s) 4.2.2 [75] apply.
        /// This test is taken from the collection OASIS/NIST TESTS, 1-Nov-1998.
        /// </summary>
        [Test]

        public void XmlNotWfOP75fail1()
        {
            Assert.Throws<Exception>(() => { var document = @"<!DOCTYPE doc
[
<!ENTITY ent PUBLIC""PublicID"" ""nop.ent"">
]>
<doc/>
".ToXmlDocument(); });
        }

        /// <summary>
        /// S required after "SYSTEM" Here the section(s) 4.2.2 [75] apply
        /// This test is taken from the collection OASIS/NIST TESTS, 1-Nov-1998.
        /// </summary>
        [Test]

        public void XmlNotWfOP75fail2()
        {
            Assert.Throws<Exception>(() => { var document = @"<!DOCTYPE doc
[
<!ENTITY ent SYSTEM""nop.ent"">
]>
<doc/>
".ToXmlDocument(); });
        }

        /// <summary>
        /// S required between literals Here the section(s) 4.2.2 [75] apply.
        /// This test is taken from the collection OASIS/NIST TESTS, 1-Nov-1998.
        /// </summary>
        [Test]

        public void XmlNotWfOP75fail3()
        {
            Assert.Throws<Exception>(() => { var document = @"<!DOCTYPE doc
[
<!ENTITY ent PUBLIC ""PublicID""""nop.ent"">
]>
<doc/>
".ToXmlDocument(); });
        }

        /// <summary>
        /// terminating ';' is required Here the section(s) 4.1 [69] apply.
        /// This test is taken from the collection OASIS/NIST TESTS, 1-Nov-1998.
        /// </summary>
        [Test]

        public void XmlNotWfOP69fail1()
        {
            Assert.Throws<Exception>(() => { var document = @"<!DOCTYPE doc
[
<!ELEMENT doc (#PCDATA)>
<!ENTITY % pe ""<!---->"">
%pe<!---->
]>
<doc/>
".ToXmlDocument(); });
        }

        /// <summary>
        /// no S after '%' Here the section(s) 4.1 [69] apply.
        /// This test is taken from the collection OASIS/NIST TESTS, 1-Nov-1998.
        /// </summary>
        [Test]

        public void XmlNotWfOP69fail2()
        {
            Assert.Throws<Exception>(() => { var document = @"<!DOCTYPE doc
[
<!ELEMENT doc (#PCDATA)>
<!ENTITY % pe ""<!---->"">
% pe;
]>
<doc/>
".ToXmlDocument(); });
        }

        /// <summary>
        /// no S before ';' Here the section(s) 4.1 [69] apply.
        /// This test is taken from the collection OASIS/NIST TESTS, 1-Nov-1998.
        /// </summary>
        [Test]

        public void XmlNotWfOP69fail3()
        {
            Assert.Throws<Exception>(() => { var document = @"<!DOCTYPE doc
[
<!ELEMENT doc (#PCDATA)>
<!ENTITY % pe ""<!---->"">
%pe ;
]>
<doc/>
".ToXmlDocument(); });
        }

        /// <summary>
        /// PUBLIC literal must be quoted Here the section(s) 4.2.2 [75] apply.
        /// This test is taken from the collection Sun Microsystems XML Tests.
        /// </summary>
        [Test]

        public void XmlNotWfDtd04()
        {
            Assert.Throws<Exception>(() => { var document = @"<!DOCTYPE root [
    <!ELEMENT root EMPTY>
    <!-- PUBLIC id must be quoted -->
    <!ENTITY foo PUBLIC -//BadCorp//DTD-foo-1.0//EN ""elvis.ent"">
]>

<root/>
".ToXmlDocument(); });
        }

        /// <summary>
        /// SYSTEM identifier must be quoted Here the section(s) 4.2.2 [75] apply.
        /// This test is taken from the collection Sun Microsystems XML Tests.
        /// </summary>
        [Test]

        public void XmlNotWfDtd05()
        {
            Assert.Throws<Exception>(() => { var document = @"<!DOCTYPE root [
    <!ELEMENT root EMPTY>
    <!-- SYSTEM id must be quoted -->
    <!ENTITY foo SYSTEM elvis.ent>
]>

<root/>
".ToXmlDocument(); });
        }

        /// <summary>
        /// terminating ';' is required Here the section(s) 4.1 [66] apply.
        /// This test is taken from the collection OASIS/NIST TESTS, 1-Nov-1998.
        /// </summary>
        [Test]

        public void XmlNotWfOP66fail1()
        {
            Assert.Throws<Exception>(() => { var document = @"<doc>&#65</doc>".ToXmlDocument(); });
        }

        /// <summary>
        /// no S after '&#' Here the section(s) 4.1 [66] apply. This test is
        /// taken from the collection OASIS/NIST TESTS, 1-Nov-1998.
        /// </summary>
        [Test]

        public void XmlNotWfOP66fail2()
        {
            Assert.Throws<Exception>(() => { var document = @"<doc>&# 65;</doc>".ToXmlDocument(); });
        }

        /// <summary>
        /// no hex digits in numeric reference Here the section(s) 4.1 [66] apply.
        /// This test is taken from the collection OASIS/NIST TESTS, 1-Nov-1998.
        /// </summary>
        [Test]

        public void XmlNotWfOP66fail3()
        {
            Assert.Throws<Exception>(() => { var document = @"<doc>&#A;</doc>".ToXmlDocument(); });
        }

        /// <summary>
        /// only hex digits in hex references Here the section(s) 4.1 [66] apply.
        /// This test is taken from the collection OASIS/NIST TESTS, 1-Nov-1998.
        /// </summary>
        [Test]

        public void XmlNotWfOP66fail4()
        {
            Assert.Throws<Exception>(() => { var document = @"<doc>&#x4G;</doc>".ToXmlDocument(); });
        }

        /// <summary>
        /// No references to non-characters. Here the section(s) 4.1 [66] apply.
        /// This test is taken from the collection OASIS/NIST TESTS, 1-Nov-1998.
        /// </summary>
        [Test]

        public void XmlNotWfOP66fail5()
        {
            Assert.Throws<Exception>(() => { var document = @"<doc>&#5;</doc>".ToXmlDocument(); });
        }

        /// <summary>
        /// no references to non-characters Here the section(s) 4.1 [66] apply.
        /// This test is taken from the collection OASIS/NIST TESTS, 1-Nov-1998.
        /// </summary>
        [Test]

        public void XmlNotWfOP66fail6()
        {
            Assert.Throws<Exception>(() => { var document = @"<doc>&#xd802;&#xdc02;</doc>".ToXmlDocument(); });
        }

        /// <summary>
        /// terminating ';' is required Here the section(s) 4.1 [68] apply.
        /// This test is taken from the collection OASIS/NIST TESTS, 1-Nov-1998.
        /// </summary>
        [Test]

        public void XmlNotWfOP68fail1()
        {
            Assert.Throws<Exception>(() => { var document = @"<!DOCTYPE doc
[
<!ELEMENT doc (#PCDATA)>
<!ENTITY ent ""replacement text"">
]>
<doc>
&ent
</doc>
".ToXmlDocument(); });
        }

        /// <summary>
        /// no S after '&' Here the section(s) 4.1 [68] apply.
        /// This test is taken from the collection OASIS/NIST TESTS, 1-Nov-1998.
        /// </summary>
        [Test]

        public void XmlNotWfOP68fail2()
        {
            Assert.Throws<Exception>(() => { var document = @"<!DOCTYPE doc
[
<!ELEMENT doc (#PCDATA)>
<!ENTITY ent ""replacement text"">
]>
<doc>
& ent;
</doc>
".ToXmlDocument(); });
        }

        /// <summary>
        /// no S before ';' Here the section(s) 4.1 [68] apply. This test is
        /// taken from the collection OASIS/NIST TESTS, 1-Nov-1998.
        /// </summary>
        [Test]

        public void XmlNotWfOP68fail3()
        {
            Assert.Throws<Exception>(() => { var document = @"<!DOCTYPE doc
[
<!ELEMENT doc (#PCDATA)>
<!ENTITY ent ""replacement text"">
]>
<doc>
&ent ;
</doc>
".ToXmlDocument(); });
        }

        /// <summary>
        /// PE name immediately after "%" Here the section(s) 4.1 [69] apply.
        /// This test is taken from the collection Sun Microsystems XML Tests.
        /// </summary>
        [Test]

        public void XmlNotWfDtd02()
        {
            Assert.Throws<Exception>(() => { var document = @"<!DOCTYPE root [
    <!ELEMENT root EMPTY>
    <!-- correct PE ref syntax -->
    <!ENTITY % foo ""<!ATTLIST root>"">
    % foo;
]>

<root/>
".ToXmlDocument(); });
        }

        /// <summary>
        /// PE name immediately followed by ";" Here the section(s) 4.1 [69] apply.
        /// This test is taken from the collection Sun Microsystems XML Tests.
        /// </summary>
        [Test]

        public void XmlNotWfDtd03()
        {
            Assert.Throws<Exception>(() => { var document = @"<!DOCTYPE root [
    <!ELEMENT root EMPTY>
    <!-- correct PE ref syntax -->
    <!ENTITY % foo ""<!ATTLIST root>"">
    %foo
    ;
]>

<root/>

".ToXmlDocument(); });
        }

        /// <summary>
        /// Must have keyword in conditional sections Here the section(s) 3.4 [61] apply.
        /// This test is taken from the collection Sun Microsystems XML Tests.
        /// </summary>
        [Test]

        public void XmlNotWfCond02()
        {
            Assert.Throws<Exception>(() => { var document = @"<!DOCTYPE root [
    <!ELEMENT root EMPTY>
    <!-- correct PE ref syntax -->
    <!ENTITY % foo ""<!ATTLIST root>"">
    %foo
    ;
]>

<root/>

".ToXmlDocument(); });
        }

        /// <summary>
        /// SGML-ism: omitted end tag for EMPTY content Here the section(s) 3 [39] apply.
        /// This test is taken from the collection Sun Microsystems XML Tests.
        /// </summary>
        [Test]

        public void XmlNotWfSgml01()
        {
            Assert.Throws<Exception>(() => { var document = @"<!DOCTYPE root [
    <!ELEMENT root EMPTY>

    <!-- SGML-ism:  omitted end tag -->
]>

<root>
".ToXmlDocument(); });
        }

        /// <summary>
        /// start-tag requires end-tag Here the section(s) 3 [39] apply.
        /// This test is taken from the collection OASIS/NIST TESTS, 1-Nov-1998.
        /// </summary>
        [Test]

        public void XmlNotWfOP39fail1()
        {
            Assert.Throws<Exception>(() => { var document = @"<doc>content".ToXmlDocument(); });
        }

        /// <summary>
        /// end-tag requires start-tag Here the section(s) 3 [39] apply. This test is
        /// taken from the collection OASIS/NIST TESTS, 1-Nov-1998.
        /// </summary>
        [Test]

        public void XmlNotWfOP39fail2()
        {
            Assert.Throws<Exception>(() => { var document = @"<doc>content</a></doc>".ToXmlDocument(); });
        }

        /// <summary>
        /// XML documents contain one or more elements. Here the section(s) 3 [39] apply.
        /// This test is taken from the collection OASIS/NIST TESTS, 1-Nov-1998.
        /// </summary>
        [Test]

        public void XmlNotWfOP39fail3()
        {
            Assert.Throws<Exception>(() => { var document = @"".ToXmlDocument(); });
        }

        /// <summary>
        /// A name is required. Here the section(s) 3.3 [52] apply. This test is
        /// taken from the collection OASIS/NIST TESTS, 1-Nov-1998.
        /// </summary>
        [Test]

        public void XmlNotWfOP52fail1()
        {
            Assert.Throws<Exception>(() => { var document = @"<!DOCTYPE doc
[
<!ELEMENT doc EMPTY>
<!ATTLIST  >
]>
<doc/>".ToXmlDocument(); });
        }

        /// <summary>
        /// A name is required. Here the section(s) 3.3 [52] apply. This test is
        /// taken from the collection OASIS/NIST TESTS, 1-Nov-1998.
        /// </summary>
        [Test]

        public void XmlNotWfOP52fail2()
        {
            Assert.Throws<Exception>(() => { var document = @"<!DOCTYPE doc
[
<!ELEMENT doc EMPTY>
<!ATTLIST>
]>
<doc/>".ToXmlDocument(); });
        }

        /// <summary>
        /// S is required before default. Here the section(s) 3.3 [53] apply.
        /// This test is taken from the collection OASIS/NIST TESTS, 1-Nov-1998.
        /// </summary>
        [Test]

        public void XmlNotWfOP53fail1()
        {
            Assert.Throws<Exception>(() => { var document = @"<!DOCTYPE doc
[
<!ELEMENT doc EMPTY>
<!ATTLIST doc att CDATA#IMPLIED>
]>
<doc/>
".ToXmlDocument(); });
        }

        /// <summary>
        /// S is required before type. Here the section(s) 3.3 [53] apply. This
        /// test is taken from the collection OASIS/NIST TESTS, 1-Nov-1998.
        /// </summary>
        [Test]

        public void XmlNotWfOP53fail2()
        {
            Assert.Throws<Exception>(() => { var document = @"<!DOCTYPE doc
[
<!ELEMENT doc EMPTY>
<!ATTLIST doc att(a|b) #IMPLIED>
]>
<doc/>
".ToXmlDocument(); });
        }

        /// <summary>
        /// Type is required. Here the section(s) 3.3 [53] apply. This test is
        /// taken from the collection OASIS/NIST TESTS, 1-Nov-1998.
        /// </summary>
        [Test]

        public void XmlNotWfOP53fail3()
        {
            Assert.Throws<Exception>(() => { var document = @"<!DOCTYPE doc
[
<!ELEMENT doc EMPTY>
<!ATTLIST doc att #IMPLIED>
]>
<doc/>
".ToXmlDocument(); });
        }

        /// <summary>
        /// Default is required. Here the section(s) 3.3 [53] apply. This test is taken from
        /// the collection OASIS/NIST TESTS, 1-Nov-1998.
        /// </summary>
        [Test]

        public void XmlNotWfOP53fail4()
        {
            Assert.Throws<Exception>(() => { var document = @"<!DOCTYPE doc
[
<!ELEMENT doc EMPTY>
<!ATTLIST doc att CDATA>
]>
<doc/>
".ToXmlDocument(); });
        }

        /// <summary>
        /// Name is requried. Here the section(s) 3.3 [53] apply. This test is taken from
        /// the collection OASIS/NIST TESTS, 1-Nov-1998.
        /// </summary>
        [Test]

        public void XmlNotWfOP53fail5()
        {
            Assert.Throws<Exception>(() => { var document = @"<!DOCTYPE doc
[
<!ELEMENT doc EMPTY>
<!ATTLIST doc (a|b) #IMPLIED>
]>
<doc/>
".ToXmlDocument(); });
        }

        /// <summary>
        /// Comma doesn't separate enumerations, unlike in SGML. Here the section(s) 3.3.1 [59]
        /// apply. This test is taken from the collection Sun Microsystems XML Tests.
        /// </summary>
        [Test]

        public void XmlNotWfAttlist03()
        {
            Assert.Throws<Exception>(() => { var document = @"<!DOCTYPE root [
    <!ELEMENT root EMPTY>

    <!-- SGML-ism:  illegal attribute types -->

    <!ATTLIST root
	choice	(a,b,c)	""a""
	>

]>

<root/>

".ToXmlDocument(); });
        }

        /// <summary>
        /// at least one required Here the section(s) 3.3.1 [59] apply. This test is taken
        /// from the collection OASIS/NIST TESTS, 1-Nov-1998.
        /// </summary>
        [Test]

        public void XmlNotWfOP59fail1()
        {
            Assert.Throws<Exception>(() => { var document = @"<!DOCTYPE doc
[
<!ELEMENT doc EMPTY>
<!ATTLIST doc att () #IMPLIED>
]>
<doc/>
".ToXmlDocument(); });
        }

        /// <summary>
        /// separator must be "," Here the section(s) 3.3.1 [59] apply. This test is
        /// taken from the collection OASIS/NIST TESTS, 1-Nov-1998.
        /// </summary>
        [Test]

        public void XmlNotWfOP59fail2()
        {
            Assert.Throws<Exception>(() => { var document = @"<!DOCTYPE doc
[
<!ELEMENT doc EMPTY>
<!ATTLIST doc att (a,b) #IMPLIED>
]>
<doc/>
".ToXmlDocument(); });
        }

        /// <summary>
        /// values are unquoted Here the section(s) 3.3.1 [59] apply. This test is taken
        /// from the collection OASIS/NIST TESTS, 1-Nov-1998.
        /// </summary>
        [Test]

        public void XmlNotWfOP59fail3()
        {
            Assert.Throws<Exception>(() => { var document = @"<!DOCTYPE doc
[
<!ELEMENT doc EMPTY>
<!ATTLIST doc att (""a"") #IMPLIED>
]>
<doc/>
".ToXmlDocument(); });
        }

        /// <summary>
        /// keywords must be upper case Here the section(s) 3.3.2 [60] apply. This test is
        /// taken from the collection OASIS/NIST TESTS, 1-Nov-1998.
        /// </summary>
        [Test]

        public void XmlNotWfOP60fail1()
        {
            Assert.Throws<Exception>(() => { var document = @"<!DOCTYPE doc
[
<!ELEMENT doc EMPTY>
<!ATTLIST doc att CDATA #implied>
]>
<doc/>
".ToXmlDocument(); });
        }

        /// <summary>
        /// S is required after #FIXED Here the section(s) 3.3.2 [60] apply. This test is
        /// taken from the collection OASIS/NIST TESTS, 1-Nov-1998.
        /// </summary>
        [Test]

        public void XmlNotWfOP60fail2()
        {
            Assert.Throws<Exception>(() => { var document = @"<!DOCTYPE doc
[
<!ELEMENT doc EMPTY>
<!ATTLIST doc att CDATA #FIXED""value"">
]>
<doc/>
".ToXmlDocument(); });
        }

        /// <summary>
        /// only #FIXED has both keyword and value Here the section(s) 3.3.2 [60] apply. This test
        /// is taken from the collection OASIS/NIST TESTS, 1-Nov-1998.
        /// </summary>
        [Test]

        public void XmlNotWfOP60fail3()
        {
            Assert.Throws<Exception>(() => { var document = @"<!DOCTYPE doc
[
<!ELEMENT doc EMPTY>
<!ATTLIST doc att CDATA #REQUIRED ""value"">
]>
<doc att=""value""/>
".ToXmlDocument(); });
        }

        /// <summary>
        /// #FIXED required value Here the section(s) 3.3.2 [60] apply. This test is taken
        /// from the collection OASIS/NIST TESTS, 1-Nov-1998.
        /// </summary>
        [Test]

        public void XmlNotWfOP60fail4()
        {
            Assert.Throws<Exception>(() => { var document = @"<!DOCTYPE doc
[
<!ELEMENT doc EMPTY>
<!ATTLIST doc att CDATA #FIXED>
]>
<doc/>
".ToXmlDocument(); });
        }

        /// <summary>
        /// only one default type Here the section(s) 3.3.2 [60] apply. This test is taken
        /// from the collection OASIS/NIST TESTS, 1-Nov-1998.
        /// </summary>
        [Test]

        public void XmlNotWfOP60fail5()
        {
            Assert.Throws<Exception>(() => { var document = @"<!DOCTYPE doc
[
<!ELEMENT doc EMPTY>
<!ATTLIST doc att CDATA #IMPLIED #REQUIRED>
]>
<doc att=""value""/>
".ToXmlDocument(); });
        }

        /// <summary>
        /// ATTLIST declarations apply to only one element, unlike SGML Here the section(s) 3.3 [52] apply.
        /// This test is taken from the collection Sun Microsystems XML Tests.
        /// </summary>
        [Test]

        public void XmlNotWfSgml04()
        {
            Assert.Throws<Exception>(() => { var document = @"<!DOCTYPE root [
    <!-- SGML-ism:  multiple attlist types -->

    <!ELEMENT root EMPTY>
    <!ELEMENT branch EMPTY>

    <!ATTLIST (root|branch)
	TreeType CDATA #REQUIRED
	>
]>

<root/>
".ToXmlDocument(); });
        }

        /// <summary>
        /// ATTLIST declarations are never global, unlike in SGML Here the section(s) 3.3 [52] apply.
        /// This test is taken from the collection Sun Microsystems XML Tests.
        /// </summary>
        [Test]

        public void XmlNotWfSgml06()
        {
            Assert.Throws<Exception>(() => { var document = @"<!DOCTYPE root [
    <!-- Web-SGML-ism:  global attlist types -->

    <!ELEMENT root EMPTY>

    <!ATTLIST #ALL
	TreeType CDATA #REQUIRED
	>
]>

<root/>
".ToXmlDocument(); });
        }

        /// <summary>
        /// SGML's #CURRENT is not allowed. Here the section(s) 3.3.1 [56] apply. This test is
        /// taken from the collection Sun Microsystems XML Tests.
        /// </summary>
        [Test]

        public void XmlNotWfAttlist08()
        {
            Assert.Throws<Exception>(() => { var document = @"<!DOCTYPE root [
    <!ELEMENT root EMPTY>

    <!-- SGML-ism:  illegal attribute default -->

    <!ATTLIST root
	language	CDATA	#CURRENT
	>

]>

<root/>
".ToXmlDocument(); });
        }

        /// <summary>
        /// SGML's #CONREF is not allowed. Here the section(s) 3.3.1 [56] apply. This test
        /// is taken from the collection Sun Microsystems XML Tests.
        /// </summary>
        [Test]

        public void XmlNotWfAttlist09()
        {
            Assert.Throws<Exception>(() => { var document = @"<!DOCTYPE root [
    <!-- SGML-ism:  illegal attribute default -->

    <!ATTLIST root
	language	CDATA	#CONREF
	>

]>

<root language=""Dutch""/>

".ToXmlDocument(); });
        }

        /// <summary>
        /// no IDS type Here the section(s) 3.3.1 [56] apply. This test is taken from
        /// the collection OASIS/NIST TESTS, 1-Nov-1998.
        /// </summary>
        [Test]

        public void XmlNotWfOP56fail1()
        {
            Assert.Throws<Exception>(() => { var document = @"<!DOCTYPE doc
[
<!ELEMENT doc EMPTY>
<!ATTLIST doc att IDS #IMPLIED>
]>
<doc/>
".ToXmlDocument(); });
        }

        /// <summary>
        /// no NUMBER type Here the section(s) 3.3.1 [56] apply. This test is taken
        /// from the collection OASIS/NIST TESTS, 1-Nov-1998.
        /// </summary>
        [Test]

        public void XmlNotWfOP56fail2()
        {
            Assert.Throws<Exception>(() => { var document = @"<!DOCTYPE doc
[
<!ELEMENT doc EMPTY>
<!ATTLIST doc att NUMBER #IMPLIED>
]>
<doc/>
".ToXmlDocument(); });
        }

        /// <summary>
        /// no NAME type Here the section(s) 3.3.1 [56] apply. This test is taken from
        /// the collection OASIS/NIST TESTS, 1-Nov-1998.
        /// </summary>
        [Test]

        public void XmlNotWfOP56fail3()
        {
            Assert.Throws<Exception>(() => { var document = @"<!DOCTYPE doc
[
<!ELEMENT doc EMPTY>
<!ATTLIST doc att NAME #IMPLIED>
]>
<doc/>
".ToXmlDocument(); });
        }

        /// <summary>
        /// no ENTITYS type - types must be upper case Here the section(s) 3.3.1 [56] apply.
        /// This test is taken from the collection OASIS/NIST TESTS, 1-Nov-1998.
        /// </summary>
        [Test]

        public void XmlNotWfOP56fail4()
        {
            Assert.Throws<Exception>(() => { var document = @"<!DOCTYPE doc
[
<!ELEMENT doc EMPTY>
<!ATTLIST doc att ENTITYS #IMPLIED>
]>
<doc/>
".ToXmlDocument(); });
        }

        /// <summary>
        /// types must be upper case Here the section(s) 3.3.1 [56] apply. This test is taken
        /// from the collection OASIS/NIST TESTS, 1-Nov-1998.
        /// </summary>
        [Test]

        public void XmlNotWfOP56fail5()
        {
            Assert.Throws<Exception>(() => { var document = @"<!DOCTYPE doc
[
<!ELEMENT doc EMPTY>
<!ATTLIST doc att id #IMPLIED>
]>
<doc/>
".ToXmlDocument(); });
        }

        /// <summary>
        /// no keyword for NMTOKEN enumeration Here the section(s) 3.3.1 [57] apply. This
        /// test is taken from the collection OASIS/NIST TESTS, 1-Nov-1998.
        /// </summary>
        [Test]

        public void XmlNotWfOP57fail1()
        {
            Assert.Throws<Exception>(() => { var document = @"<!DOCTYPE doc
[
<!ELEMENT doc EMPTY>
<!ATTLIST doc att NMTOKEN (a|b) #IMPLIED>
]>
<doc/>
".ToXmlDocument(); });
        }

        /// <summary>
        /// at least one value required Here the section(s) 3.3.1 [58] apply. This test
        /// is taken from the collection OASIS/NIST TESTS, 1-Nov-1998.
        /// </summary>
        [Test]

        public void XmlNotWfOP58fail1()
        {
            Assert.Throws<Exception>(() => { var document = @"<!DOCTYPE doc
[
<!ELEMENT doc EMPTY>
<!NOTATION a SYSTEM ""a"">
<!NOTATION b SYSTEM ""b"">
<!ATTLIST doc att NOTATION () #IMPLIED>
]>
<doc/>
".ToXmlDocument(); });
        }

        /// <summary>
        /// Separator must be '|' Here the section(s) 3.3.1 [58] apply. This test
        /// is taken from the collection OASIS/NIST TESTS, 1-Nov-1998.
        /// </summary>
        [Test]

        public void XmlNotWfOP58fail2()
        {
            Assert.Throws<Exception>(() => { var document = @"<!DOCTYPE doc
[
<!ELEMENT doc EMPTY>
<!NOTATION a SYSTEM ""a"">
<!NOTATION b SYSTEM ""b"">
<!ATTLIST doc att NOTATION (a,b) #IMPLIED>
]>
<doc/>
".ToXmlDocument(); });
        }

        /// <summary>
        /// Notations are NAMEs, not NMTOKENs -- note: Leaving the invalid notation undeclared would
        /// cause a validating parser to fail without checking the name syntax, so the notation is
        /// declared with an invalid name. A parser that reports error positions should report an error
        /// at the AttlistDecl on line 6, before reaching the notation declaration. Here the section(s)
        /// 3.3.1 [58] apply. This test is taken from the collection OASIS/NIST TESTS, 1-Nov-1998.
        /// </summary>
        [Test]

        public void XmlNotWfOP58fail3()
        {
            Assert.Throws<Exception>(() => { var document = @"<!DOCTYPE doc
[
<!ELEMENT doc EMPTY>
<!NOTATION a SYSTEM ""a"">
<!--should fail at this AttlistDecl, before NOTATION decl-->
<!ATTLIST doc att NOTATION (a|0b) #IMPLIED>



<!NOTATION 0b SYSTEM ""0b"">
]>
<doc/>
".ToXmlDocument(); });
        }

        /// <summary>
        /// NOTATION must be upper case Here the section(s) 3.3.1 [58] apply.
        /// This test is taken from the collection OASIS/NIST TESTS, 1-Nov-1998.
        /// </summary>
        [Test]

        public void XmlNotWfOP58fail4()
        {
            Assert.Throws<Exception>(() => { var document = @"<!DOCTYPE doc
[
<!ELEMENT doc EMPTY>
<!NOTATION a SYSTEM ""a"">
<!NOTATION b SYSTEM ""b"">
<!ATTLIST doc att notation (a|b) #IMPLIED>
]>
<doc/>
".ToXmlDocument(); });
        }

        /// <summary>
        /// S after keyword is required Here the section(s) 3.3.1 [58] apply.
        /// This test is taken from the collection OASIS/NIST TESTS, 1-Nov-1998.
        /// </summary>
        [Test]

        public void XmlNotWfOP58fail5()
        {
            Assert.Throws<Exception>(() => { var document = @"<!DOCTYPE doc
[
<!ELEMENT doc EMPTY>
<!NOTATION a SYSTEM ""a"">
<!NOTATION b SYSTEM ""b"">
<!ATTLIST doc att NOTATION(a|b) #IMPLIED>
]>
<doc/>
".ToXmlDocument(); });
        }

        /// <summary>
        /// Parentheses are require Here the section(s) 3.3.1 [58] apply.
        /// This test is taken from the collection OASIS/NIST TESTS, 1-Nov-1998.
        /// </summary>
        [Test]

        public void XmlNotWfOP58fail6()
        {
            Assert.Throws<Exception>(() => { var document = @"<!DOCTYPE doc
[
<!ELEMENT doc EMPTY>
<!NOTATION a SYSTEM ""a"">
<!ATTLIST doc att NOTATION a #IMPLIED>
]>
<doc/>
".ToXmlDocument(); });
        }
        /// <summary>
        /// Values are unquoted Here the section(s) 3.3.1 [58] apply.
        /// This test is taken from the collection OASIS/NIST TESTS, 1-Nov-1998.
        /// </summary>
        [Test]

        public void XmlNotWfOP58fail7()
        {
            Assert.Throws<Exception>(() => { var document = @"<!DOCTYPE doc
[
<!ELEMENT doc EMPTY>
<!NOTATION a SYSTEM ""a"">
<!ATTLIST doc att NOTATION ""a"" #IMPLIED>
]>
<doc/>
".ToXmlDocument(); });
        }

        /// <summary>
        /// Values are unquoted Here the section(s) 3.3.1 [58] apply.
        /// This test is taken from the collection OASIS/NIST TESTS, 1-Nov-1998.
        /// </summary>
        [Test]

        public void XmlNotWfOP58fail8()
        {
            Assert.Throws<Exception>(() => { var document = @"<!DOCTYPE doc
[
<!ELEMENT doc EMPTY>
<!NOTATION a SYSTEM ""a"">
<!ATTLIST doc att NOTATION (""a"") #IMPLIED>
]>
<doc/>
".ToXmlDocument(); });
        }

        /// <summary>
        /// Don't pass unknown attribute types Here the section(s) 3.3.1 [54] apply.
        /// This test is taken from the collection OASIS/NIST TESTS, 1-Nov-1998.
        /// </summary>
        [Test]

        public void XmlNotWfOP54fail1()
        {
            Assert.Throws<Exception>(() => { var document = @"<!DOCTYPE doc
[
<!ELEMENT doc EMPTY>
<!ATTLIST doc att DUNNO #IMPLIED>
]>
<doc/>
".ToXmlDocument(); });
        }

        /// <summary>
        /// Must be upper case Here the section(s) 3.3.1 [55] apply.
        /// This test is taken from the collection OASIS/NIST TESTS, 1-Nov-1998.
        /// </summary>
        [Test]

        public void XmlNotWfOP55fail1()
        {
            Assert.Throws<Exception>(() => { var document = @"<!DOCTYPE doc
[
<!ELEMENT doc EMPTY>
<!ATTLIST doc att cdata #IMPLIED>
]>
<doc/>
".ToXmlDocument(); });
        }

        /// <summary>
        /// SGML's NUTOKEN is not allowed. Here the section(s) 3.3.1 [56] apply.
        /// This test is taken from the collection Sun Microsystems XML Tests.
        /// </summary>
        [Test]

        public void XmlNotWfAttlist01()
        {
            Assert.Throws<Exception>(() => { var document = @"<!DOCTYPE root [
    <!ELEMENT root EMPTY>

    <!-- SGML-ism:  illegal attribute types -->

    <!ATTLIST root
	number	NUTOKEN	""1""
	>

]>

<root/>
".ToXmlDocument(); });
        }

        /// <summary>
        /// SGML's NUTOKENS attribute type is not allowed. Here the section(s) 3.3.1 [56] apply.
        /// This test is taken from the collection Sun Microsystems XML Tests.
        /// </summary>
        [Test]

        public void XmlNotWfAttlist02()
        {
            Assert.Throws<Exception>(() => { var document = @"<!DOCTYPE root [
    <!ELEMENT root EMPTY>

    <!-- SGML-ism:  illegal attribute types -->

    <!ATTLIST root
	number	NUTOKENS	""1 2 3""
	>

]>

<root/>

".ToXmlDocument(); });
        }

        /// <summary>
        /// SGML's NUMBER attribute type is not allowed. Here the section(s) 3.3.1 [56] apply.
        /// This test is taken from the collection Sun Microsystems XML Tests.
        /// </summary>
        [Test]

        public void XmlNotWfAttlist04()
        {
            Assert.Throws<Exception>(() => { var document = @"<!DOCTYPE root [
    <!ELEMENT root EMPTY>

    <!-- SGML-ism:  illegal attribute types -->

    <!ATTLIST root
	number	NUMBER	""1""
	>

]>

<root/>

".ToXmlDocument(); });
        }

        /// <summary>
        /// SGML's NUMBERS attribute type is not allowed. Here the section(s) 3.3.1 [56] apply.
        /// This test is taken from the collection Sun Microsystems XML Tests.
        /// </summary>
        [Test]

        public void XmlNotWfAttlist05()
        {
            Assert.Throws<Exception>(() => { var document = @"<!DOCTYPE root [
    <!ELEMENT root EMPTY>

    <!-- SGML-ism:  illegal attribute types -->

    <!ATTLIST root
	numbers	NUMBERS	""1 2 3 4""
	>

]>

<root/>

".ToXmlDocument(); });
        }

        /// <summary>
        /// SGML's NAME attribute type is not allowed. Here the section(s) 3.3.1 [56] apply.
        /// This test is taken from the collection Sun Microsystems XML Tests.
        /// </summary>
        [Test]

        public void XmlNotWfAttlist06()
        {
            Assert.Throws<Exception>(() => { var document = @"<!DOCTYPE root [
    <!ELEMENT root EMPTY>

    <!-- SGML-ism:  illegal attribute types -->

    <!ATTLIST root
	number	NAME	""Elvis""
	>

]>

<root/>

".ToXmlDocument(); });
        }

        /// <summary>
        /// SGML's NAMES attribute type is not allowed. Here the section(s) 3.3.1 [56] apply.
        /// This test is taken from the collection Sun Microsystems XML Tests.
        /// </summary>
        [Test]

        public void XmlNotWfAttlist07()
        {
            Assert.Throws<Exception>(() => { var document = @"<!DOCTYPE root [
    <!ELEMENT root EMPTY>

    <!-- SGML-ism:  illegal attribute types -->

    <!ATTLIST root
	number	NAMES	""The King""
	>

]>

<root/>

".ToXmlDocument(); });
        }

        /// <summary>
        /// occurrence on #PCDATA group must be *. Here the section(s) 3.2.2 [51] apply.
        /// This test is taken from the collection OASIS/NIST TESTS, 1-Nov-1998.
        /// </summary>
        [Test]

        public void XmlNotWfOP51fail2()
        {
            Assert.Throws<Exception>(() => { var document = @"<!DOCTYPE doc
[
<!ELEMENT doc (#PCDATA)+>
]>
<doc/>".ToXmlDocument(); });
        }

        /// <summary>
        /// #PCDATA must come first. Here the section(s) 3.2.2 [51] apply.
        /// This test is taken from the collection OASIS/NIST TESTS, 1-Nov-1998.
        /// </summary>
        [Test]

        public void XmlNotWfOP51fail3()
        {
            Assert.Throws<Exception>(() => { var document = @"<!DOCTYPE doc
[
<!ELEMENT doc (#PCDATA)>
<!ELEMENT a (doc|#PCDATA)*>
]>
<doc/>".ToXmlDocument(); });
        }

        /// <summary>
        /// occurrence on #PCDATA group must be *. Here the section(s) 3.2.2 [51] apply.
        /// This test is taken from the collection OASIS/NIST TESTS, 1-Nov-1998.
        /// </summary>
        [Test]

        public void XmlNotWfOP51fail4()
        {
            Assert.Throws<Exception>(() => { var document = @"<!DOCTYPE doc
[
<!ELEMENT doc (#PCDATA)>
<!ELEMENT a (#PCDATA|doc)?>
]>
<doc/>".ToXmlDocument(); });
        }

        /// <summary>
        /// Only '|' connectors. Here the section(s) 3.2.2 [51] apply.
        /// This test is taken from the collection OASIS/NIST TESTS, 1-Nov-1998.
        /// </summary>
        [Test]

        public void XmlNotWfOP51fail5()
        {
            Assert.Throws<Exception>(() => { var document = @"<!DOCTYPE doc
[
<!ELEMENT doc (#PCDATA)>
<!ELEMENT a (#PCDATA|doc,a?)*>
]>
<doc/>".ToXmlDocument(); });
        }

        /// <summary>
        /// Only '|' connectors and occurrence on #PCDATA group must be *. Here the section(s) 3.2.2 [51] apply.
        /// This test is taken from the collection OASIS/NIST TESTS, 1-Nov-1998.
        /// </summary>
        [Test]

        public void XmlNotWfOP51fail6()
        {
            Assert.Throws<Exception>(() => { var document = @"<!DOCTYPE doc
[
<!ELEMENT doc (#PCDATA)>
<!ELEMENT a (#PCDATA,doc,a?)*>
]>
<doc/>".ToXmlDocument(); });
        }

        /// <summary>
        /// No nested groups. Here the section(s) 3.2.2 [51] apply.
        /// This test is taken from the collection OASIS/NIST TESTS, 1-Nov-1998.
        /// </summary>
        [Test]

        public void XmlNotWfOP51fail7()
        {
            Assert.Throws<Exception>(() => { var document = @"<!DOCTYPE doc
[
<!ELEMENT doc (#PCDATA)>
<!ELEMENT a (#PCDATA|(doc|a))*>
]>
<doc/>".ToXmlDocument(); });
        }

        /// <summary>
        /// ELEMENT declarations apply to only one element, unlike SGML. Here the section(s) 3.2 [45] apply.
        /// This test is taken from the collection Sun Microsystems XML Tests.
        /// </summary>
        [Test]

        public void XmlNotWfSgml05()
        {
            Assert.Throws<Exception>(() => { var document = @"<!DOCTYPE root [
    <!-- SGML-ism:  multiple element types -->

    <!ELEMENT root EMPTY>
    <!ELEMENT leaves EMPTY>
    <!ELEMENT branch EMPTY>

    <!ELEMENT (bush|tree) (root,leaves,branch)>
]>

<root/>

".ToXmlDocument(); });
        }

        /// <summary>
        /// SGML Tag minimization specifications are not allowed. Here the section(s) 3.2 [45] apply.
        /// This test is taken from the collection Sun Microsystems XML Tests.
        /// </summary>
        [Test]

        public void XmlNotWfSgml07()
        {
            Assert.Throws<Exception>(() => { var document = @"<!DOCTYPE root [
    <!-- SGML-ism:  omitted tag minimzation spec -->
    <!ELEMENT root - o EMPTY>
]>

<root/>
".ToXmlDocument(); });
        }

        /// <summary>
        /// SGML Tag minimization specifications are not allowed. Here the section(s) 3.2 [45] apply.
        /// This test is taken from the collection Sun Microsystems XML Tests.
        /// </summary>
        [Test]

        public void XmlNotWfSgml08()
        {
            Assert.Throws<Exception>(() => { var document = @"<!DOCTYPE root [
    <!-- SGML-ism:  omitted tag minimzation spec -->
    <!ELEMENT root - - EMPTY>
]>

<root/>

".ToXmlDocument(); });
        }

        /// <summary>
        /// SGML Content model exception specifications are not allowed. Here the section(s) 3.2 [45] apply.
        /// This test is taken from the collection Sun Microsystems XML Tests.
        /// </summary>
        [Test]

        public void XmlNotWfSgml09()
        {
            Assert.Throws<Exception>(() => { var document = @"<!DOCTYPE root [
    <!-- SGML-ism:  exception spec -->

    <!ELEMENT footnote (para*) -footnote>
]>

<root/>

".ToXmlDocument(); });
        }

        /// <summary>
        /// SGML Content model exception specifications are not allowed. Here the section(s) 3.2 [45] apply.
        /// This test is taken from the collection Sun Microsystems XML Tests.
        /// </summary>
        [Test]

        public void XmlNotWfSgml10()
        {
            Assert.Throws<Exception>(() => { var document = @"<!DOCTYPE root [
    <!-- SGML-ism:  exception spec -->
    <!ELEMENT section (header,(para|section))* +(annotation|todo)>
]>

<root/>

".ToXmlDocument(); });
        }

        /// <summary>
        /// ELEMENT must be upper case. Here the section(s) 3.2 [45] apply.
        /// This test is taken from the collection OASIS/NIST TESTS, 1-Nov-1998.
        /// </summary>
        [Test]

        public void XmlNotWfOP45fail1()
        {
            Assert.Throws<Exception>(() => { var document = @"<!DOCTYPE doc
[
<!element doc EMPTY>
]>
<doc/>".ToXmlDocument(); });
        }

        /// <summary>
        /// S before contentspec is required. Here the section(s) 3.2 [45] apply.
        /// This test is taken from the collection OASIS/NIST TESTS, 1-Nov-1998.
        /// </summary>
        [Test]

        public void XmlNotWfOP45fail2()
        {
            Assert.Throws<Exception>(() => { var document = @"<!DOCTYPE doc
[
<!ELEMENT doc(#PCDATA)>
]>
<doc/>".ToXmlDocument(); });
        }

        /// <summary>
        /// only one content spec Here the section(s) 3.2 [45] apply.
        /// This test is taken from the collection OASIS/NIST TESTS, 1-Nov-1998.
        /// </summary>
        [Test]

        public void XmlNotWfOP45fail3()
        {
            Assert.Throws<Exception>(() => { var document = @"<!DOCTYPE doc
[
<!ELEMENT (doc|a) (#PCDATA)>
]>
<doc/>".ToXmlDocument(); });
        }

        /// <summary>
        /// no comments in declarations (contrast with SGML) Here the section(s) 3.2 [45] apply.
        /// This test is taken from the collection OASIS/NIST TESTS, 1-Nov-1998.
        /// </summary>
        [Test]

        public void XmlNotWfOP45fail4()
        {
            Assert.Throws<Exception>(() => { var document = @"<!DOCTYPE doc
[
<!ELEMENT doc (#PCDATA) --bad comment-->
]>
<doc/>".ToXmlDocument(); });
        }

        /// <summary>
        /// CDATA is not a valid content model spec Here the section(s) 3.2 [46] apply.
        /// This test is taken from the collection Sun Microsystems XML Tests.
        /// </summary>
        [Test]

        public void XmlNotWfSgml11()
        {
            Assert.Throws<Exception>(() => { var document = @"<!DOCTYPE root [
    <!-- SGML-ism:  CDATA content type -->
    <!ELEMENT ROOT CDATA>
]>

<root/>

".ToXmlDocument(); });
        }

        /// <summary>
        /// RCDATA is not a valid content model spec Here the section(s) 3.2 [46] apply.
        /// This test is taken from the collection Sun Microsystems XML Tests.
        /// </summary>
        [Test]

        public void XmlNotWfSgml12()
        {
            Assert.Throws<Exception>(() => { var document = @"<!DOCTYPE root [
    <!-- SGML-ism:  RCDATA content type -->
    <!ELEMENT ROOT RCDATA>
]>

<root/>


".ToXmlDocument(); });
        }

        /// <summary>
        /// No parens on declared content. Here the section(s) 3.2 [46] apply.
        /// This test is taken from the collection OASIS/NIST TESTS, 1-Nov-1998.
        /// </summary>
        [Test]

        public void XmlNotWfOP46fail1()
        {
            Assert.Throws<Exception>(() => { var document = @"<!DOCTYPE doc
[
<!ELEMENT doc ANY>
<!ELEMENT a (#EMPTY)>
]>
<doc/>".ToXmlDocument(); });
        }

        /// <summary>
        /// No inclusions (contrast with SGML). Here the section(s) 3.2 [46] apply.
        /// This test is taken from the collection OASIS/NIST TESTS, 1-Nov-1998.
        /// </summary>
        [Test]

        public void XmlNotWfOP46fail2()
        {
            Assert.Throws<Exception>(() => { var document = @"<!DOCTYPE doc
[
<!ELEMENT doc ANY>
<!ELEMENT a (#PCDATA) +(doc)>
]>
<doc/>".ToXmlDocument(); });
        }

        /// <summary>
        /// No exclusions (contrast with SGML). Here the section(s) 3.2 [46] apply.
        /// This test is taken from the collection OASIS/NIST TESTS, 1-Nov-1998.
        /// </summary>
        [Test]

        public void XmlNotWfOP46fail3()
        {
            Assert.Throws<Exception>(() => { var document = @"<!DOCTYPE doc
[
<!ELEMENT doc ANY>
<!ELEMENT a (#PCDATA) -(doc)>
]>
<doc/>".ToXmlDocument(); });
        }

        /// <summary>
        /// No space before occurrence. Here the section(s) 3.2 [46] apply.
        /// This test is taken from the collection OASIS/NIST TESTS, 1-Nov-1998.
        /// </summary>
        [Test]

        public void XmlNotWfOP46fail4()
        {
            Assert.Throws<Exception>(() => { var document = @"<!DOCTYPE doc
[
<!ELEMENT doc ANY>
<!ELEMENT a (doc) +>
]>
<doc/>".ToXmlDocument(); });
        }

        /// <summary>
        /// single group Here the section(s) 3.2 [46] apply.
        /// This test is taken from the collection OASIS/NIST TESTS, 1-Nov-1998.
        /// </summary>
        [Test]

        public void XmlNotWfOP46fail5()
        {
            Assert.Throws<Exception>(() => { var document = @"<!DOCTYPE doc
[
<!ELEMENT doc ANY>
<!ELEMENT a (#PCDATA)(doc)>
]>
<doc/>".ToXmlDocument(); });
        }

        /// <summary>
        /// can't be both declared and modeled Here the section(s) 3.2 [46] apply.
        /// This test is taken from the collection OASIS/NIST TESTS, 1-Nov-1998.
        /// </summary>
        [Test]

        public void XmlNotWfOP46fail6()
        {
            Assert.Throws<Exception>(() => { var document = @"<!DOCTYPE doc
[
<!ELEMENT doc ANY>
<!ELEMENT a EMPTY (doc)>
]>
<doc/>".ToXmlDocument(); });
        }

        /// <summary>
        /// Illegal comment in Empty element tag. Here the section(s) 3.1 [44] apply.
        /// This test is taken from the collection OASIS/NIST TESTS, 1-Nov-1998.
        /// </summary>
        [Test]

        public void XmlNotWfOP44fail3()
        {
            Assert.Throws<Exception>(() => { var document = @"<doc --bad comment--/>".ToXmlDocument(); });
        }

        /// <summary>
        /// Whitespace required between attributes. Here the section(s) 3.1 [44] apply.
        /// This test is taken from the collection OASIS/NIST TESTS, 1-Nov-1998.
        /// </summary>
        [Test]

        public void XmlNotWfOP44fail4()
        {
            Assert.Throws<Exception>(() => { var document = @"<doc att=""val""att2=""val2""/>".ToXmlDocument(); });
        }

        /// <summary>
        /// Duplicate attribute name is illegal. Here the section(s) 3.1 [44] apply.
        /// This test is taken from the collection OASIS/NIST TESTS, 1-Nov-1998.
        /// </summary>
        [Test]

        public void XmlNotWfOP44fail5()
        {
            Assert.Throws<Exception>(() => { var document = @"<doc att=""val"" att=""val""/>".ToXmlDocument(); });
        }

        /// <summary>
        /// SGML Unordered content models not allowed Here the section(s) 3.2.1 [47] apply.
        /// This test is taken from the collection Sun Microsystems XML Tests.
        /// </summary>
        [Test]

        public void XmlNotWfSgml13()
        {
            Assert.Throws<Exception>(() => { var document = @"<!DOCTYPE root [
    <!-- SGML-ism:  unordered content type -->
    <!ELEMENT ROOT (a & b & c)>
    <!ELEMENT a EMPTY>
    <!ELEMENT b EMPTY>
    <!ELEMENT c EMPTY>
]>

<root><b/><c/><a/></root>


".ToXmlDocument(); });
        }
        /// <summary>
        /// Invalid operator '|' must match previous operator ',' Here the section(s) 3.2.1 [47] apply.
        /// This test is taken from the collection OASIS/NIST TESTS, 1-Nov-1998.
        /// </summary>
        [Test]

        public void XmlNotWfOP47fail1()
        {
            Assert.Throws<Exception>(() => { var document = @"<!DOCTYPE doc
[
<!ELEMENT doc ANY>
<!ELEMENT a (doc,a?|a?)>
]>
<doc/>".ToXmlDocument(); });
        }

        /// <summary>
        /// Illegal character '-' in Element-content model Here the section(s) 3.2.1 [47] apply.
        /// This test is taken from the collection OASIS/NIST TESTS, 1-Nov-1998.
        /// </summary>
        [Test]

        public void XmlNotWfOP47fail2()
        {
            Assert.Throws<Exception>(() => { var document = @"<!DOCTYPE doc
[
<!ELEMENT doc ANY>
<!ELEMENT a (doc)->
]>
<doc/>".ToXmlDocument(); });
        }

        /// <summary>
        /// Optional character must follow a name or list Here the section(s) 3.2.1 [47] apply.
        /// This test is taken from the collection OASIS/NIST TESTS, 1-Nov-1998.
        /// </summary>
        [Test]

        public void XmlNotWfOP47fail3()
        {
            Assert.Throws<Exception>(() => { var document = @"<!DOCTYPE doc
[
<!ELEMENT doc ANY>
<!ELEMENT a *(doc)>
]>
<doc/>".ToXmlDocument(); });
        }

        /// <summary>
        /// Illegal space before optional character Here the section(s) 3.2.1 [47] apply.
        /// This test is taken from the collection OASIS/NIST TESTS, 1-Nov-1998.
        /// </summary>
        [Test]

        public void XmlNotWfOP47fail4()
        {
            Assert.Throws<Exception>(() => { var document = @"<!DOCTYPE doc
[
<!ELEMENT doc ANY>
<!ELEMENT a (doc) ?>
]>
<doc/>".ToXmlDocument(); });
        }

        /// <summary>
        /// No whitespace before "?" in content model Here the section(s) 3.2.1 [48] apply.
        /// This test is taken from the collection Sun Microsystems XML Tests.
        /// </summary>
        [Test]

        public void XmlNotWfContent01()
        {
            Assert.Throws<Exception>(() => { var document = @"<!DOCTYPE root [
    <!-- no whitespace before '?', '*', '+' -->
    <!ELEMENT root ((root) ?)>
]>
<root/>
".ToXmlDocument(); });
        }

        /// <summary>
        /// No whitespace before "*" in content model Here the section(s) 3.2.1 [48] apply.
        /// This test is taken from the collection Sun Microsystems XML Tests.
        /// </summary>
        [Test]

        public void XmlNotWfContent02()
        {
            Assert.Throws<Exception>(() => { var document = @"<!DOCTYPE root [
    <!-- no whitespace before '?', '*', '+' -->
    <!ELEMENT root ((root) *)>
]>
<root/>

".ToXmlDocument(); });
        }
        /// <summary>
        /// No whitespace before "+" in content model Here the section(s) 3.2.1 [48] apply.
        /// This test is taken from the collection Sun Microsystems XML Tests.
        /// </summary>
        [Test]

        public void XmlNotWfContent03()
        {
            Assert.Throws<Exception>(() => { var document = @"<!DOCTYPE root [
    <!-- no whitespace before '?', '*', '+' -->
    <!ELEMENT root (root +)>
]>
<root/>

".ToXmlDocument(); });
        }

        /// <summary>
        /// Illegal space before optional character Here the section(s) 3.2.1 [48] apply.
        /// This test is taken from the collection OASIS/NIST TESTS, 1-Nov-1998.
        /// </summary>
        [Test]

        public void XmlNotWfOP48fail1()
        {
            Assert.Throws<Exception>(() => { var document = @"<!DOCTYPE doc
[
<!ELEMENT doc ANY>
<!ELEMENT a (doc *)>
]>
<doc/>".ToXmlDocument(); });
        }

        /// <summary>
        /// Illegal space before optional character Here the section(s) 3.2.1 [48] apply.
        /// This test is taken from the collection OASIS/NIST TESTS, 1-Nov-1998.
        /// </summary>
        [Test]

        public void XmlNotWfOP48fail2()
        {
            Assert.Throws<Exception>(() => { var document = @"<!DOCTYPE doc
[
<!ELEMENT doc ANY>
<!ELEMENT a ((doc|a?) +)>
]>
<doc/>".ToXmlDocument(); });
        }

        /// <summary>
        /// connectors must match Here the section(s) 3.2.1 [49] apply.
        /// This test is taken from the collection OASIS/NIST TESTS, 1-Nov-1998.
        /// </summary>
        [Test]

        public void XmlNotWfOP49fail1()
        {
            Assert.Throws<Exception>(() => { var document = @"<!DOCTYPE doc
[
<!ELEMENT doc ANY>
<!ELEMENT a (doc|a?,a?)>
<doc/>".ToXmlDocument(); });
        }

        /// <summary>
        /// connectors must match Here the section(s) 3.2.1 [50] apply.
        /// This test is taken from the collection OASIS/NIST TESTS, 1-Nov-1998.
        /// </summary>
        [Test]

        public void XmlNotWfOP50fail1()
        {
            Assert.Throws<Exception>(() => { var document = @"<!DOCTYPE doc
[
<!ELEMENT doc ANY>
<!ELEMENT a (doc,a?|a?)>
<doc/>".ToXmlDocument(); });
        }

        /// <summary>
        /// occurrence on #PCDATA group must be * Here the section(s) 3.2.2 [51] apply.
        /// This test is taken from the collection OASIS/NIST TESTS, 1-Nov-1998.
        /// </summary>
        [Test]

        public void XmlNotWfOP51fail1()
        {
            Assert.Throws<Exception>(() => { var document = @"<!DOCTYPE doc
[
<!ELEMENT doc (#PCDATA)?>
]>
<doc/>".ToXmlDocument(); });
        }

        /// <summary>
        /// quote types must match. Here the section(s) 2.9 [32] apply. This test is
        /// taken from the collection OASIS/NIST TESTS, 1-Nov-1998.
        /// </summary>
        [Test]

        public void XmlNotWfOP32fail1()
        {
            Assert.Throws<Exception>(() => { var document = @"<?xml version=""1.0"" standalone='yes""?>
<doc/>
".ToXmlDocument(); });
        }
        /// <summary>
        /// quote types must match. Here the section(s) 2.9 [32] apply. This test is
        /// taken from the collection OASIS/NIST TESTS, 1-Nov-1998.
        /// </summary>
        [Test]

        public void XmlNotWfOP32fail2()
        {
            Assert.Throws<Exception>(() => { var document = @"<?xml version=""1.0"" standalone=""yes'?>
<doc/>
".ToXmlDocument(); });
        }
        /// <summary>
        /// initial S is required. Here the section(s) 2.9 [32] apply. This test is
        /// taken from the collection OASIS/NIST TESTS, 1-Nov-1998.
        /// </summary>
        [Test]

        public void XmlNotWfOP32fail3()
        {
            Assert.Throws<Exception>(() => { var document = @"<?xml version=""1.0""standalone=""yes""?>
<doc/>
".ToXmlDocument(); });
        }
        /// <summary>
        /// quotes are required. Here the section(s) 2.9 [32] apply. This test is taken
        /// from the collection OASIS/NIST TESTS, 1-Nov-1998.
        /// </summary>
        [Test]

        public void XmlNotWfOP32fail4()
        {
            Assert.Throws<Exception>(() => { var document = @"<?xml version=""1.0"" standalone=yes?>
<doc/>
".ToXmlDocument(); });
        }
        /// <summary>
        /// yes or no must be lower case. Here the section(s) 2.9 [32] apply. This test is
        /// taken from the collection OASIS/NIST TESTS, 1-Nov-1998.
        /// </summary>
        [Test]

        public void XmlNotWfOP32fail5()
        {
            Assert.Throws<Exception>(() => { var document = @"<?xml version=""1.0"" standalone=""YES""?>
<doc/>
".ToXmlDocument(); });
        }

        /// <summary>
        /// Whitespace required between attributes. Here the section(s) 3.1 [40] apply.
        /// This test is taken from the collection Sun Microsystems XML Tests.
        /// </summary>
        [Test]

        public void XmlNotWfAttlist10()
        {
            Assert.Throws<Exception>(() => { var document = @"<!DOCTYPE root [
<!ELEMENT root ANY>
<!ATTLIST root att1 CDATA #IMPLIED>
<!ATTLIST root att2 CDATA #IMPLIED>
]>
<root att1=""value1""att2=""value2"">
    <!-- whitespace required between attributes -->
</root>
".ToXmlDocument(); });
        }

        /// <summary>
        /// S is required between attributes. Here the section(s) 3.1 [40] apply. This
        /// test is taken from the collection OASIS/NIST TESTS, 1-Nov-1998.
        /// </summary>
        [Test]

        public void XmlNotWfOP40fail1()
        {
            Assert.Throws<Exception>(() => { var document = @"<doc att=""val""att2=""val2""></doc>".ToXmlDocument(); });
        }

        /// <summary>
        /// tags start with names, not nmtokens. Here the section(s) 3.1 [40] apply.
        /// This test is taken from the collection OASIS/NIST TESTS, 1-Nov-1998.
        /// </summary>
        [Test]

        public void XmlNotWfOP40fail2()
        {
            Assert.Throws<Exception>(() => { var document = @"<3notname></3notname>".ToXmlDocument(); });
        }

        /// <summary>
        /// tags start with names, not nmtokens. Here the section(s) 3.1 [40] apply.
        /// This test is taken from the collection OASIS/NIST TESTS, 1-Nov-1998.
        /// </summary>
        [Test]

        public void XmlNotWfOP40fail3()
        {
            Assert.Throws<Exception>(() => { var document = @"<3notname></notname>".ToXmlDocument(); });
        }

        /// <summary>
        /// no space before name. Here the section(s) 3.1 [40] apply.
        /// This test is taken from the collection OASIS/NIST TESTS, 1-Nov-1998.
        /// </summary>
        [Test]

        public void XmlNotWfOP40fail4()
        {
            Assert.Throws<Exception>(() => { var document = @"< doc></doc>".ToXmlDocument(); });
        }

        /// <summary>
        /// quotes are required (contrast with SGML) Here the section(s) 3.1 [41] apply.
        /// This test is taken from the collection OASIS/NIST TESTS, 1-Nov-1998.
        /// </summary>
        [Test]

        public void XmlNotWfOP41fail1()
        {
            Assert.Throws<Exception>(() => { var document = @"<!DOCTYPE doc
[
<!ELEMENT doc att (val|val2)>
]>
<doc att=val></doc>".ToXmlDocument(); });
        }
        /// <summary>
        /// attribute name is required (contrast with SGML). Here the section(s) 3.1 [41] apply.
        /// This test is taken from the collection OASIS/NIST TESTS, 1-Nov-1998.
        /// </summary>
        [Test]

        public void XmlNotWfOP41fail2()
        {
            Assert.Throws<Exception>(() => { var document = @"<!DOCTYPE doc
[
<!ELEMENT doc att (val|val2)>
]>
<doc val></doc>".ToXmlDocument(); });
        }
        /// <summary>
        /// Eq required. Here the section(s) 3.1 [41] apply. This test is taken from the
        /// collection OASIS/NIST TESTS, 1-Nov-1998.
        /// </summary>
        [Test]

        public void XmlNotWfOP41fail3()
        {
            Assert.Throws<Exception>(() => { var document = @"<doc att ""val""></doc>".ToXmlDocument(); });
        }

        /// <summary>
        /// EOF in middle of incomplete ETAG Here the section(s) 3.1 [42] apply.
        /// This test is taken from the collection Sun Microsystems XML Tests.
        /// </summary>
        [Test]

        public void XmlNotWfElement00()
        {
            Assert.Throws<Exception>(() => { var document = @"<root>
    Incomplete end tag.
</ro".ToXmlDocument(); });
        }

        /// <summary>
        /// EOF in middle of incomplete ETAG. Here the section(s) 3.1 [42] apply.
        /// This test is taken from the collection Sun Microsystems XML Tests.
        /// </summary>
        [Test]

        public void XmlNotWfElement01()
        {
            Assert.Throws<Exception>(() => { var document = @"<root>
    Incomplete end tag.
</root".ToXmlDocument(); });
        }

        /// <summary>
        /// no space before name. Here the section(s) 3.1 [42] apply. This
        /// test is taken from the collection OASIS/NIST TESTS, 1-Nov-1998.
        /// </summary>
        [Test]

        public void XmlNotWfOP42fail1()
        {
            Assert.Throws<Exception>(() => { var document = @"<doc></ doc>".ToXmlDocument(); });
        }

        /// <summary>
        /// cannot end with "/>". Here the section(s) 3.1 [42] apply. This test is
        /// taken from the collection OASIS/NIST TESTS, 1-Nov-1998.
        /// </summary>
        [Test]

        public void XmlNotWfOP42fail2()
        {
            Assert.Throws<Exception>(() => { var document = @"<doc></doc/>".ToXmlDocument(); });
        }

        /// <summary>
        /// no NET (contrast with SGML), Here the section(s) 3.1 [42] apply. This
        /// test is taken from the collection OASIS/NIST TESTS, 1-Nov-1998.
        /// </summary>
        [Test]

        public void XmlNotWfOP42fail3()
        {
            Assert.Throws<Exception>(() => { var document = @"<doc/doc/".ToXmlDocument(); });
        }

        /// <summary>
        /// Illegal markup (&lt;%@ ... %&gt;). Here the section(s) 3.1 [43] apply.
        /// This test is taken from the collection Sun Microsystems XML Tests.
        /// </summary>
        [Test]

        public void XmlNotWfElement02()
        {
            Assert.Throws<Exception>(() => { var document = @"<!DOCTYPE html [ <!ELEMENT html ANY> ]>
<html>
    <% @ LANGUAGE=""VBSCRIPT"" %>
</html>
".ToXmlDocument(); });
        }

        /// <summary>
        /// Illegal markup (&lt;% ... %&gt;). Here the section(s) 3.1 [43] apply.
        /// This test is taken from the collection Sun Microsystems XML Tests.
        /// </summary>
        [Test]

        public void XmlNotWfElement03()
        {
            Assert.Throws<Exception>(() => { var document = @"<!DOCTYPE html [ <!ELEMENT html ANY> ]>
<html>
    <% document.println (""hello, world"".ToXmlDocument(); }); %>
</html>

".ToXmlDocument(); });
        }

        /// <summary>
        /// Illegal markup (&lt;!ELEMENT ... >). Here the section(s) 3.1 [43] apply.
        /// This test is taken from the collection Sun Microsystems XML Tests.
        /// </summary>
        [Test]

        public void XmlNotWfElement04()
        {
            Assert.Throws<Exception>(() => { var document = @"<!DOCTYPE root [ <!ELEMENT root ANY> ]>
<root>
    <!ELEMENT foo EMPTY>
</root>
".ToXmlDocument(); });
        }

        /// <summary>
        /// no non-comment declarations. Here the section(s) 3.1 [43] apply.
        /// This test is taken from the collection OASIS/NIST TESTS, 1-Nov-1998.
        /// </summary>
        [Test]

        public void XmlNotWfOP43fail1()
        {
            Assert.Throws<Exception>(() => { var document = @"<!DOCTYPE elem
[
<!ELEMENT elem (#PCDATA|elem)*>
<!ENTITY ent ""<elem>CharData</elem>"">
]>
<elem>
<!ENTITY badent ""bad"">
</elem>
".ToXmlDocument(); });
        }

        /// <summary>
        /// no conditional sections. Here the section(s) 3.1 [43] apply.
        /// This test is taken from the collection OASIS/NIST TESTS, 1-Nov-1998.
        /// </summary>
        [Test]

        public void XmlNotWfOP43fail2()
        {
            Assert.Throws<Exception>(() => { var document = @"<!DOCTYPE elem
[
<!ELEMENT elem (#PCDATA|elem)*>
<!ENTITY ent ""<elem>CharData</elem>"">
]>
<elem>
<![IGNORE[This was valid in SGML, but not XML]]>
</elem>
".ToXmlDocument(); });
        }

        /// <summary>
        /// no conditional sections. Here the section(s) 3.1 [43] apply.
        /// This test is taken from the collection OASIS/NIST TESTS, 1-Nov-1998.
        /// </summary>
        [Test]

        public void XmlNotWfOP43fail3()
        {
            Assert.Throws<Exception>(() => { var document = @"<!DOCTYPE elem
[
<!ELEMENT elem (#PCDATA|elem)*>
<!ENTITY ent ""<elem>CharData</elem>"">
]>
<elem>
<![INCLUDE[This was valid in SGML, but not XML]]>
</elem>
".ToXmlDocument(); });
        }

        /// <summary>
        /// Whitespace required between attributes. Here the section(s) 3.1 [44] apply.
        /// This test is taken from the collection Sun Microsystems XML Tests.
        /// </summary>
        [Test]

        public void XmlNotWfAttlist11()
        {
            Assert.Throws<Exception>(() => { var document = @"<!DOCTYPE root [
<!ELEMENT root ANY>
<!ATTLIST root att1 CDATA #IMPLIED>
<!ATTLIST root att2 CDATA #IMPLIED>
]>
<root att1=""value1""att2=""value2""/>
    <!-- whitespace required between attributes -->
".ToXmlDocument(); });
        }

        /// <summary>
        /// Illegal space before Empty element tag. Here the section(s) 3.1 [44] apply.
        /// This test is taken from the collection OASIS/NIST TESTS, 1-Nov-1998.
        /// </summary>
        [Test]

        public void XmlNotWfOP44fail1()
        {
            Assert.Throws<Exception>(() => { var document = @"< doc/>".ToXmlDocument(); });
        }

        /// <summary>
        /// Illegal space after Empty element tag. Here the section(s) 3.1 [44] apply.
        /// This test is taken from the collection OASIS/NIST TESTS, 1-Nov-1998.
        /// </summary>
        [Test]

        public void XmlNotWfOP44fail2()
        {
            Assert.Throws<Exception>(() => { var document = @"<doc/ >".ToXmlDocument(); });
        }

        /// <summary>
        /// no S after "&lt;!". Here the section(s) 4.2 [71] apply. This test is
        /// taken from the collection OASIS/NIST TESTS, 1-Nov-1998.
        /// </summary>
        [Test]

        public void XmlNotWfOP71fail3()
        {
            Assert.Throws<Exception>(() => { var document = @"<!DOCTYPE doc
[
<!ELEMENT doc (#PCDATA)>
<! ENTITY ge ""replacement text"">
]>
<doc/>
".ToXmlDocument(); });
        }

        /// <summary>
        /// S is required after "&lt;!ENTITY". Here the section(s) 4.2 [71] apply.
        /// This test is taken from the collection OASIS/NIST TESTS, 1-Nov-1998.
        /// </summary>
        [Test]

        public void XmlNotWfOP71fail4()
        {
            Assert.Throws<Exception>(() => { var document = @"<!DOCTYPE doc
[
<!ELEMENT doc (#PCDATA)>
<!ENTITYge ""replacement text"">
]>
<doc/>
".ToXmlDocument(); });
        }

        /// <summary>
        /// S is required after "&lt;!ENTITY". Here the section(s) 4.2 [72] apply.
        /// This test is taken from the collection OASIS/NIST TESTS, 1-Nov-1998.
        /// </summary>
        [Test]

        public void XmlNotWfOP72fail1()
        {
            Assert.Throws<Exception>(() => { var document = @"<!DOCTYPE doc
[
<!ELEMENT doc (#PCDATA)>
<!ENTITY% pe ""<!--replacement decl-->"">
]>
<doc/>
".ToXmlDocument(); });
        }

        /// <summary>
        /// incomplete character reference. Here the section(s) 2.3 [9] apply.
        /// This test is taken from the collection OASIS/NIST TESTS, 1-Nov-1998.
        /// </summary>
        [Test]

        public void XmlNotWfOP09fail3()
        {
            Assert.Throws<Exception>(() => { var document = @"<!DOCTYPE doc
[
<!ELEMENT doc EMPTY>
<!ENTITY % ent1 ""asdf&#65"">
]>
<doc/>".ToXmlDocument(); });
        }

        /// <summary>
        /// quote types must match. Here the section(s) 2.3 [9] apply.
        /// This test is taken from the collection OASIS/NIST TESTS, 1-Nov-1998.
        /// </summary>
        [Test]

        public void XmlNotWfOP09fail4()
        {
            Assert.Throws<Exception>(() => { var document = @"<!DOCTYPE doc
[
<!ELEMENT doc EMPTY>
<!ENTITY % ent1 'a"">
]>
<doc/>".ToXmlDocument(); });
        }

        /// <summary>
        /// quote types must match. Here the section(s) 2.3 [9] apply.
        /// This test is taken from the collection OASIS/NIST TESTS, 1-Nov-1998.
        /// </summary>
        [Test]

        public void XmlNotWfOP09fail5()
        {
            Assert.Throws<Exception>(() => { var document = @"<!DOCTYPE doc
[
<!ELEMENT doc EMPTY>
<!ENTITY % ent1 ""a'>
]>
<doc/>".ToXmlDocument(); });
        }

        /// <summary>
        /// '&lt;' excluded. Here the section(s) 2.4 [14] apply. This
        /// test is taken from the collection OASIS/NIST TESTS, 1-Nov-1998.
        /// </summary>
        [Test]

        public void XmlNotWfOP14fail1()
        {
            Assert.Throws<Exception>(() => { var document = @"<doc>< </doc>
".ToXmlDocument(); });
        }

        /// <summary>
        /// '&' excluded. Here the section(s) 2.4 [14] apply. This test is
        /// taken from the collection OASIS/NIST TESTS, 1-Nov-1998.
        /// </summary>
        [Test]

        public void XmlNotWfOP14fail2()
        {
            Assert.Throws<Exception>(() => { var document = @"<doc>& </doc>
".ToXmlDocument(); });
        }

        /// <summary>
        /// "]]>" excluded. Here the section(s) 2.4 [14] apply. This test is
        /// taken from the collection OASIS/NIST TESTS, 1-Nov-1998.
        /// </summary>
        [Test]

        public void XmlNotWfOP14fail3()
        {
            Assert.Throws<Exception>(() => { var document = @"<doc>a]]>b</doc>
".ToXmlDocument(); });
        }

        /// <summary>
        /// Comments may not contain "--". Here the section(s) 2.5 [15] apply.
        /// This test is taken from the collection Sun Microsystems XML Tests.
        /// </summary>
        [Test]

        public void XmlNotWfSgml03()
        {
            Assert.Throws<Exception>(() => { var document = @"<!DOCTYPE root [ <!ELEMENT root EMPTY> ]>

    <!-- SGML-ism:  -- inside comment -->
<root/>
".ToXmlDocument(); });
        }

        /// <summary>
        /// comments can't end in '-'. Here the section(s) 2.5 [15] apply. This
        /// test is taken from the collection OASIS/NIST TESTS, 1-Nov-1998.
        /// </summary>
        [Test]

        public void XmlNotWfOP15fail1()
        {
            Assert.Throws<Exception>(() => { var document = @"<!--a--->
<doc/>".ToXmlDocument(); });
        }

        /// <summary>
        /// one comment per comment (contrasted with SGML). Here the section(s) 2.5 [15]
        /// apply. This test is taken from the collection OASIS/NIST TESTS, 1-Nov-1998.
        /// </summary>
        [Test]

        public void XmlNotWfOP15fail2()
        {
            Assert.Throws<Exception>(() => { var document = @"<!-- -- -- -->
<doc/>".ToXmlDocument(); });
        }

        /// <summary>
        /// can't include 2 or more adjacent '-'s. Here the section(s) 2.5 [15] apply.
        /// This test is taken from the collection OASIS/NIST TESTS, 1-Nov-1998.
        /// </summary>
        [Test]

        public void XmlNotWfOP15fail3()
        {
            Assert.Throws<Exception>(() => { var document = @"<!-- --- -->
<doc/>".ToXmlDocument(); });
        }

        /// <summary>
        /// No space between PI target name and data. Here the section(s) 2.6 [16] apply.
        /// This test is taken from the collection Sun Microsystems XML Tests.
        /// </summary>
        [Test]

        public void XmlNotWfPi()
        {
            Assert.Throws<Exception>(() => { var document = @"<!DOCTYPE root [
<!ELEMENT root EMPTY>
<!-- space before PI data and ?> -->
<?bad-pi+?>
]>
<root/>
".ToXmlDocument(); });
        }

        /// <summary>
        /// "xml" is an invalid PITarget. Here the section(s) 2.6 [16] apply. This test
        /// is taken from the collection OASIS/NIST TESTS, 1-Nov-1998.
        /// </summary>
        [Test]

        public void XmlNotWfOP16fail1()
        {
            Assert.Throws<Exception>(() => { var document = @"<?pitarget?>
<?xml?>
<doc/>".ToXmlDocument(); });
        }

        /// <summary>
        /// a PITarget must be present. Here the section(s) 2.6 [16] apply. This test
        /// is taken from the collection OASIS/NIST TESTS, 1-Nov-1998.
        /// </summary>
        [Test]

        public void XmlNotWfOP16fail2()
        {
            Assert.Throws<Exception>(() => { var document = @"<??>
<doc/>".ToXmlDocument(); });
        }

        /// <summary>
        /// no space before "CDATA". Here the section(s) 2.7 [18] apply. This test is
        /// taken from the collection OASIS/NIST TESTS, 1-Nov-1998.
        /// </summary>
        [Test]

        public void XmlNotWfOP18fail1()
        {
            Assert.Throws<Exception>(() => { var document = @"<doc><![ CDATA[a]]></doc>".ToXmlDocument(); });
        }

        /// <summary>
        /// no space after "CDATA". Here the section(s) 2.7 [18] apply. This test is
        /// taken from the collection OASIS/NIST TESTS, 1-Nov-1998.
        /// </summary>
        [Test]

        public void XmlNotWfOP18fail2()
        {
            Assert.Throws<Exception>(() => { var document = @"<doc><![CDATA [a]]></doc>".ToXmlDocument(); });
        }

        /// <summary>
        /// CDSect's can't nest. Here the section(s) 2.7 [18] apply. This test is
        /// taken from the collection OASIS/NIST TESTS, 1-Nov-1998.
        /// </summary>
        [Test]

        public void XmlNotWfOP18fail3()
        {
            Assert.Throws<Exception>(() => { var document = @"<doc>
<![CDATA[
<![CDATA[XML doesn't allow CDATA sections to nest]]>
]]>
</doc>".ToXmlDocument(); });
        }

        /// <summary>
        /// This refers to an undefined parameter entity reference within a markup declaration
        /// in the internal DTD subset, violating the PEs in Internal Subset WFC. Here the
        /// section(s) 2.8 apply. This test is taken from the collection James Clark XMLTEST
        /// cases, 18-Nov-1998.
        /// </summary>
        [Test]

        public void XmlNotWfValidSa094()
        {
            Assert.Throws<Exception>(() => { var document = @"<!DOCTYPE doc [
<!ENTITY % e ""foo"">
<!ELEMENT doc (#PCDATA)>
<!ATTLIST doc a1 CDATA ""%e;"">
]>
<doc></doc>
".ToXmlDocument(); });
        }

        /// <summary>
        /// XML declaration must be at the very beginning of a document; it"s not a
        /// processing instruction. Here the section(s) 2.8  apply. This test is
        /// taken from the collection Sun Microsystems XML Tests.
        /// </summary>
        [Test]

        public void XmlNotWfSgml02()
        {
            Assert.Throws<Exception>(() => { var document = @" <?xml version=""1.0""?>
    <!-- SGML-ism:  XML PI not at beginning -->
<!DOCTYPE root [ <!ELEMENT root EMPTY> ]>
<root/>
".ToXmlDocument(); });
        }

        /// <summary>
        /// prolog must start with XML decl. Here the section(s) 2.8 [22] apply.
        /// This test is taken from the collection OASIS/NIST TESTS, 1-Nov-1998.
        /// </summary>
        [Test]

        public void XmlNotWfOP22fail1()
        {
            Assert.Throws<Exception>(() => { var document = @"
<?xml version=""1.0""?>
<doc/>
".ToXmlDocument(); });
        }

        /// <summary>
        /// prolog must start with XML decl. Here the section(s) 2.8 [22] apply.
        /// This test is taken from the collection OASIS/NIST TESTS, 1-Nov-1998.
        /// </summary>
        [Test]

        public void XmlNotWfOP22fail2()
        {
            Assert.Throws<Exception>(() => { var document = @"<!DOCTYPE doc [
<!ELEMENT doc EMPTY>
]>
<?xml version=""1.0""?>
<doc/>
".ToXmlDocument(); });
        }

        /// <summary>
        /// "xml" must be lower-case. Here the section(s) 2.8 [23] apply. This test
        /// is taken from the collection OASIS/NIST TESTS, 1-Nov-1998.
        /// </summary>
        [Test]

        public void XmlNotWfOP23fail1()
        {
            Assert.Throws<Exception>(() => { var document = @"<?XML version=""1.0""?>
<doc/>
".ToXmlDocument(); });
        }

        /// <summary>
        /// VersionInfo must be supplied. Here the section(s) 2.8 [23] apply.
        /// This test is taken from the collection OASIS/NIST TESTS, 1-Nov-1998.
        /// </summary>
        [Test]

        public void XmlNotWfOP23fail2()
        {
            Assert.Throws<Exception>(() => { var document = @"<?xml encoding=""UTF-8""?>
<doc/>
".ToXmlDocument(); });
        }

        /// <summary>
        /// VersionInfo must come first. Here the section(s) 2.8 [23] apply. This
        /// test is taken from the collection OASIS/NIST TESTS, 1-Nov-1998.
        /// </summary>
        [Test]

        public void XmlNotWfOP23fail3()
        {
            Assert.Throws<Exception>(() => { var document = @"<?xml encoding=""UTF-8"" version=""1.0""?>
<doc/>
".ToXmlDocument(); });
        }

        /// <summary>
        /// SDDecl must come last. Here the section(s) 2.8 [23] apply. This test is
        /// taken from the collection OASIS/NIST TESTS, 1-Nov-1998.
        /// </summary>
        [Test]

        public void XmlNotWfOP23fail4()
        {
            Assert.Throws<Exception>(() => { var document = @"<?xml version=""1.0"" standalone=""yes"" encoding=""UTF-8""?>
<doc/>
".ToXmlDocument(); });
        }

        /// <summary>
        /// no SGML-type PIs. Here the section(s) 2.8 [23] apply. This test is taken
        /// from the collection OASIS/NIST TESTS, 1-Nov-1998.
        /// </summary>
        [Test]

        public void XmlNotWfOP23fail5()
        {
            Assert.Throws<Exception>(() => { var document = @"<?xml version=""1.0"">
<doc/>
".ToXmlDocument(); });
        }

        /// <summary>
        /// XML declarations must be correctly terminated. Here the section(s) 2.8 [23] apply.
        /// This test is taken from the collection OASIS/NIST TESTS, 1-Nov-1998.
        /// </summary>
        [Test]

        public void XmlNotWfOP39fail4()
        {
            Assert.Throws<Exception>(() => { var document = @"<?xml version=""1.0"">
".ToXmlDocument(); });
        }

        /// <summary>
        /// XML declarations must be correctly terminated. Here the section(s) 2.8 [23] apply.
        /// This test is taken from the collection OASIS/NIST TESTS, 1-Nov-1998.
        /// </summary>
        [Test]

        public void XmlNotWfOP39fail5()
        {
            Assert.Throws<Exception>(() => { var document = @"<?xml version=""1.0"">
<!DOCTYPE doc
[
<!ELEMENT doc EMPTY>
]>

<!--comment-->
<?pi?>
".ToXmlDocument(); });
        }

        /// <summary>
        /// quote types must match. Here the section(s) 2.8 [24] apply.
        /// This test is taken from the collection OASIS/NIST TESTS, 1-Nov-1998.
        /// </summary>
        [Test]

        public void XmlNotWfOP24fail1()
        {
            Assert.Throws<Exception>(() => { var document = @"<?xml version = '1.0""?>
<doc/>
".ToXmlDocument(); });
        }

        /// <summary>
        /// quote types must match. Here the section(s) 2.8 [24] apply. This test is
        /// taken from the collection OASIS/NIST TESTS, 1-Nov-1998.
        /// </summary>
        [Test]

        public void XmlNotWfOP24fail2()
        {
            Assert.Throws<Exception>(() => { var document = @"<?xml version = ""1.0'?>
<doc/>
".ToXmlDocument(); });
        }

        /// <summary>
        /// Comment is illegal in VersionInfo. Here the section(s) 2.8 [25] apply. This test
        /// is taken from the collection OASIS/NIST TESTS, 1-Nov-1998.
        /// </summary>
        [Test]

        public void XmlNotWfOP25fail1()
        {
            Assert.Throws<Exception>(() => { var document = @"<?xml version <!--bad comment--> =""1.0""?>
<doc/>
".ToXmlDocument(); });
        }

        /// <summary>
        /// Illegal character in VersionNum. Here the section(s) 2.8 [26] apply. This test
        /// is taken from the collection OASIS/NIST TESTS, 1-Nov-1998.
        /// </summary>
        [Test]

        public void XmlNotWfOP26fail1()
        {
            Assert.Throws<Exception>(() => { var document = @"<?xml version=""1.0?""?>
<doc/>
".ToXmlDocument(); });
        }

        /// <summary>
        /// Illegal character in VersionNum. Here the section(s) 2.8 [26] apply. This test is
        /// taken from the collection OASIS/NIST TESTS, 1-Nov-1998.
        /// </summary>
        [Test]

        public void XmlNotWfOP26fail2()
        {
            Assert.Throws<Exception>(() => { var document = @"<?xml version=""1.0^""?>
<doc/>
".ToXmlDocument(); });
        }

        /// <summary>
        /// References aren't allowed in Misc, even if they would resolve to valid Misc.
        /// Here the section(s) 2.8 [27] apply. This test is taken from the collection
        /// OASIS/NIST TESTS, 1-Nov-1998.
        /// </summary>
        [Test]

        public void XmlNotWfOP27fail1()
        {
            Assert.Throws<Exception>(() => { var document = @"<?xml version=""1.0""?>
&#32;
<doc/>
".ToXmlDocument(); });
        }

        /// <summary>
        /// only declarations in DTD. Here the section(s) 2.8 [28] apply. This test is
        /// taken from the collection OASIS/NIST TESTS, 1-Nov-1998.
        /// </summary>
        [Test]

        public void XmlNotWfOP28fail1()
        {
            Assert.Throws<Exception>(() => { var document = @"<!DOCTYPE doc [
<!ELEMENT doc EMPTY>
<doc/>
]>
".ToXmlDocument(); });
        }

        /// <summary>
        /// A processor must not pass unknown declaration types. Here the section(s) 2.8
        /// [29] apply. This test is taken from the collection OASIS/NIST TESTS, 1-Nov-1998.
        /// </summary>
        [Test]

        public void XmlNotWfOP29fail1()
        {
            Assert.Throws<Exception>(() => { var document = @"<!DOCTYPE doc [
<!ELEMENT doc EMPTY>
<!DUNNO should not pass unknown declaration types>
]>
<doc/>
".ToXmlDocument(); });
        }
    }
}
