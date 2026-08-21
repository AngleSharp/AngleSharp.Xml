namespace AngleSharp.Xml.Dtd.Declaration
{
    using AngleSharp.Dom;
    using AngleSharp.Text;
    using System;

    sealed class AttributeTokenizedType : AttributeTypeDeclaration
    {
        #region Properties

        public TokenizedType Value
        {
            get;
            set;
        }

        #endregion

        #region Enumeration

        public enum TokenizedType
        {
            ID,
            IDREF,
            IDREFS,
            ENTITY,
            ENTITIES,
            NMTOKEN,
            NMTOKENS
        }

        #endregion

        #region Methods

        public override Boolean Check(Element element)
        {
            var attr = element.GetAttribute(Parent.Name);

            if (attr == null)
                return true;

            switch (Value)
            {
                case TokenizedType.ENTITIES:
                {
                    return CheckNames(attr, false);
                }
                case TokenizedType.ENTITY:
                {
                    return CheckNames(attr, true);
                }
                case TokenizedType.ID:
                {
                    return CheckNames(attr, true);
                }
                case TokenizedType.IDREF:
                {
                    return CheckNames(attr, true);
                }
                case TokenizedType.IDREFS:
                {
                    return CheckNames(attr, false);
                }
                case TokenizedType.NMTOKEN:
                {
                    for (var i = 0; i < attr.Length; i++)
                    {
                        if (!attr[i].IsXmlName())
                        {
                            return false;
                        }
                    }

                    return true;
                }
                case TokenizedType.NMTOKENS:
                {
                    for (var i = 0; i < attr.Length; i++)
                    {
                        if (!attr[i].IsSpaceCharacter() && !attr[i].IsXmlName())
                        {
                            return false;
                        }
                    }

                    break;
                }
            }

            return true;
        }

        private static Boolean CheckNames(String value, Boolean requiresSingleName)
        {
            var names = value.Split((Char[])null, StringSplitOptions.RemoveEmptyEntries);

            if (names.Length == 0 || requiresSingleName && names.Length != 1)
            {
                return false;
            }

            foreach (var name in names)
            {
                if (String.IsNullOrEmpty(name) || !name[0].IsXmlNameStart())
                {
                    return false;
                }

                for (var i = 1; i < name.Length; i++)
                {
                    if (!name[i].IsXmlName())
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        #endregion
    }
}
