namespace AngleSharp.Xml.Dtd.Declaration
{
    using AngleSharp.Dom;
    using System;
    using System.Collections.Generic;

    sealed class AttributeDeclaration : Node
    {
        #region Fields

        private readonly List<AttributeDeclarationEntry> _attrs;

        #endregion

        #region ctor

        internal AttributeDeclaration(IEnumerable<AttributeDeclarationEntry> attributes)
            : base(null, String.Empty, NodeType.ProcessingInstruction)
        {
            _attrs = new List<AttributeDeclarationEntry>(attributes);
        }

        #endregion

        #region Properties

        public AttributeDeclarationEntry this[Int32 index]
        {
            get { return _attrs[index]; }
        }

        public Int32 Count
        {
            get { return _attrs.Count; }
        }

        public IEnumerable<AttributeDeclarationEntry> Declarations
        {
            get
            {
                foreach (var attribute in _attrs)
                {
                    yield return attribute;
                }
            }
        }

        public String Name
        {
            get;
            set;
        }

        #endregion

        #region Methods

        public Boolean Check(Element element)
        {
            foreach (var attr in _attrs)
            {
                if (!attr.Check(element))
                {
                    return false;
                }
            }

            return true;
        }

        public override Node Clone(Document newOwner, Boolean deep)
        {
            var attr = new AttributeDeclaration(_attrs);
            CloneNode(attr, newOwner, deep);
            return attr;
        }

        #endregion
    }
}
