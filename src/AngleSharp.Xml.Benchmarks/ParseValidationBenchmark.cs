namespace AngleSharp.Xml.Benchmarks
{
    using AngleSharp.Xml.Parser;
    using BenchmarkDotNet.Attributes;
    using System;
    using System.IO;

    /// <summary>
    /// Parsing with the validation walks that run after the tree is built: the identity walk runs
    /// on every document, and a DOCTYPE with declarations adds the DTD and the internal-subset
    /// walks. Validation is only reachable through parsing, so each row measures the whole parse;
    /// compare a row across builds, and against its <see cref="DoctypeKind.None"/> row for the
    /// share the DTD walks take.
    /// </summary>
    /// <remarks>
    /// The deep shapes stop at 2000 levels: before the walks used an explicit stack, parsing
    /// overflowed the call stack at about 2600 levels with a DTD and 2850 without (net8.0,
    /// 1 MB main thread stack), so 2000 is the deepest the old build can still be measured at.
    /// </remarks>
    [MemoryDiagnoser]
    public class ParseValidationBenchmark
    {
        private readonly XmlParser parser = CreateParser();
        private String source;
        private String externalSubsetPath;

        public enum DoctypeKind
        {
            None,
            Internal,
            External,
        }

        /// <summary>
        /// "catalog" is 1000 flat records and "catalog-N" N of them; "deep-N" is a chain N levels deep.
        /// The catalog sizes around 1000 tell a change in the work per record from a shift in
        /// when the garbage collector runs during a parse, which moves with the document size.
        /// </summary>
        [Params("catalog", "catalog-500", "catalog-2000", "deep-10", "deep-100", "deep-1000", "deep-2000")]
        public String Shape { get; set; }

        [ParamsAllValues]
        public DoctypeKind Doctype { get; set; }

        [GlobalSetup]
        public void GlobalSetup()
        {
            Console.WriteLine(Documents.LibraryMarker);
            var isCatalog = Shape.StartsWith("catalog", StringComparison.Ordinal);
            var root = isCatalog ? "catalog" : "a";
            var declarations = isCatalog ? Documents.CatalogDeclarations : Documents.DeepDeclarations;
            var doctype = String.Empty;

            if (Doctype == DoctypeKind.Internal)
            {
                doctype = Documents.InternalDoctype(root, declarations);
            }
            else if (Doctype == DoctypeKind.External)
            {
                externalSubsetPath = Path.Combine(Path.GetTempPath(), "anglesharp-xml-benchmark-" + Guid.NewGuid().ToString("N") + ".dtd");
                File.WriteAllText(externalSubsetPath, declarations);
                doctype = Documents.ExternalDoctype(root, externalSubsetPath);
            }

            source = isCatalog ?
                Documents.Catalog(doctype, Shape == "catalog" ? 1000 : Int32.Parse(Shape.Substring("catalog-".Length))) :
                Documents.Deep(doctype, Int32.Parse(Shape.Substring("deep-".Length)));

            // A document failing validation stops the walks at the first error, which would
            // measure an early exit instead of the walk
            if (!Parse())
            {
                throw new InvalidOperationException($"The {Shape} document with {Doctype} DOCTYPE is not valid.");
            }

            // A DTD that was not applied leaves every document valid, so a document breaking it
            // must come out invalid: the root's model and the required ID both reject the element
            if (Doctype != DoctypeKind.None && parser.ParseDocument($"{doctype}<{root}><undeclared/></{root}>").IsValid)
            {
                throw new InvalidOperationException($"The {Doctype} DTD of the {Shape} document was not applied.");
            }
        }

        [GlobalCleanup]
        public void GlobalCleanup()
        {
            if (externalSubsetPath != null)
            {
                File.Delete(externalSubsetPath);
            }
        }

        [Benchmark]
        public Boolean Parse() => parser.ParseDocument(source).IsValid;

        /// <summary>
        /// A parser without a depth limit, so the deep shapes measure their full depth, in a
        /// browsing context that loads the External rows' DTD from the local file. Builds with
        /// XmlParserOptions.MaximumDepth stop at 512 levels by default; older builds have no
        /// limit and lack the option, so it is set when present. Builds that fetch the external
        /// subset through the resource loader need resource loading and a requester for the
        /// DTD's path; older builds read the file themselves and ignore both.
        /// </summary>
        private static XmlParser CreateParser()
        {
            Object options = new XmlParserOptions();
            typeof(XmlParserOptions).GetProperty("MaximumDepth")?.SetValue(options, Int32.MaxValue);
            var configuration = Configuration.Default
                .WithXml()
                .With(new LocalFileRequester())
                .WithDefaultLoader(new Io.LoaderOptions { IsResourceLoadingEnabled = true });
            return new XmlParser((XmlParserOptions)options, BrowsingContext.New(configuration));
        }
    }
}
