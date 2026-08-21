namespace AngleSharp.Xml
{
    using System;

    /// <summary>
    /// Represents an XSD schema compilation or document validation diagnostic.
    /// </summary>
    public sealed class XsdValidationDiagnostic
    {
        internal XsdValidationDiagnostic(XsdValidationSeverity severity, String message, String sourceUri, Int32 lineNumber, Int32 linePosition)
        {
            Severity = severity;
            Message = message;
            SourceUri = sourceUri;
            LineNumber = lineNumber;
            LinePosition = linePosition;
        }

        /// <summary>
        /// Gets the diagnostic severity.
        /// </summary>
        public XsdValidationSeverity Severity { get; }

        /// <summary>
        /// Gets the diagnostic message.
        /// </summary>
        public String Message { get; }

        /// <summary>
        /// Gets the source URI when available.
        /// </summary>
        public String SourceUri { get; }

        /// <summary>
        /// Gets the one-based source line, or zero when unavailable.
        /// </summary>
        public Int32 LineNumber { get; }

        /// <summary>
        /// Gets the one-based source position, or zero when unavailable.
        /// </summary>
        public Int32 LinePosition { get; }
    }
}