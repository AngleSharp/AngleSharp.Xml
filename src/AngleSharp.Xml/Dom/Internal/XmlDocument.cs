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
        private String _xmlVersion;
        private String _xmlEncoding;
        private Boolean _xmlStandalone;

        #region ctor

        internal XmlDocument(IBrowsingContext context, TextSource source)
            : base(context ?? BrowsingContext.New(), source)
        {
            ContentType = MimeTypeNames.Xml;
            _isValid = true;
            _xmlVersion = "1.0";
        }

        internal XmlDocument(IBrowsingContext context = null)
            : this(context, new TextSource(String.Empty))
        {
        }

        #endregion

        #region Properties

        public override IElement DocumentElement => this.FindChild<IElement>();

        public override IEntityProvider Entities => Context.GetProvider<IEntityProvider>() ?? XmlEntityProvider.Resolver;

        public String XmlVersion => _xmlVersion;

        public String XmlEncoding => _xmlEncoding;

        public Boolean XmlStandalone => _xmlStandalone;

        public Boolean IsValid => _isValid;

        #endregion

        #region Methods

        public override Element CreateElementFrom(String name, String prefix, NodeFlags flags = NodeFlags.None) => new XmlElement(this, name, prefix, flags: flags);

        public IXmlCDataSection CreateCDataSection(String data)
        {
            return new XmlCDataSection(this, data);
        }

        public INode CreateEntityReference(String name) => throw new NotSupportedException("Entity reference nodes are not supported by AngleSharp's DOM.");

        public override Node Clone(Document owner, Boolean deep)
        {
            var node = new XmlDocument(Context, new TextSource(Source.Text));
            node.SetDeclaration(_xmlVersion, _xmlEncoding, _xmlStandalone);
            node.SetValidity(_isValid);
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

        internal void SetDeclaration(String version, String encoding, Boolean standalone)
        {
            _xmlVersion = version;
            _xmlEncoding = encoding;
            _xmlStandalone = standalone;
        }

        #endregion
    }
}
