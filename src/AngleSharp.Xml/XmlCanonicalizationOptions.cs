namespace AngleSharp.Xml
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Configures canonical XML serialization.
    /// </summary>
    public sealed class XmlCanonicalizationOptions
    {
        /// <summary>
        /// Gets or sets the canonicalization algorithm.
        /// </summary>
        public XmlCanonicalizationMode Mode { get; set; } = XmlCanonicalizationMode.CanonicalXml11;

        /// <summary>
        /// Gets or sets if comments are included in the canonical output.
        /// </summary>
        public Boolean IncludeComments { get; set; }

        /// <summary>
        /// Gets or sets the prefixes processed inclusively by exclusive canonicalization.
        /// Use an empty string or <c>#default</c> for the default namespace.
        /// </summary>
        public IEnumerable<String> InclusiveNamespacePrefixes { get; set; }
    }
}