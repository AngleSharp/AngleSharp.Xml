namespace AngleSharp.Xml.Dom
{
    using AngleSharp.Dom;
    using AngleSharp.Text;
    using AngleSharp.Xml.Parser;
    using System;
    using System.Collections.Generic;
    using System.Text;

    /// <summary>
    /// The object representation of an XMLElement.
    /// </summary>
    sealed class XmlElement : Element
    {        
        #region ctor

        public XmlElement(Document owner, String name, String prefix = null, String namespaceUri = null, NodeFlags flags = NodeFlags.None)
            : base(owner, name, prefix, namespaceUri, flags)
        {
        }

        #endregion

        #region Properties

        internal String IdAttribute
        {
            get;
            set;
        }

        public override String TextContent
        {
            get
            {
                var children = ((INode)this).ChildNodes;

                // Most elements hold a single text or CDATA section, which already is the result
                if (children.Length == 1 && children[0] is not XmlElement)
                {
                    return HasTextContent(children[0]) ? children[0].TextContent : String.Empty;
                }

                // Walks with an explicit stack, so deeply nested documents cannot overflow the call stack
                var content = StringBuilderPool.Obtain();
                var pending = default(Stack<INode>);

                for (var i = 0; i < children.Length; i++)
                {
                    Collect(content, children[i], ref pending);

                    while (pending != null && pending.Count > 0)
                    {
                        Collect(content, pending.Pop(), ref pending);
                    }
                }

                return content.ToPool();
            }
            set => base.TextContent = value;
        }

        #endregion

        #region Methods

        /// <inheritdoc />
        public override IElement ParseSubtree(String html)
        {
            var context = ((IElement)this).Owner?.Context;
            var source = new TextSource(html);
            var document = new XmlDocument(context, source);
            var parser = new XmlDomBuilder(document);
            var options = new XmlParserOptions
            {
                IsSuppressingErrors = true,
            };
            return parser.ParseFragment(options, this).DocumentElement;
        }

        /// <inheritdoc />
        public override Node Clone(Document owner, Boolean deep)
        {
            var node = new XmlElement(owner, LocalName, Prefix, NamespaceUri, Flags);
            CloneElement(node, owner, deep);
            node.IdAttribute = IdAttribute;
            return node;
        }

        #endregion

        #region Helpers

        private static Boolean HasTextContent(INode node) =>
            node.NodeType == NodeType.Element ||
            node.NodeType == NodeType.Text ||
            node.NodeType == NodeType.CharacterData;

        /// <summary>
        /// Appends the text of a node that is not an XML element, or of an XML element holding a
        /// single such node; the children of any other XML element go on the stack, which is only
        /// allocated then.
        /// </summary>
        private static void Collect(StringBuilder content, INode node, ref Stack<INode> pending)
        {
            if (node is XmlElement)
            {
                var children = node.ChildNodes;

                if (children.Length == 1 && children[0] is not XmlElement)
                {
                    if (HasTextContent(children[0]))
                    {
                        content.Append(children[0].TextContent);
                    }
                }
                else if (children.Length > 0)
                {
                    pending ??= new Stack<INode>();

                    for (var i = children.Length - 1; i >= 0; i--)
                    {
                        pending.Push(children[i]);
                    }
                }
            }
            else if (HasTextContent(node))
            {
                content.Append(node.TextContent);
            }
        }

        #endregion
    }
}
