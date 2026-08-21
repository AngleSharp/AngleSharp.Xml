namespace AngleSharp.Xml.Dom
{
    using AngleSharp.Attributes;
    using AngleSharp.Dom;
    using System;

    /// <summary>
    /// The interface represent an XML document.
    /// </summary>
    [DomName("XMLDocument")]
    public interface IXmlDocument : IDocument
    {
        /// <summary>
        /// Gets the XML declaration version.
        /// </summary>
        String XmlVersion { get; }

        /// <summary>
        /// Gets the encoding specified by the XML declaration, if any.
        /// </summary>
        String XmlEncoding { get; }

        /// <summary>
        /// Gets if the XML declaration specifies a standalone document.
        /// </summary>
        Boolean XmlStandalone { get; }

        /// <summary>
        /// Gets if the document is actually valid.
        /// </summary>
        Boolean IsValid { get; }

        /// <summary>
        /// Creates a CDATA section owned by this document.
        /// </summary>
        /// <param name="data">The section's character data.</param>
        /// <returns>The created CDATA section.</returns>
        IXmlCDataSection CreateCDataSection(String data);

        /// <summary>
        /// Entity reference nodes are not supported by AngleSharp's DOM.
        /// </summary>
        /// <param name="name">The entity name.</param>
        /// <returns>This method does not return.</returns>
        /// <exception cref="NotSupportedException">Always thrown.</exception>
        INode CreateEntityReference(String name);
    }
}
