namespace AngleSharp.Xml
{
    using AngleSharp.Dom;
    using AngleSharp.Html;
    using AngleSharp.Xml.Dom;
    using AngleSharp.Xhtml;
    using System;

    /// <summary>
    /// AutoSelectedMarkupFormatter class to select the proper MarkupFormatter
    /// implementation depending on the used document type.
    /// </summary>
    public class AutoSelectedMarkupFormatter : IMarkupFormatter
    {
        #region Fields

        private IMarkupFormatter childFormatter = null;
        private IDocumentType _docType;

        #endregion

        #region ctor

        /// <summary>
        /// Creates a new instance of the auto selected markup formatter.
        /// </summary>
        /// <param name="docType">
        /// Optional DocumentType to hint the implementation to use.
        /// </param>
        public AutoSelectedMarkupFormatter(IDocumentType docType = null)
        {
            _docType = docType;
        }

        #endregion

        #region Properties

        private IMarkupFormatter ChildFormatter
        {
            get
            {
                if (childFormatter == null && _docType != null)
                {
                    if (_docType.PublicIdentifier.Contains("XML"))
                    {
                        childFormatter = XmlMarkupFormatter.Instance;
                    }
                    else if (_docType.PublicIdentifier.Contains("XHTML"))
                    {
                        childFormatter = XhtmlMarkupFormatter.Instance;
                    }
                }

                return childFormatter ?? HtmlMarkupFormatter.Instance;
            }
            set => childFormatter = value;
        }

        #endregion

        #region Methods

        /// <inheritdoc />
        public virtual String OpenTag(IElement element, Boolean selfClosing)
        {
            Confirm(element.Owner);
            return ChildFormatter.OpenTag(element, selfClosing);
        }

        /// <inheritdoc />
        public virtual String CloseTag(IElement element, Boolean selfClosing)
        {
            Confirm(element.Owner);
            return ChildFormatter.CloseTag(element, selfClosing);
        }

        /// <inheritdoc />
        public virtual String Comment(IComment comment)
        {
            Confirm(comment.Owner);
            return ChildFormatter.Comment(comment);
        }

        /// <inheritdoc />
        public virtual String Doctype(IDocumentType doctype)
        {
            Confirm(doctype);
            return ChildFormatter.Doctype(doctype);
        }

        /// <inheritdoc />
        public virtual String Processing(IProcessingInstruction processing)
        {
            Confirm(processing.Owner);
            return ChildFormatter.Processing(processing);
        }

        /// <inheritdoc />
        public virtual String Text(ICharacterData text)
        {
            Confirm(text.Owner);
            return ChildFormatter.Text(text);
        }

        /// <inheritdoc />
        public virtual String LiteralText(ICharacterData text)
        {
            Confirm(text.Owner);
            return ChildFormatter.LiteralText(text);
        }

        #endregion

        #region Helpers

        private void Confirm(IDocumentType docType)
        {
            if (_docType == null)
            {
                _docType = docType;
            }
        }

        private void Confirm(IDocument document)
        {
            if (childFormatter == null && document is IXmlDocument)
            {
                ChildFormatter = XmlMarkupFormatter.Instance;
            }
            else
            {
                Confirm(document?.Doctype);
            }
        }

        #endregion
    }
}
