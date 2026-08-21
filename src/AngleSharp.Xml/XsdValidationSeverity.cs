namespace AngleSharp.Xml
{
    /// <summary>
    /// Defines the severity of an XSD validation diagnostic.
    /// </summary>
    public enum XsdValidationSeverity
    {
        /// <summary>
        /// A non-fatal schema validation warning.
        /// </summary>
        Warning,

        /// <summary>
        /// A schema compilation or document validation error.
        /// </summary>
        Error,
    }
}