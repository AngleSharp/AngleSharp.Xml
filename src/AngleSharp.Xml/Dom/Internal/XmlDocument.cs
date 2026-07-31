namespace AngleSharp.Xml.Dom
{
    using AngleSharp.Dom;
    using AngleSharp.Io;
    using AngleSharp.Text;
    using System;

    /// <summary>
    /// Represents a document node that contains only XML nodes.
    /// </summary>
    sealed class XmlDocument : Document, IXmlDocument
    {
        private Boolean _isValid;

        #region ctor

        internal XmlDocument(IBrowsingContext context, TextSource source)
            : base(context ?? BrowsingContext.New(), source)
        {
            ContentType = MimeTypeNames.Xml;
            _isValid = true;
        }

        internal XmlDocument(IBrowsingContext context = null)
            : this(context, new TextSource(String.Empty))
        {
        }

        #endregion

        #region Properties

        public override IElement DocumentElement => this.FindChild<IElement>();

        public override IEntityProvider Entities => Context.GetProvider<IEntityProvider>() ?? XmlEntityProvider.Resolver;

        public Boolean IsValid => _isValid;

        #endregion

        #region Methods

        public override Element CreateElementFrom(String name, String prefix, NodeFlags flags = NodeFlags.None) => new XmlElement(this, name, prefix, flags: flags);

        public override Node Clone(Document owner, Boolean deep)
        {
            var node = new XmlDocument(Context, new TextSource(Source.Text));
            CloneDocument(node, deep);
            return node;
        }

        #endregion

        #region Helpers

        protected override void SetTitle(String value)
        {
        }

        internal void SetValidity(Boolean isValid)
        {
            _isValid = isValid;
        }

        #endregion
    }
}
