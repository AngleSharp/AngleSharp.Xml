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

        public static IHtmlDocument ToHtmlDocument(this String sourceCode, IConfiguration configuration = null)
        {
            var context = BrowsingContext.New(configuration ?? Configuration.Default);
            var htmlParser = context.GetService<IHtmlParser>();
            return htmlParser.ParseDocument(sourceCode);
        }
    }
}
