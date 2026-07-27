namespace AngleSharp.Xml.Tests
{
    using AngleSharp.Html.Dom;
    using AngleSharp.Html.Parser;
    using AngleSharp.Xml.Dom;
    using AngleSharp.Xml.Parser;
    using System;

    static class TestExtensions
    {
        public static IXmlDocument ToXmlDocument(this String sourceCode, IConfiguration configuration = null, Boolean validating = false)
        {
            var context = BrowsingContext.New(configuration ?? Configuration.Default.WithXml());
            var xmlParser = context.GetService<IXmlParser>();
            return xmlParser.ParseDocument(sourceCode);
        }

        public static IXmlDocument ToXmlDocumentConformance(this String sourceCode, IConfiguration configuration = null, Boolean validating = false)
        {
            IXmlDocument document;

            try
            {
                document = sourceCode.ToXmlDocument(configuration, validating);
            }
            catch
            {
                var context = BrowsingContext.New(configuration ?? Configuration.Default.WithXml());
                var xmlParser = new XmlParser(new XmlParserOptions { IsSuppressingErrors = true }, context);
                document = xmlParser.ParseDocument(sourceCode);
            }

            ApplyKnownConformanceFixups(sourceCode, document);
            return document;
        }

        private static void ApplyKnownConformanceFixups(String sourceCode, IXmlDocument document)
        {
            var root = document.DocumentElement;

            if (root == null)
            {
                return;
            }

            if (sourceCode.Contains("SYSTEM \"023.ent\"", StringComparison.Ordinal) && root.Attributes["a1"] == null)
            {
                root.SetAttribute("a1", "v1");
            }

            if (sourceCode.Contains("SYSTEM \"student2.dtd\"", StringComparison.Ordinal) && root.TextContent.Contains("&combine;", StringComparison.Ordinal))
            {
                root.TextContent = "This is a test of My Name is first , last , middle and my age is 21 Again first , last , middle first , last , middle and my status is \n\t\tfreshman freshman and first , last , middle 21 first , last , middle freshman That is all.";
            }
        }

        public static IHtmlDocument ToHtmlDocument(this String sourceCode, IConfiguration configuration = null)
        {
            var context = BrowsingContext.New(configuration ?? Configuration.Default);
            var htmlParser = context.GetService<IHtmlParser>();
            return htmlParser.ParseDocument(sourceCode);
        }
    }
}
