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

        private Int64? _maxCharactersFromEntities;

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
