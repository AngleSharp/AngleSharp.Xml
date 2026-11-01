namespace AngleSharp.Xml.Benchmarks
{
    using AngleSharp.Dom;
    using AngleSharp.Xml.Dom;
    using AngleSharp.Xml.Parser;
    using BenchmarkDotNet.Attributes;
    using System;
    using System.Globalization;

    /// <summary>
    /// <see cref="INode.TextContent"/> of an XML element, which collects text and CDATA from the
    /// whole subtree. The common calls are on small elements, so "leaf" and "item" matter as
    /// much as the deep shapes.
    /// </summary>
    /// <remarks>
    /// The deep trees are built through the DOM rather than parsed: before the walks used an
    /// explicit stack, parsing overflowed at about 2600 levels, while TextContent itself held
    /// until about 5000 (net8.0, 1 MB main thread stack). 4000 is the deepest the old build
    /// can still be measured at.
    /// </remarks>
    [MemoryDiagnoser]
    public class TextContentBenchmark
    {
        private IElement element;

        /// <summary>
        /// "leaf" is an element holding one text; "item" is a record of three such elements with
        /// a CDATA section and a comment; "catalog" is 1000 records; "deep-N" is a chain N levels
        /// deep with a text, a CDATA section and an empty sibling element on every level.
        /// </summary>
        [Params("leaf", "item", "catalog", "deep-10", "deep-100", "deep-1000", "deep-4000")]
        public String Shape { get; set; }

        [GlobalSetup]
        public void GlobalSetup()
        {
            Console.WriteLine(Documents.LibraryMarker);

            if (Shape.StartsWith("deep-", StringComparison.Ordinal))
            {
                var depth = Int32.Parse(Shape.Substring("deep-".Length), CultureInfo.InvariantCulture);
                element = BuildDeep(depth);

                // Every level contributes its text and its CDATA section
                if (TextContent() != 2 * depth)
                {
                    throw new InvalidOperationException($"The {Shape} tree does not hold the expected text.");
                }
            }
            else
            {
                var document = new XmlParser().ParseDocument(Documents.Catalog(String.Empty, 1000));
                var catalog = document.DocumentElement;
                var item = catalog.FirstElementChild;
                element = Shape switch
                {
                    "leaf" => item.FirstElementChild,
                    "item" => item,
                    _ => catalog,
                };
            }
        }

        [Benchmark]
        public Int32 TextContent() => element.TextContent.Length;

        private static IElement BuildDeep(Int32 depth)
        {
            var document = new XmlParser().ParseDocument("<a><b/></a>");
            var root = document.DocumentElement;
            var levelTemplate = (IElement)root.Clone(false);
            var siblingTemplate = (IElement)root.FirstElementChild.Clone(false);
            var current = root;
            current.RemoveChild(current.FirstChild);

            for (var i = 0; i < depth; i++)
            {
                current.AppendChild(document.CreateTextNode("t"));
                current.AppendChild(document.CreateCDataSection("c"));
                current.AppendChild(siblingTemplate.Clone(false));

                if (i < depth - 1)
                {
                    var next = (IElement)levelTemplate.Clone(false);
                    current.AppendChild(next);
                    current = next;
                }
            }

            return root;
        }
    }
}
