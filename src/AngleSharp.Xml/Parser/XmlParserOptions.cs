namespace AngleSharp.Xml.Parser
{
    using AngleSharp.Dom;
    using AngleSharp.Text;
    using System;

    /// <summary>
    /// Contains a number of options for the XML parser.
    /// </summary>
    public struct XmlParserOptions
    {
        /// <summary>
        /// The limit <see cref="MaxCharactersFromEntities"/> has unless it is set.
        /// </summary>
        public const Int64 DefaultMaxCharactersFromEntities = 10_000_000;

        /// <summary>
        /// The element nesting depth used when <see cref="MaximumDepth"/> is not set, as
        /// AngleSharp.Html.Parser.HtmlParserOptions.DefaultMaximumDepth.
        /// </summary>
        public static readonly Int32 DefaultMaximumDepth = 512;

        private Int64? _maxCharactersFromEntities;
        private Int32 _maximumDepth;

        /// <summary>
        /// Gets or sets the deepest level at which the parser nests elements, counting the root
        /// element as level 1, as AngleSharp.Html.Parser.HtmlParserOptions.MaximumDepth does.
        /// Defaults to <see cref="DefaultMaximumDepth"/>; zero or a negative value restores the
        /// default and <see cref="Int32.MaxValue"/> lifts the limit.
        /// </summary>
        /// <remarks>
        /// XML puts no limit on nesting, but code walking a tree that is thousands of levels deep
        /// recursively can overflow the call stack, which ends the process. An element beyond the
        /// limit throws an <see cref="XmlParseException"/>, since nesting it elsewhere would change
        /// the document; when errors are suppressed, it is attached at the limit instead, as a
        /// sibling of the deepest element, as the HTML parser does.
        /// </remarks>
        public Int32 MaximumDepth
        {
            readonly get => _maximumDepth > 0 ? _maximumDepth : DefaultMaximumDepth;
            set => _maximumDepth = value;
        }

        /// <summary>
        /// Gets or sets the maximum number of characters that the expansion of entities declared
        /// in the document's DTD may produce in total, as
        /// System.Xml.XmlReaderSettings.MaxCharactersFromEntities does. Zero (0) means no limit.
        /// Unlike System.Xml, which prohibits DTD processing by default, this parser always
        /// processes the DTD, so the limit defaults to
        /// <see cref="DefaultMaxCharactersFromEntities"/> rather than to no limit, which guards
        /// against entities that expand exponentially ("billion laughs").
        /// Exceeding it throws an <see cref="XmlParseException"/>; when errors are suppressed,
        /// the remaining entity references are kept as text instead.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">The value is negative.</exception>
        public Int64 MaxCharactersFromEntities
        {
            get => _maxCharactersFromEntities ?? DefaultMaxCharactersFromEntities;
            set => _maxCharactersFromEntities = value >= 0 ? value : throw new ArgumentOutOfRangeException(nameof(value), value, "The limit must not be negative.");
        }

        /// <summary>
        /// Gets or sets if errors should be surpressed. This way the document
        /// will be parsed for sure, but may look weird.
        /// </summary>
        public Boolean IsSuppressingErrors 
        { 
            get; 
            set;
        }

        /// <summary>
        /// Gets or sets if references to the original source document
        /// should be kept on the elements in form of their tokens.
        /// </summary>
        public Boolean IsKeepingSourceReferences
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the callback once a new element was created.
        /// </summary>
        public Action<IElement, TextPosition> OnCreated
        {
            get;
            set;
        }
    }
}
