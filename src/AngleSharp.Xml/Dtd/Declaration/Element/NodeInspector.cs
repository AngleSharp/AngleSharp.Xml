namespace AngleSharp.Xml.Dtd.Declaration
{
    using AngleSharp.Dom;
    using System;
    using System.Collections.Generic;

    sealed class NodeInspector
    {
        private readonly List<INode> nodes;

        public NodeInspector(IElement element)
        {
            nodes = new List<INode>();

            foreach (var child in element.ChildNodes)
            {
                if ((child is IText && !String.IsNullOrEmpty(((IText)child).Text)) || child is IElement)
                {
                    nodes.Add(child);
                }
            }
        }

        public List<INode> Children
        {
            get { return nodes; }
        }

        public INode Current
        {
            get { return Children[Index]; }
        }

        public Int32 Length
        {
            get { return Children.Count; }
        }

        public Int32 Index
        {
            get;
            set;
        }

        public Boolean IsCompleted 
        {
            get { return Children.Count == Index; }
        }
    }
}
