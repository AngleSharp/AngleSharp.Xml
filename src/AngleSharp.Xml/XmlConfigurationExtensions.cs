namespace AngleSharp
{
    using AngleSharp.Browser;
    using AngleSharp.Dom;
    using AngleSharp.Html.Dom;
    using AngleSharp.Io;
    using AngleSharp.Scripting;
    using AngleSharp.Svg.Dom;
    using AngleSharp.Text;
    using AngleSharp.Xml.Dom;
    using AngleSharp.Xml.Parser;
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// XML related extensions for the configuration.
    /// </summary>
    public static class XmlConfigurationExtensions
    {
        /// <summary>
        /// Adds XML capabilities to the configuration.
        /// </summary>
        /// <param name="configuration">The configuration to extend.</param>
        /// <returns>The new configuration.</returns>
        public static IConfiguration WithXml(this IConfiguration configuration)
        {
            var documentFactory = configuration.Services.OfType<DefaultDocumentFactory>().FirstOrDefault();

            if (documentFactory != null)
            {
                documentFactory.Unregister(MimeTypeNames.Xml);
                documentFactory.Unregister(MimeTypeNames.ApplicationXml);
                documentFactory.Unregister(MimeTypeNames.Svg);
                documentFactory.Register(MimeTypeNames.Xml, LoadXmlAsync);
                documentFactory.Register(MimeTypeNames.ApplicationXml, LoadXmlAsync);
                documentFactory.Register(MimeTypeNames.Svg, LoadSvgAsync);
            }

            // AngleSharp.Js exposes the DOM extensions of the libraries it finds services of, but
            // only sees the parser once it was created (e.g. createDocument would be missing)
            if (!configuration.Services.OfType<XmlLibrary>().Any())
            {
                configuration = configuration.With(XmlLibrary.Instance);
            }

            return configuration.WithOnly<IXmlParser>(ctx => new XmlParser(ctx));
        }

        /// <summary>
        /// Marks a configuration as using this library.
        /// </summary>
        private sealed class XmlLibrary
        {
            public static readonly XmlLibrary Instance = new XmlLibrary();
        }

        private static Task<IDocument> LoadXmlAsync(IBrowsingContext context, CreateDocumentOptions options, CancellationToken cancellationToken)
        {
            var document = new XmlDocument(context, options.Source);
            return LoadAsync(context, document, options, cancellationToken);
        }

        private static Task<IDocument> LoadSvgAsync(IBrowsingContext context, CreateDocumentOptions options, CancellationToken cancellationToken)
        {
            var document = new SvgDocument(context, options.Source);
            return LoadAsync(context, document, options, cancellationToken);
        }

        private static async Task<IDocument> LoadAsync(IBrowsingContext context, Document document, CreateDocumentOptions options, CancellationToken cancellationToken)
        {
            var parser = context.GetService<IXmlParser>() ?? throw new InvalidOperationException("The IXmlParser service has been removed. Cannot continue.");
            document.Setup(options.Response, options.ContentType, options.ImportAncestor);
            context.NavigateTo(document);

            try
            {
                await parser.ParseDocumentAsync(document, cancellationToken).ConfigureAwait(false);
            }
            catch (XmlParseException ex) when (!cancellationToken.IsCancellationRequested)
            {
                // like a browser, a document that isn't well-formed is shown as the error (and
                // its scripts don't run), but it is a document still, which loads
                ShowParseError(document, ex);
                return document;
            }

            await RunScriptsAsync(context, document, cancellationToken).ConfigureAwait(false);
            return document;
        }

        private static void ShowParseError(Document document, XmlParseException error)
        {
            while (document.HasChildNodes)
            {
                document.RemoveChild(((INode)document).FirstChild);
            }

            var root = document.CreateElement(ParserErrorNamespace, "parsererror");
            root.TextContent = error.Message;
            document.AppendChild(root);
        }

        /// <summary>
        /// Runs the (inline) scripts of the XHTML elements of a document that was navigated to,
        /// in tree order once it has been parsed.
        /// </summary>
        private static async Task RunScriptsAsync(IBrowsingContext context, Document document, CancellationToken cancellationToken)
        {
            if (!context.IsScripting())
            {
                return;
            }

            var loop = context.GetService<IEventLoop>();
            // the parser creates its own elements, so they are found by their name
            var scripts = document.Descendants<IElement>()
                .Where(m => m.LocalName.Is("script") && m.NamespaceUri.Is(NamespaceNames.HtmlUri) && !m.HasAttribute("src"))
                .ToArray();

            foreach (var script in scripts)
            {
                var type = script.GetAttribute("type");
                var service = context.GetScripting(String.IsNullOrEmpty(type) ? MimeTypeNames.DefaultJavaScript : type);

                if (service is null || script.Parent is null)
                {
                    continue;
                }

                var response = VirtualResponse.Create(r => r.Address(document.DocumentUri).Content(script.TextContent ?? String.Empty));

                try
                {
                    await service.EvaluateScriptAsync(response, new ScriptOptions(document, loop) { Element = script as IHtmlScriptElement }, cancellationToken).ConfigureAwait(false);
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    // a failing script doesn't stop the others
                    System.Diagnostics.Debug.WriteLine(ex);
                }
            }
        }

        private static readonly String ParserErrorNamespace = "http://www.mozilla.org/newlayout/xml/parsererror.xml";
    }
}
