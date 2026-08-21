namespace AngleSharp.Xml
{
    using System;
    using System.Xml;
    using System.Xml.Schema;

    /// <summary>
    /// Configures XSD schema compilation and document validation.
    /// </summary>
    public sealed class XsdValidationOptions
    {
        /// <summary>
        /// Gets or sets if validation stops after the first error.
        /// </summary>
        public Boolean IsFailFast { get; set; }

        /// <summary>
        /// Gets or sets if schema validation warnings are collected.
        /// </summary>
        public Boolean IsReportingWarnings { get; set; } = true;

        /// <summary>
        /// Gets or sets the resolver used for schema imports and includes.
        /// The default is null, which disables external resource resolution.
        /// </summary>
        public XmlResolver SchemaResolver { get; set; }

        /// <summary>
        /// Gets or sets additional validation flags.
        /// </summary>
        public XmlSchemaValidationFlags ValidationFlags { get; set; } = XmlSchemaValidationFlags.ProcessIdentityConstraints;
    }
}