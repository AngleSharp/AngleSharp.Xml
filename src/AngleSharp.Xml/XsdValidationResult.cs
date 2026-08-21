namespace AngleSharp.Xml
{
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Contains the result of XSD validation.
    /// </summary>
    public sealed class XsdValidationResult
    {
        internal XsdValidationResult(IReadOnlyList<XsdValidationDiagnostic> diagnostics)
        {
            Diagnostics = diagnostics;
            IsValid = diagnostics.All(m => m.Severity != XsdValidationSeverity.Error);
        }

        /// <summary>
        /// Gets if validation completed without errors.
        /// </summary>
        public System.Boolean IsValid { get; }

        /// <summary>
        /// Gets the collected validation diagnostics.
        /// </summary>
        public IReadOnlyList<XsdValidationDiagnostic> Diagnostics { get; }
    }
}