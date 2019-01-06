namespace AngleSharp.Xml.Tests
{
    using AngleSharp.Dom;
    using AngleSharp.Html.Parser;
    using AngleSharp.Xml.Parser;
    using System;

    static class TestExtensions
    {
        public static IDocument ToXmlDocument(this String sourceCode, IConfiguration configuration = null)
        {
            var context = BrowsingContext.New(configuration);
            var xmlParser = context.GetService<IXmlParser>();
            return xmlParser.ParseDocument(sourceCode);
        }

        public static IDocument ToHtmlDocument(this String sourceCode, IConfiguration configuration = null)
        {
            var context = BrowsingContext.New(configuration ?? Configuration.Default);
            var htmlParser = context.GetService<IHtmlParser>();
            return htmlParser.ParseDocument(sourceCode);
        }
    }
}
