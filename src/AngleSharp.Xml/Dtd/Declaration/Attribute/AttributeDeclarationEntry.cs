namespace AngleSharp.Xml.Dtd.Declaration
{
    using AngleSharp.Dom;
    using System;

    sealed class AttributeDeclarationEntry
    {
        #region Fields

        private AttributeTypeDeclaration _type;
        private AttributeValueDeclaration _value;

        #endregion

        #region Properties

        public String Name
        {
            get;
            set;
        }

        public AttributeTypeDeclaration Type
        {
            get { return _type; }
            set
            {
                _type = value;
                _type.Parent = this;
            }
        }

        public AttributeValueDeclaration Default
        {
            get { return _value; }
            set
            {
                _value = value;
                _value.Parent = this;
            }
        }

        #endregion

        #region Methods

        public Boolean Check(Element element)
        {
            return _type.Check(element) && _value.Apply(element);
        }

        #endregion
    }
}
