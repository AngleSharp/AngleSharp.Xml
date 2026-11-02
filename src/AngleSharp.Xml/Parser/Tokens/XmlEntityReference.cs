namespace AngleSharp.Xml.Parser.Tokens
{
    using AngleSharp.Text;
    using System;

    /// <summary>
    /// A reference to a DTD-declared entity (or an unknown one) that a token's text keeps as
    /// written, to be resolved once the parse has the complete DTD.
    /// </summary>
    sealed class XmlEntityReference
    {
        public XmlEntityReference(Int32 offset, String name, TextPosition position)
        {
            Offset = offset;
            Name = name;
            Position = position;
        }

        /// <summary>
        /// Gets the offset of the reference's ampersand in the token's text.
        /// </summary>
        public Int32 Offset { get; }

        /// <summary>
        /// Gets the entity's name, without the ampersand and the semicolon.
        /// </summary>
        public String Name { get; }

        /// <summary>
        /// Gets the length of the reference in the text, ampersand and semicolon included.
        /// </summary>
        public Int32 Length => Name.Length + 2;

        /// <summary>
        /// Gets the position to report an error about the reference at.
        /// </summary>
        public TextPosition Position { get; }
    }
}
