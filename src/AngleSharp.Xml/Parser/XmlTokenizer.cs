namespace AngleSharp.Xml.Parser
{
    using AngleSharp.Common;
    using AngleSharp.Dom;
    using AngleSharp.Text;
    using AngleSharp.Xml.Parser.Tokens;
    using System;
    using System.Collections.Generic;
    using System.Text;

    /// <summary>
    /// Performs the tokenization of the source code. Most of
    /// the information is taken from http://www.w3.org/TR/REC-xml/.
    /// </summary>
    sealed class XmlTokenizer : BaseTokenizer
    {
        #region Fields

        private readonly IEntityProvider _resolver;
        private List<XmlEntityReference> _entityReferences;
        private TextPosition _position;
        private Int64 _charactersFromEntities;

        #endregion

        #region ctor

        /// <summary>
        /// Creates a new tokenizer for XML documents.
        /// </summary>
        /// <param name="source">The source code manager.</param>
        /// <param name="resolver">The entity resolver to use.</param>
        public XmlTokenizer(TextSource source, IEntityProvider resolver)
            : base(source)
        {
            _resolver = resolver;

        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets if some errors should be suppressed.
        /// </summary>
        public Boolean IsSuppressingErrors
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the maximum number of characters the expansion of declared entities may
        /// produce in total; 0 means no limit.
        /// </summary>
        public Int64 MaxCharactersFromEntities
        {
            get;
            set;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets the next available token.
        /// </summary>
        /// <returns>The next available token.</returns>
        public XmlToken Get()
        {
            var current = GetNext();

            if (current != Symbols.EndOfFile)
            {
                _position = GetCurrentPosition();
                return Data(current);
            }

            return NewEof();
        }


        /// <summary>
        /// Replaces each CDATA section in an entity's replacement text by its content, as the text
        /// the entity contributes to element content. Equivalent to replacing
        /// <c>&lt;!\[CDATA\[(.*?)\]\]&gt;</c> by its group, in a single pass.
        /// </summary>
        /// <param name="value">The replacement text.</param>
        /// <returns>The text with the CDATA sections unwrapped.</returns>
        internal static String UnwrapCDataSections(String value)
        {
            const String start = "<![CDATA[";
            const String end = "]]>";
            var sectionStart = value.IndexOf(start, StringComparison.Ordinal);

            if (sectionStart < 0)
            {
                return value;
            }

            var result = new StringBuilder(value.Length);
            var position = 0;

            while (sectionStart >= 0)
            {
                var contentStart = sectionStart + start.Length;
                var sectionEnd = value.IndexOf(end, contentStart, StringComparison.Ordinal);

                // Without a terminator after it, no later start has one either
                if (sectionEnd < 0)
                {
                    break;
                }

                result.Append(value, position, sectionStart - position);
                result.Append(value, contentStart, sectionEnd - contentStart);
                position = sectionEnd + end.Length;
                sectionStart = value.IndexOf(start, position, StringComparison.Ordinal);
            }

            return result.Append(value, position, value.Length - position).ToString();
        }

        /// <summary>
        /// Counts the characters one expansion of a declared entity produces against
        /// <see cref="MaxCharactersFromEntities"/>, before the expansion is made.
        /// </summary>
        /// <param name="count">The number of characters the expansion produces.</param>
        /// <param name="position">The position to report the error at.</param>
        /// <returns>
        /// True if the expansion stays within the limit; false if it would exceed it and errors
        /// are suppressed, so the reference should be kept as text.
        /// </returns>
        public Boolean ClaimEntityCharacters(Int32 count, TextPosition position)
        {
            if (MaxCharactersFromEntities > 0)
            {
                if (count > MaxCharactersFromEntities - _charactersFromEntities)
                {
                    if (!IsSuppressingErrors)
                    {
                        throw XmlParseError.EntityExpansionLimitExceeded.At(position);
                    }

                    return false;
                }

                _charactersFromEntities += count;
            }

            return true;
        }

        #endregion

        #region General

        /// <summary>
        /// More http://www.w3.org/TR/REC-xml/#sec-logical-struct.
        /// </summary>
        /// <param name="c">The next input character.</param>
        private XmlToken Data(Char c)
        {
            switch (c)
            {
                case Symbols.LessThan:
                    return TagOpen();

                case Symbols.EndOfFile:
                    return NewEof();

                default:
                    return DataText(c);
            }
        }

        private XmlToken DataText(Char c)
        {
            var hasCharacterReference = false;
            _entityReferences = null;

            while (true)
            {
                switch (c)
                {
                    case Symbols.LessThan:
                    case Symbols.EndOfFile:
                        Back();
                        return NewCharacters(hasCharacterReference);

                    case Symbols.Ampersand:
                        hasCharacterReference = true;
                        StringBuffer.Append(CharacterReference());
                        c = GetNext();
                        break;

                    case Symbols.SquareBracketClose:
                        StringBuffer.Append(c);
                        c = CheckNextCharacter();
                        break;

                    default:
                        StringBuffer.Append(c);
                        c = GetNext();
                        break;
                }
            }
        }

        #endregion

        #region CDATA

        /// <summary>
        /// Checks if the character sequence is equal to ]]&gt;.
        /// </summary>
        /// <returns>The given character.</returns>
        private Char CheckNextCharacter()
        {
            var c = GetNext();

            if (c == Symbols.SquareBracketClose)
            {
                if (GetNext() == Symbols.GreaterThan && !IsSuppressingErrors)
                {
                    throw XmlParseError.XmlInvalidCharData.At(GetCurrentPosition());
                }

                Back();
            }

            return c;
        }

        /// <summary>
        /// See http://www.w3.org/TR/REC-xml/#NT-CData.
        /// </summary>
        private XmlCDataToken CData()
        {
            var c = GetNext();

            while (true)
            {
                if (c == Symbols.EndOfFile)
                {
                    if (IsSuppressingErrors)
                    {
                        break;
                    }
                    else
                    {
                        throw XmlParseError.EOF.At(GetCurrentPosition());
                    }
                }

                if (c == Symbols.SquareBracketClose && ContinuesWithSensitive("]]>"))
                {
                    Advance(2);
                    break;
                }

                StringBuffer.Append(c);
                c = GetNext();
            }

            return NewCharacterData();
        }

        /// <summary>
        /// Called once an &amp; character is being seen.
        /// </summary>
        /// <returns>The entity token.</returns>
        private String CharacterReference()
        {
            var c = GetNext();
            var start = StringBuffer.Length;
            var hex = false;
            var numeric = c == Symbols.Num;

            if (numeric)
            {
                c = GetNext();
                hex = c == 'x' || c == 'X';

                if (hex)
                {
                    c = GetNext();

                    while (c.IsHex())
                    {
                        StringBuffer.Append(c);
                        c = GetNext();
                    }
                }
                else
                {
                    while (c.IsDigit())
                    {
                        StringBuffer.Append(c);
                        c = GetNext();
                    }
                }
            }
            else if (c.IsXmlNameStart())
            {
                do
                {
                    StringBuffer.Append(c);
                    c = GetNext();
                }
                while (c.IsXmlName());
            }

            var length = StringBuffer.Length - start;

            if (c == Symbols.Semicolon && length > 0)
            {
                if (numeric)
                {
                    var content = StringBuffer.ToString(start, length);
                    var number = hex ? content.FromHex() : content.FromDec();

                    if (number.IsValidAsCharRef())
                    {
                        StringBuffer.Remove(start, length);
                        return Char.ConvertFromUtf32(number);
                    }

                    StringBuffer.Append(c);
                }
                else
                {
                    var content = StringBuffer.Append(c).ToString(start, ++length);
                    var entity = _resolver.GetSymbol(content);

                    if (!String.IsNullOrEmpty(entity))
                    {
                        StringBuffer.Remove(start, length);
                        return entity;
                    }

                    // Any other entity is declared in the DTD or unknown, which is only decided
                    // at the end of the parse, once the whole DTD is there: the reference stays in
                    // the text as written, and its offset is kept for the builder to resolve it
                    StringBuffer.Insert(start, Symbols.Ampersand);
                    _entityReferences = _entityReferences ?? new List<XmlEntityReference>();
                    _entityReferences.Add(new XmlEntityReference(start, content.Substring(0, content.Length - 1), _position));
                    return String.Empty;
                }

                if (!IsSuppressingErrors)
                {
                    throw XmlParseError.CharacterReferenceInvalidCode.At(_position);
                }
            }

            if (!IsSuppressingErrors)
            {
                throw XmlParseError.CharacterReferenceNotTerminated.At(GetCurrentPosition());
            }

            if (c == Symbols.DoubleQuote)
            {
                Back();
            }

            StringBuffer.Insert(start++, Symbols.Ampersand);

            if (numeric)
            {
                StringBuffer.Insert(start++, Symbols.Num);
            }

            if (hex)
            {
                StringBuffer.Insert(start++, 'x');
            }

            return String.Empty;
        }

        #endregion

        #region Tags

        /// <summary>
        /// More http://www.w3.org/TR/REC-xml/#sec-starttags.
        /// </summary>
        private XmlToken TagOpen()
        {
            var c = GetNext();

            if (c == Symbols.ExclamationMark)
            {
                return MarkupDeclaration();
            }
            else if (c == Symbols.QuestionMark)
            {
                c = GetNext();

                if (ContinuesWithSensitive(TagNames.Xml))
                {
                    Advance(2);
                    return DeclarationStart();
                }

                return ProcessingStart(c);
            }
            else if (c == Symbols.Solidus)
            {
                return TagEnd();
            }
            else if (c.IsXmlNameStart())
            {
                StringBuffer.Append(c);
                return TagName(NewOpenTag());
            }
            else if (IsSuppressingErrors)
            {
                StringBuffer.Append(Symbols.LessThan);
                return DataText(c);
            }
            else
            {
                throw XmlParseError.XmlInvalidStartTag.At(GetCurrentPosition());
            }
        }

        /// <summary>
        /// More http://www.w3.org/TR/REC-xml/#dt-etag.
        /// </summary>
        private XmlToken TagEnd()
        {
            var c = GetNext();

            if (c.IsXmlNameStart())
            {
                do
                {
                    StringBuffer.Append(c);
                    c = GetNext();
                }
                while (c.IsXmlName());

                while (c.IsSpaceCharacter())
                {
                    c = GetNext();
                }

                if (c == Symbols.GreaterThan)
                {
                    var tag = NewCloseTag();
                    tag.Name = FlushBuffer();
                    return tag;
                }
            }

            if (IsSuppressingErrors)
            {
                var tag = NewCloseTag();
                tag.Name = FlushBuffer();
                return tag;
            }
            else if (c == Symbols.EndOfFile)
            {
                throw XmlParseError.EOF.At(GetCurrentPosition());
            }
            else
            {
                throw XmlParseError.XmlInvalidEndTag.At(GetCurrentPosition());
            }
        }

        /// <summary>
        /// More http://www.w3.org/TR/REC-xml/#NT-Name.
        /// </summary>
        /// <param name="tag">The current tag token.</param>
        /// <returns>The emitted token.</returns>
        private XmlToken TagName(XmlTagToken tag)
        {
            var c = GetNext();

            while (c.IsXmlName())
            {
                StringBuffer.Append(c);
                c = GetNext();
            }

            tag.Name = FlushBuffer();

            if (c == Symbols.GreaterThan)
            {
                return tag;
            }
            else if (c.IsSpaceCharacter())
            {
                return AttributeBeforeName(tag);
            }
            else if (c == Symbols.Solidus)
            {
                return TagSelfClosing(tag);
            }
            else if (IsSuppressingErrors)
            {
                return tag;
            }
            else if (c == Symbols.EndOfFile)
            {
                throw XmlParseError.EOF.At(GetCurrentPosition());
            }
            else
            {
                throw XmlParseError.XmlInvalidName.At(GetCurrentPosition());
            }
        }

        /// <summary>
        /// More http://www.w3.org/TR/REC-xml/#d0e2480.
        /// </summary>
        /// <param name="tag">The current tag token.</param>
        private XmlToken TagSelfClosing(XmlTagToken tag)
        {
            var c = GetNext();
            tag.IsSelfClosing = true;

            if (c == Symbols.GreaterThan)
            {
                return tag;
            }
            else if (IsSuppressingErrors)
            {
                return tag;
            }
            else if (c == Symbols.EndOfFile)
            {
                throw XmlParseError.EOF.At(GetCurrentPosition());
            }
            else
            {
                throw XmlParseError.XmlInvalidName.At(GetCurrentPosition());
            }
        }

        /// <summary>
        /// More http://www.w3.org/TR/REC-xml/#dt-markup.
        /// </summary>
        private XmlToken MarkupDeclaration()
        {
            var pos = GetCurrentPosition();
            Advance();

            if (ContinuesWithSensitive("--"))
            {
                Advance();
                return CommentStart();
            }
            else if (ContinuesWithSensitive(TagNames.Doctype))
            {
                Advance(6);
                return Doctype();
            }
            else if (ContinuesWithSensitive(Keywords.CData))
            {
                Advance(6);
                return CData();
            }
            else if (IsSuppressingErrors)
            {
                return CommentStart();
            }
            else
            {
                throw XmlParseError.UndefinedMarkupDeclaration.At(pos);
            }
        }

        #endregion

        #region XML Declaration

        /// <summary>
        /// More http://www.w3.org/TR/REC-xml/#NT-XMLDecl.
        /// </summary>
        private XmlToken DeclarationStart()
        {
            var c = GetNext();

            if (!c.IsSpaceCharacter())
            {
                StringBuffer.Append(TagNames.Xml);
                return ProcessingTarget(c, NewProcessing());
            }

            do c = GetNext();
            while (c.IsSpaceCharacter());

            if (ContinuesWithSensitive(AttributeNames.Version))
            {
                Advance(6);
                return DeclarationVersionAfterName(NewDeclaration());
            }
            else if (IsSuppressingErrors)
            {
                StringBuffer.Append(TagNames.Xml);
                return ProcessingTarget(c, NewProcessing());
            }
            else
            {
                throw XmlParseError.XmlDeclarationInvalid.At(GetCurrentPosition());
            }
        }

        /// <summary>
        /// More http://www.w3.org/TR/REC-xml/#NT-VersionInfo.
        /// </summary>
        /// <param name="decl">The current declaration token.</param>
        private XmlToken DeclarationVersionAfterName(XmlDeclarationToken decl)
        {
            var c = SkipSpaces();

            if (c != Symbols.Equality && !IsSuppressingErrors)
            {
                throw XmlParseError.XmlDeclarationInvalid.At(GetCurrentPosition());
            }

            return DeclarationVersionBeforeValue(decl);
        }

        /// <summary>
        /// More http://www.w3.org/TR/REC-xml/#NT-VersionInfo.
        /// </summary>
        /// <param name="decl">The current declaration token.</param>
        private XmlToken DeclarationVersionBeforeValue(XmlDeclarationToken decl)
        {
            var c = SkipSpaces();

            if (c != Symbols.DoubleQuote && c != Symbols.SingleQuote && !IsSuppressingErrors)
            {
                throw XmlParseError.XmlDeclarationInvalid.At(GetCurrentPosition());
            }

            return DeclarationVersionValue(decl, c);
        }

        /// <summary>
        /// More http://www.w3.org/TR/REC-xml/#NT-VersionInfo.
        /// </summary>
        /// <param name="decl">The current declaration token.</param>
        /// <param name="quote">The quote character.</param>
        private XmlToken DeclarationVersionValue(XmlDeclarationToken decl, Char quote)
        {
            var c = GetNext();

            while (c != quote)
            {
                if (c == Symbols.EndOfFile)
                {
                    if (IsSuppressingErrors)
                    {
                        return DeclarationEnd(c, decl);
                    }
                    else
                    {
                        throw XmlParseError.EOF.At(GetCurrentPosition());
                    }
                }

                StringBuffer.Append(c);
                c = GetNext();
            }

            decl.Version = FlushBuffer();
            c = GetNext();

            if (c.IsSpaceCharacter())
            {
                return DeclarationAfterVersion(decl);
            }

            return DeclarationEnd(c, decl);
        }

        /// <summary>
        /// More http://www.w3.org/TR/REC-xml/#NT-VersionNum.
        /// </summary>
        /// <param name="decl">The current declaration token.</param>
        private XmlToken DeclarationAfterVersion(XmlDeclarationToken decl)
        {
            var c = SkipSpaces();

            if (ContinuesWithSensitive(AttributeNames.Encoding))
            {
                Advance(7);
                return DeclarationEncodingAfterName(decl);
            }
            else if (ContinuesWithSensitive(AttributeNames.Standalone))
            {
                Advance(9);
                return DeclarationStandaloneAfterName(decl);
            }

            return DeclarationEnd(c, decl);
        }

        /// <summary>
        /// More http://www.w3.org/TR/REC-xml/#NT-EncodingDecl.
        /// </summary>
        /// <param name="decl">The current declaration token.</param>
        private XmlToken DeclarationEncodingAfterName(XmlDeclarationToken decl)
        {
            var c = SkipSpaces();

            if (c != Symbols.Equality && !IsSuppressingErrors)
            {
                throw XmlParseError.XmlDeclarationInvalid.At(GetCurrentPosition());
            }

            return DeclarationEncodingBeforeValue(decl);
        }

        /// <summary>
        /// More http://www.w3.org/TR/REC-xml/#NT-EncodingDecl.
        /// </summary>
        /// <param name="decl">The current declaration token.</param>
        private XmlToken DeclarationEncodingBeforeValue(XmlDeclarationToken decl)
        {
            var c = SkipSpaces();

            if (c == Symbols.DoubleQuote || c == Symbols.SingleQuote)
            {
                var q = c;
                c = GetNext();

                if (c.IsLetter())
                {
                    StringBuffer.Append(c);
                    return DeclarationEncodingValue(decl, q);
                }
            }

            if (IsSuppressingErrors)
            {
                return DataText(c);
            }
            else
            {
                throw XmlParseError.XmlDeclarationInvalid.At(GetCurrentPosition());
            }
        }

        /// <summary>
        /// More http://www.w3.org/TR/REC-xml/#NT-EncodingDecl.
        /// </summary>
        /// <param name="decl">The current declaration token.</param>
        /// <param name="quote">The quote character.</param>
        private XmlToken DeclarationEncodingValue(XmlDeclarationToken decl, Char quote)
        {
            var c = GetNext();

            while (c != quote)
            {
                if (!c.IsAlphanumericAscii() && c != Symbols.Dot && c != Symbols.Underscore && c != Symbols.Minus)
                {
                    if (IsSuppressingErrors)
                    {
                        return DeclarationAfterEncoding(decl);
                    }
                    else
                    {
                        throw XmlParseError.XmlDeclarationInvalid.At(GetCurrentPosition());
                    }
                }

                StringBuffer.Append(c);
                c = GetNext();
            }

            decl.Encoding = FlushBuffer();
            c = GetNext();

            if (c.IsSpaceCharacter())
            {
                return DeclarationAfterEncoding(decl);
            }

            return DeclarationEnd(c, decl);
        }

        /// <summary>
        /// More http://www.w3.org/TR/REC-xml/#NT-SDDecl.
        /// </summary>
        /// <param name="decl">The current declaration token.</param>
        private XmlToken DeclarationAfterEncoding(XmlDeclarationToken decl)
        {
            var c = SkipSpaces();

            if (ContinuesWithSensitive(AttributeNames.Standalone))
            {
                Advance(9);
                return DeclarationStandaloneAfterName(decl);
            }

            return DeclarationEnd(c, decl);
        }

        /// <summary>
        /// More http://www.w3.org/TR/REC-xml/#NT-SDDecl.
        /// </summary>
        /// <param name="decl">The current declaration token.</param>
        private XmlToken DeclarationStandaloneAfterName(XmlDeclarationToken decl)
        {
            var c = SkipSpaces();

            if (c == Symbols.Equality)
            {
                return DeclarationStandaloneBeforeValue(decl);
            }
            else if (IsSuppressingErrors)
            {
                return DeclarationStandaloneBeforeValue(decl);
            }
            else
            {
                throw XmlParseError.XmlDeclarationInvalid.At(GetCurrentPosition());
            }

        }

        /// <summary>
        /// More http://www.w3.org/TR/REC-xml/#NT-SDDecl.
        /// </summary>
        /// <param name="decl">The current declaration token.</param>
        private XmlToken DeclarationStandaloneBeforeValue(XmlDeclarationToken decl)
        {
            var c = SkipSpaces();

            if (c == Symbols.DoubleQuote || c == Symbols.SingleQuote)
            {
                return DeclarationStandaloneValue(decl, c);
            }
            else if (IsSuppressingErrors)
            {
                return decl;
            }
            else
            {
                throw XmlParseError.XmlDeclarationInvalid.At(GetCurrentPosition());
            }
        }

        /// <summary>
        /// More http://www.w3.org/TR/REC-xml/#NT-SDDecl.
        /// </summary>
        /// <param name="decl">The current declaration token.</param>
        /// <param name="quote">The quote character.</param>
        private XmlToken DeclarationStandaloneValue(XmlDeclarationToken decl, Char quote)
        {
            var c = GetNext();

            while (c != quote)
            {
                if (c == Symbols.EndOfFile)
                {
                    if (IsSuppressingErrors)
                    {
                        return decl;
                    }
                    else
                    {
                        throw XmlParseError.EOF.At(GetCurrentPosition());
                    }
                }

                StringBuffer.Append(c);
                c = GetNext();
            }

            var s = FlushBuffer();

            if (s.Is(Keywords.Yes))
            {
                decl.Standalone = true;
            }
            else if (s.Is(Keywords.No))
            {
                decl.Standalone = false;
            }
            else if (IsSuppressingErrors)
            {
                return decl;
            }
            else
            {
                throw XmlParseError.XmlDeclarationInvalid.At(GetCurrentPosition());
            }

            return DeclarationEnd(GetNext(), decl);
        }

        /// <summary>
        /// More http://www.w3.org/TR/REC-xml/#NT-XMLDecl.
        /// </summary>
        /// <param name="c">The next input character.</param>
        /// <param name="decl">The current declaration token.</param>
        private XmlDeclarationToken DeclarationEnd(Char c, XmlDeclarationToken decl)
        {
            while (c.IsSpaceCharacter())
            {
                c = GetNext();
            }

            if (c == Symbols.QuestionMark && GetNext() == Symbols.GreaterThan)
            {
                return decl;
            }
            else if (IsSuppressingErrors)
            {
                return decl;
            }
            else
            {
                throw XmlParseError.XmlDeclarationInvalid.At(GetCurrentPosition());
            }
        }

        #endregion

        #region Doctype

        /// <summary>
        /// See 8.2.4.52 DOCTYPE state
        /// </summary>
        private XmlToken Doctype()
        {
            var c = GetNext();

            if (c.IsSpaceCharacter())
            {
                return DoctypeNameBefore();
            }
            else if (IsSuppressingErrors)
            {
                return DataText(c);
            }
            else
            {
                throw XmlParseError.DoctypeInvalid.At(GetCurrentPosition());
            }
        }

        /// <summary>
        /// See 8.2.4.53 Before DOCTYPE name state
        /// </summary>
        private XmlToken DoctypeNameBefore()
        {
            var c = SkipSpaces();

            if (c.IsXmlNameStart())
            {
                StringBuffer.Append(c);
                return DoctypeName(NewDoctype());
            }
            else if (IsSuppressingErrors)
            {
                return NewDoctype();
            }
            else
            {
                throw XmlParseError.DoctypeInvalid.At(GetCurrentPosition());
            }
        }

        /// <summary>
        /// See 8.2.4.54 DOCTYPE name state
        /// </summary>
        /// <param name="doctype">The current doctype token.</param>
        /// <returns>The emitted token.</returns>
        private XmlToken DoctypeName(XmlDoctypeToken doctype)
        {
            var c = GetNext();

            while (c.IsXmlName())
            {
                StringBuffer.Append(c);
                c = GetNext();
            }

            doctype.Name = FlushBuffer();

            if (c == Symbols.GreaterThan)
            {
                return doctype;
            }
            else if (c.IsSpaceCharacter())
            {
                return DoctypeNameAfter(doctype);
            }
            else if (IsSuppressingErrors)
            {
                return doctype;
            }
            else
            {
                throw XmlParseError.DoctypeInvalid.At(GetCurrentPosition());
            }
        }

        /// <summary>
        /// See 8.2.4.55 After DOCTYPE name state
        /// </summary>
        /// <param name="doctype">The current doctype token.</param>
        /// <returns>The emitted token.</returns>
        private XmlToken DoctypeNameAfter(XmlDoctypeToken doctype)
        {
            var c = SkipSpaces();

            if (c == Symbols.GreaterThan)
            {
                return doctype;
            }
            else if (ContinuesWithSensitive(Keywords.Public))
            {
                Advance(5);
                return DoctypePublic(doctype);
            }
            else if (ContinuesWithSensitive(Keywords.System))
            {
                Advance(5);
                return DoctypeSystem(doctype);
            }
            else if (c == Symbols.SquareBracketOpen)
            {
                return DoctypeInternalSubset(doctype);
            }
            else if (IsSuppressingErrors)
            {
                return doctype;
            }
            else
            {
                throw XmlParseError.DoctypeInvalid.At(GetCurrentPosition());
            }
        }

        /// <summary>
        /// See 8.2.4.56 After DOCTYPE public keyword state
        /// </summary>
        /// <param name="doctype">The current doctype token.</param>
        /// <returns>The emitted token.</returns>
        private XmlToken DoctypePublic(XmlDoctypeToken doctype)
        {
            var c = GetNext();

            if (c.IsSpaceCharacter())
            {
                c = SkipSpaces();

                if (c == Symbols.DoubleQuote || c == Symbols.SingleQuote)
                {
                    doctype.PublicIdentifier = String.Empty;
                    return DoctypePublicIdentifierValue(doctype, c);
                }
            }

            if (IsSuppressingErrors)
            {
                return doctype;
            }
            else
            {
                throw XmlParseError.DoctypeInvalid.At(GetCurrentPosition());
            }
        }

        /// <summary>
        /// See 8.2.4.58 DOCTYPE public identifier (double-quoted) state
        /// </summary>
        /// <param name="doctype">The current doctype token.</param>
        /// <param name="quote">The closing character.</param>
        /// <returns>The emitted token.</returns>
        private XmlToken DoctypePublicIdentifierValue(XmlDoctypeToken doctype, Char quote)
        {
            var c = GetNext();

            while (c != quote)
            {
                if (c.IsPubidChar())
                {
                    StringBuffer.Append(c);
                    c = GetNext();
                }
                else if (IsSuppressingErrors)
                {
                    break;
                }
                else
                {
                    throw XmlParseError.XmlInvalidPubId.At(GetCurrentPosition());
                }
            }

            doctype.PublicIdentifier = FlushBuffer();
            return DoctypePublicIdentifierAfter(doctype);
        }

        /// <summary>
        /// See 8.2.4.60 After DOCTYPE public identifier state
        /// </summary>
        /// <param name="doctype">The current doctype token.</param>
        /// <returns>The emitted token.</returns>
        private XmlToken DoctypePublicIdentifierAfter(XmlDoctypeToken doctype)
        {
            var c = GetNext();

            if (c == Symbols.GreaterThan)
            {
                return doctype;
            }
            else if (c.IsSpaceCharacter())
            {
                return DoctypeBetween(doctype);
            }
            else if (IsSuppressingErrors)
            {
                return doctype;
            }
            else
            {
                throw XmlParseError.DoctypeInvalid.At(GetCurrentPosition());
            }
        }

        /// <summary>
        /// See 8.2.4.61 Between DOCTYPE public and system identifiers state
        /// </summary>
        /// <param name="doctype">The current doctype token.</param>
        /// <returns>The emitted token.</returns>
        private XmlToken DoctypeBetween(XmlDoctypeToken doctype)
        {
            var c = SkipSpaces();

            if (c == Symbols.GreaterThan)
            {
                return doctype;
            }
            else if (c == Symbols.DoubleQuote || c == Symbols.SingleQuote)
            {
                doctype.SystemIdentifier = String.Empty;
                return DoctypeSystemIdentifierValue(doctype, c);
            }
            else if (IsSuppressingErrors)
            {
                return doctype;
            }
            else
            {
                throw XmlParseError.DoctypeInvalid.At(GetCurrentPosition());
            }
        }

        /// <summary>
        /// See 8.2.4.62 After DOCTYPE system keyword state
        /// </summary>
        /// <param name="doctype">The current doctype token.</param>
        /// <returns>The emitted token.</returns>
        private XmlToken DoctypeSystem(XmlDoctypeToken doctype)
        {
            var c = GetNext();

            if (c.IsSpaceCharacter())
            {
                c = SkipSpaces();

                if (c == Symbols.DoubleQuote || c == Symbols.SingleQuote)
                {
                    doctype.SystemIdentifier = String.Empty;
                    return DoctypeSystemIdentifierValue(doctype, c);
                }
            }

            if (IsSuppressingErrors)
            {
                return doctype;
            }
            else
            {
                throw XmlParseError.DoctypeInvalid.At(GetCurrentPosition());
            }
        }

        /// <summary>
        /// See 8.2.4.64 DOCTYPE system identifier (double-quoted) state
        /// </summary>
        /// <param name="doctype">The current doctype token.</param>
        /// <param name="quote">The quote character.</param>
        /// <returns>The emitted token.</returns>
        private XmlToken DoctypeSystemIdentifierValue(XmlDoctypeToken doctype, Char quote)
        {
            var c = GetNext();

            while (c != quote)
            {
                if (c != Symbols.EndOfFile)
                {
                    StringBuffer.Append(c);
                    c = GetNext();
                }
                else if (IsSuppressingErrors)
                {
                    break;
                }
                else
                {
                    throw XmlParseError.EOF.At(GetCurrentPosition());
                }
            }

            doctype.SystemIdentifier = FlushBuffer();
            return DoctypeSystemIdentifierAfter(doctype);
        }

        /// <summary>
        /// See 8.2.4.66 After DOCTYPE system identifier state
        /// </summary>
        /// <param name="doctype">The current doctype token.</param>
        /// <returns>The emitted token.</returns>
        private XmlToken DoctypeSystemIdentifierAfter(XmlDoctypeToken doctype)
        {
            var c = SkipSpaces();

            if (c == Symbols.SquareBracketOpen)
            {
                return DoctypeInternalSubset(doctype);
            }

            return DoctypeAfter(c, doctype);
        }

        /// <summary>
        /// Consumes a DOCTYPE internal subset, i.e. the content enclosed in
        /// square brackets and terminated by a closing <c>]&gt;</c> sequence.
        /// </summary>
        /// <param name="doctype">The current doctype token.</param>
        /// <returns>The emitted token.</returns>
        private XmlToken DoctypeInternalSubset(XmlDoctypeToken doctype)
        {
            var quote = Symbols.Null;
            var c = GetNext();

            while (c != Symbols.EndOfFile)
            {
                if (quote != Symbols.Null)
                {
                    StringBuffer.Append(c);

                    if (c == quote)
                    {
                        quote = Symbols.Null;
                    }

                    c = GetNext();
                }
                else if (c == Symbols.DoubleQuote || c == Symbols.SingleQuote)
                {
                    quote = c;
                    StringBuffer.Append(c);
                    c = GetNext();
                }
                else if (c == Symbols.SquareBracketClose)
                {
                    var next = GetNext();

                    if (next == Symbols.GreaterThan)
                    {
                        doctype.InternalSubset = FlushBuffer();
                        return doctype;
                    }

                    StringBuffer.Append(c);
                    c = next;
                }
                else
                {
                    StringBuffer.Append(c);
                    c = GetNext();
                }
            }

            if (IsSuppressingErrors)
            {
                doctype.InternalSubset = FlushBuffer();
                return doctype;
            }

            throw XmlParseError.EOF.At(GetCurrentPosition());
        }

        /// <summary>
        /// The doctype finalizer.
        /// </summary>
        /// <param name="c">The next input character.</param>
        /// <param name="doctype">The current doctype token.</param>
        /// <returns>The emitted token.</returns>
        private XmlToken DoctypeAfter(Char c, XmlDoctypeToken doctype)
        {
            while (c.IsSpaceCharacter())
            {
                c = GetNext();
            }

            if (c == Symbols.GreaterThan)
            {
                return doctype;
            }
            else if (IsSuppressingErrors)
            {
                return doctype;
            }
            else
            {
                throw XmlParseError.DoctypeInvalid.At(GetCurrentPosition());
            }
        }

        #endregion

        #region Attributes

        /// <summary>
        /// More http://www.w3.org/TR/REC-xml/#NT-Attribute.
        /// </summary>
        /// <param name="tag">The current tag token.</param>
        private XmlToken AttributeBeforeName(XmlTagToken tag)
        {
            var c = SkipSpaces();

            if (c == Symbols.Solidus)
            {
                return TagSelfClosing(tag);
            }
            else if (c == Symbols.GreaterThan)
            {
                return tag;
            }

            if (c == Symbols.EndOfFile)
            {
                if (IsSuppressingErrors)
                {
                    return tag;
                }
                else
                {
                    throw XmlParseError.EOF.At(GetCurrentPosition());
                }
            }

            if (c.IsXmlNameStart())
            {
                StringBuffer.Append(c);
                return AttributeName(tag);
            }
            else if (IsSuppressingErrors)
            {
                return tag;
            }
            else
            {
                throw XmlParseError.XmlInvalidAttribute.At(GetCurrentPosition());
            }
        }

        /// <summary>
        /// More http://www.w3.org/TR/REC-xml/#NT-Attribute.
        /// </summary>
        /// <param name="tag">The current tag token.</param>
        private XmlToken AttributeName(XmlTagToken tag)
        {
            var c = GetNext();

            while (c.IsXmlName())
            {
                StringBuffer.Append(c);
                c = GetNext();
            }

            var name = FlushBuffer();

            if (!String.IsNullOrEmpty(tag.GetAttribute(name)))
            {
                if (IsSuppressingErrors)
                {
                    return tag;
                }
                else
                {
                    throw XmlParseError.XmlUniqueAttribute.At(GetCurrentPosition());
                }
            }

            tag.AddAttribute(name);

            if (c.IsSpaceCharacter())
            {
                do c = GetNext();
                while (c.IsSpaceCharacter());
            }

            if (c == Symbols.Equality)
            {
                return AttributeBeforeValue(tag);
            }
            else if (IsSuppressingErrors)
            {
                return tag;
            }
            else
            {
                throw XmlParseError.XmlInvalidAttribute.At(GetCurrentPosition());
            }
        }

        /// <summary>
        /// More http://www.w3.org/TR/REC-xml/#NT-Attribute.
        /// </summary>
        /// <param name="tag">The current tag token.</param>
        private XmlToken AttributeBeforeValue(XmlTagToken tag)
        {
            var c = SkipSpaces();

            if (c == Symbols.DoubleQuote || c == Symbols.SingleQuote)
            {
                return AttributeValue(tag, c);
            }
            else if (IsSuppressingErrors)
            {
                return tag;
            }
            else
            {
                throw XmlParseError.XmlInvalidAttribute.At(GetCurrentPosition());
            }
        }

        /// <summary>
        /// More http://www.w3.org/TR/REC-xml/#NT-Attribute.
        /// </summary>
        /// <param name="tag">The current tag token.</param>
        /// <param name="quote">The quote character.</param>
        private XmlToken AttributeValue(XmlTagToken tag, Char quote)
        {
            var c = GetNext();
            _entityReferences = null;

            while (c != quote)
            {
                if (c == Symbols.EndOfFile)
                {
                    if (IsSuppressingErrors)
                    {
                        return tag;
                    }
                    else
                    {
                        throw XmlParseError.EOF.At(GetCurrentPosition());
                    }
                }

                if (c == Symbols.LessThan && !IsSuppressingErrors)
                {
                    throw XmlParseError.XmlLtInAttributeValue.At(GetCurrentPosition());
                }

                if (c == Symbols.Ampersand)
                {
                    StringBuffer.Append(CharacterReference());
                }
                else
                {
                    StringBuffer.Append(c);
                }

                c = GetNext();
            }

            tag.SetAttributeValue(FlushBuffer(), TakeEntityReferences());
            return AttributeAfterValue(tag);
        }

        /// <summary>
        /// More http://www.w3.org/TR/REC-xml/#NT-Attribute.
        /// </summary>
        /// <param name="tag">The current tag token.</param>
        private XmlToken AttributeAfterValue(XmlTagToken tag)
        {
            var c = GetNext();

            if (c.IsSpaceCharacter())
            {
                return AttributeBeforeName(tag);
            }
            else if (c == Symbols.Solidus)
            {
                return TagSelfClosing(tag);
            }
            else if (c == Symbols.GreaterThan)
            {
                return tag;
            }
            else if (IsSuppressingErrors)
            {
                return tag;
            }
            else
            {
                throw XmlParseError.XmlInvalidAttribute.At(GetCurrentPosition());
            }
        }

        #endregion

        #region Processing Instruction

        /// <summary>
        /// More http://www.w3.org/TR/REC-xml/#sec-pi.
        /// </summary>
        /// <param name="c">The next input character.</param>
        private XmlToken ProcessingStart(Char c)
        {
            if (c.IsXmlNameStart())
            {
                StringBuffer.Append(c);
                return ProcessingTarget(GetNext(), NewProcessing());
            }
            else if (IsSuppressingErrors)
            {
                return DataText(c);
            }
            else
            {
                throw XmlParseError.XmlInvalidPI.At(GetCurrentPosition());
            }
        }

        /// <summary>
        /// More http://www.w3.org/TR/REC-xml/#sec-pi.
        /// </summary>
        /// <param name="c">The next input character.</param>
        /// <param name="pi">The processing instruction token.</param>
        private XmlToken ProcessingTarget(Char c, XmlPIToken pi)
        {
            while (c.IsXmlName())
            {
                StringBuffer.Append(c);
                c = GetNext();
            }

            pi.Target = FlushBuffer();

            if (pi.Target.Isi(TagNames.Xml))
            {
                if (IsSuppressingErrors)
                {
                    return pi;
                }
                else
                {
                    throw XmlParseError.XmlInvalidPI.At(GetCurrentPosition());
                }
            }

            if (c == Symbols.QuestionMark)
            {
                c = GetNext();

                if (c == Symbols.GreaterThan)
                {
                    return pi;
                }
            }
            else if (c.IsSpaceCharacter())
            {
                return ProcessingContent(pi);
            }

            if (IsSuppressingErrors)
            {
                return pi;
            }
            else
            {
                throw XmlParseError.XmlInvalidPI.At(GetCurrentPosition());
            }
        }

        /// <summary>
        /// More http://www.w3.org/TR/REC-xml/#sec-pi.
        /// </summary>
        /// <param name="pi">The processing instruction token.</param>
        private XmlToken ProcessingContent(XmlPIToken pi)
        {
            var c = GetNext();

            while (c != Symbols.EndOfFile)
            {
                if (c == Symbols.QuestionMark)
                {
                    c = GetNext();

                    if (c == Symbols.GreaterThan)
                    {
                        pi.Content = FlushBuffer();
                        return pi;
                    }

                    StringBuffer.Append(Symbols.QuestionMark);
                }
                else
                {
                    StringBuffer.Append(c);
                    c = GetNext();
                }
            }

            if (IsSuppressingErrors)
            {
                return pi;
            }
            else
            {
                throw XmlParseError.EOF.At(GetCurrentPosition());
            }
        }

        #endregion

        #region Comments

        /// <summary>
        /// More http://www.w3.org/TR/REC-xml/#sec-comments.
        /// </summary>
        private XmlToken CommentStart()
        {
            return Comment(GetNext());
        }

        /// <summary>
        /// More http://www.w3.org/TR/REC-xml/#sec-comments.
        /// </summary>
        /// <param name="c">The next input character.</param>
        private XmlToken Comment(Char c)
        {
            while (c.IsXmlChar())
            {
                if (c == Symbols.Minus)
                {
                    return CommentDash();
                }

                StringBuffer.Append(c);
                c = GetNext();
            }

            if (IsSuppressingErrors)
            {
                return CommentEnd();
            }
            else
            {
                throw XmlParseError.XmlInvalidComment.At(GetCurrentPosition());
            }
        }

        /// <summary>
        /// More http://www.w3.org/TR/REC-xml/#sec-comments.
        /// </summary>
        private XmlToken CommentDash()
        {
            var c = GetNext();

            if (c == Symbols.Minus)
            {
                return CommentEnd();
            }

            return Comment(c);
        }

        /// <summary>
        /// More http://www.w3.org/TR/REC-xml/#sec-comments.
        /// </summary>
        private XmlToken CommentEnd()
        {
            var c = GetNext();

            if (c == Symbols.GreaterThan)
            {
                return NewComment();
            }
            else if (IsSuppressingErrors)
            {
                return NewComment();
            }
            else
            {
                throw XmlParseError.XmlInvalidComment.At(GetCurrentPosition());
            }
        }

        #endregion

        #region Tokens

        private XmlEndOfFileToken NewEof()
        {
            return new XmlEndOfFileToken(GetCurrentPosition());
        }

        private XmlCharacterToken NewCharacters(Boolean hasCharacterReference = false)
        {
            var content = FlushBuffer();
            return new XmlCharacterToken(_position, content, hasCharacterReference)
            {
                EntityReferences = TakeEntityReferences(),
            };
        }

        private List<XmlEntityReference> TakeEntityReferences()
        {
            var references = _entityReferences;
            _entityReferences = null;
            return references;
        }

        private XmlCommentToken NewComment()
        {
            var comment = FlushBuffer();
            return new XmlCommentToken(_position, comment);
        }

        private XmlPIToken NewProcessing()
        {
            return new XmlPIToken(_position);
        }

        private XmlDoctypeToken NewDoctype()
        {
            return new XmlDoctypeToken(_position);
        }

        private XmlDeclarationToken NewDeclaration()
        {
            return new XmlDeclarationToken(_position);
        }

        private XmlTagToken NewOpenTag()
        {
            return new XmlTagToken(XmlTokenType.StartTag, _position);
        }

        private XmlTagToken NewCloseTag()
        {
            return new XmlTagToken(XmlTokenType.EndTag, _position);
        }

        private XmlCDataToken NewCharacterData()
        {
            var content = FlushBuffer();
            return new XmlCDataToken(_position, content);
        }

        #endregion
    }
}
