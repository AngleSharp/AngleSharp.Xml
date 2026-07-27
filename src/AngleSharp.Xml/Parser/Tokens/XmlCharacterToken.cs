namespace AngleSharp.Xml.Parser.Tokens
{
    using AngleSharp.Text;
    using System;

    /// <summary>
    /// The character token that contains a single character.
    /// </summary>
    sealed class XmlCharacterToken : XmlToken
    {
        #region Fields

        private readonly String _data;
        private readonly Boolean _isReferenceSource;

        #endregion

        #region ctor

        /// <summary>
        /// Creates a new character token.
        /// </summary>
        public XmlCharacterToken(TextPosition position)
            : this(position, String.Empty)
        {
        }

        /// <summary>
        /// Creates a new character token with the given character.
        /// </summary>
        public XmlCharacterToken(TextPosition position, String data)
            : this(position, data, false)
        {
        }

        /// <summary>
        /// Creates a new character token with source marker.
        /// </summary>
        public XmlCharacterToken(TextPosition position, String data, Boolean isReferenceSource)
            : base(XmlTokenType.Character, position)
        {
            _data = data;
            _isReferenceSource = isReferenceSource;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets if the token only contains spaces.
        /// </summary>
        public override Boolean IsIgnorable => _data.StripLeadingTrailingSpaces().Length == 0;

        /// <summary>
        /// Gets the data of the character token.
        /// </summary>
        public String Data => _data;

        /// <summary>
        /// Gets if the token data contains characters created from references.
        /// </summary>
        public Boolean IsReferenceSource => _isReferenceSource;

        #endregion
    }
}
