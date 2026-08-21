namespace AngleSharp.Xml
{
    using AngleSharp.Dom;
    using System;
    using System.IO;

    /// <summary>
    /// Provides canonical XML serialization methods.
    /// </summary>
    public static class XmlCanonicalizationExtensions
    {
        /// <summary>
        /// Serializes a document or rooted element subtree to canonical UTF-8 bytes.
        /// </summary>
        /// <param name="node">The document or element to canonicalize.</param>
        /// <param name="options">The canonicalization options.</param>
        /// <returns>The canonical UTF-8 octets without a byte-order mark.</returns>
        public static Byte[] ToCanonicalXml(this INode node, XmlCanonicalizationOptions options = null)
        {
            return new XmlCanonicalizer(options).Canonicalize(node);
        }

        /// <summary>
        /// Serializes a document or rooted element subtree to a stream as canonical UTF-8 bytes.
        /// </summary>
        /// <param name="node">The document or element to canonicalize.</param>
        /// <param name="output">The output stream, which remains open.</param>
        /// <param name="options">The canonicalization options.</param>
        public static void ToCanonicalXml(this INode node, Stream output, XmlCanonicalizationOptions options = null)
        {
            if (output == null)
            {
                throw new ArgumentNullException(nameof(output));
            }

            var content = node.ToCanonicalXml(options);
            output.Write(content, 0, content.Length);
        }
    }
}