namespace AngleSharp.Xml.Dom
{
    using AngleSharp.Attributes;
    using AngleSharp.Dom;
    using System;

    /// <summary>
    /// Represents an entity node.
    /// </summary>
    [DomName("Entity")]
    public sealed class Entity : Node
    {
        #region Fields

        String _publicId;
        String _systemId;
        String _notationName;
        String _inputEncoding;
        String _xmlVersion;
        String _xmlEncoding;
        String _value;

        #endregion

        #region ctor

        /// <summary>
        /// Creates a new entity node.
        /// </summary>
        internal Entity()
            : this(String.Empty)
        {
        }

        /// <summary>
        /// Creates a new entity node.
        /// </summary>
        internal Entity(String name)
            : base(null, name, NodeType.Entity)
        {
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the public identiifer.
        /// </summary>
        [DomName("publicId")]
        public String PublicId
        {
            get { return _publicId; }
        }

        /// <summary>
        /// Gets the system identifier.
        /// </summary>
        [DomName("systemId")]
        public String SystemId
        {
            get { return _systemId; }
        }

        /// <summary>
        /// Gets the notation name.
        /// </summary>
        [DomName("notationName")]
        public String NotationName
        {
            get { return _notationName; }
            internal set { _notationName = value; }
        }

        /// <summary>
        /// Gets the used input encoding.
        /// </summary>
        [DomName("inputEncoding")]
        public String InputEncoding
        {
            get { return _inputEncoding; }
        }

        /// <summary>
        /// Gets the used XML encoding.
        /// </summary>
        [DomName("xmlEncoding")]
        public String XmlEncoding
        {
            get { return _xmlEncoding; }
        }

        /// <summary>
        /// Gets the used XML version.
        /// </summary>
        [DomName("xmlVersion")]
        public String XmlVersion
        {
            get { return _xmlVersion; }
        }

        /// <summary>
        /// Gets or sets the entity's value.
        /// </summary>
        [DomName("textContent")]
        public override String TextContent
        {
            get { return NodeValue; }
            set { NodeValue = value; }
        }

        /// <summary>
        /// Gets or sets the value of the entity.
        /// </summary>
        [DomName("nodeValue")]
        public override String NodeValue
        {
            get { return _value; }
            set { _value = value; }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Returns a duplicate of the node on which this method was called.
        /// </summary>
        public override Node Clone(Document newOwner, Boolean deep)
        {
            var node = new Entity();
            CloneNode(node, newOwner, deep);
            node._xmlEncoding = this._xmlEncoding;
            node._xmlVersion = this._xmlVersion;
            node._systemId = this._systemId;
            node._publicId = this._publicId;
            node._inputEncoding = this._inputEncoding;
            node._notationName = this._notationName;
            return node;
        }

        #endregion
    }
}
