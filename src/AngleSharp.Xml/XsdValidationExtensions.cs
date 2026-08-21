namespace AngleSharp.Xml
{
    using AngleSharp.Dom;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Xml;
    using System.Xml.Schema;

    /// <summary>
    /// Provides XML Schema Definition (XSD) validation for AngleSharp documents.
    /// </summary>
    public static class XsdValidationExtensions
    {
        /// <summary>
        /// Validates an existing XML document against one or more inline XSD schemas.
        /// </summary>
        /// <param name="document">The document to validate.</param>
        /// <param name="schemas">The XSD schema documents.</param>
        /// <returns>The validation result.</returns>
        public static XsdValidationResult ValidateXsd(this IDocument document, params String[] schemas) =>
            document.ValidateXsd((IEnumerable<String>)schemas, null);

        /// <summary>
        /// Validates an existing XML document against one or more inline XSD schemas.
        /// </summary>
        /// <param name="document">The document to validate.</param>
        /// <param name="schemas">The XSD schema documents.</param>
        /// <param name="options">The validation options.</param>
        /// <returns>The validation result.</returns>
        public static XsdValidationResult ValidateXsd(this IDocument document, IEnumerable<String> schemas, XsdValidationOptions options)
        {
            if (document == null)
            {
                throw new ArgumentNullException(nameof(document));
            }

            if (schemas == null)
            {
                throw new ArgumentNullException(nameof(schemas));
            }

            options = options ?? new XsdValidationOptions();
            var diagnostics = new List<XsdValidationDiagnostic>();
            var schemaSet = new XmlSchemaSet
            {
                XmlResolver = options.SchemaResolver,
            };
            ValidationEventHandler handler = (sender, eventArguments) =>
                AddDiagnostic(diagnostics, eventArguments, options);
            schemaSet.ValidationEventHandler += handler;
            var schemaCount = 0;

            try
            {
                foreach (var schema in schemas)
                {
                    if (schema == null)
                    {
                        throw new ArgumentException("Schema documents cannot be null.", nameof(schemas));
                    }

                    schemaCount++;

                    using (var reader = XmlReader.Create(new StringReader(schema), CreateSchemaReaderSettings(options)))
                    {
                        schemaSet.Add(null, reader);
                    }
                }

                if (schemaCount == 0)
                {
                    throw new ArgumentException("At least one schema document is required.", nameof(schemas));
                }

                schemaSet.Compile();
            }
            catch (XsdFailFastException)
            {
                return new XsdValidationResult(diagnostics);
            }
            catch (XmlSchemaException exception)
            {
                AddException(diagnostics, exception);
                return new XsdValidationResult(diagnostics);
            }
            catch (XmlException exception)
            {
                AddException(diagnostics, exception);
                return new XsdValidationResult(diagnostics);
            }

            return Validate(document, schemaSet, options, diagnostics);
        }

        /// <summary>
        /// Validates an existing XML document against a configured schema set.
        /// Use the schema set to assign source URIs and configure imports or includes.
        /// </summary>
        /// <param name="document">The document to validate.</param>
        /// <param name="schemas">The configured schema set.</param>
        /// <param name="options">The validation options.</param>
        /// <returns>The validation result.</returns>
        public static XsdValidationResult ValidateXsd(this IDocument document, XmlSchemaSet schemas, XsdValidationOptions options = null)
        {
            if (document == null)
            {
                throw new ArgumentNullException(nameof(document));
            }

            if (schemas == null)
            {
                throw new ArgumentNullException(nameof(schemas));
            }

            if (schemas.Count == 0)
            {
                throw new ArgumentException("At least one schema document is required.", nameof(schemas));
            }

            options = options ?? new XsdValidationOptions();
            var diagnostics = new List<XsdValidationDiagnostic>();

            if (!schemas.IsCompiled)
            {
                if (options.SchemaResolver != null)
                {
                    schemas.XmlResolver = options.SchemaResolver;
                }

                ValidationEventHandler handler = (sender, eventArguments) =>
                    AddDiagnostic(diagnostics, eventArguments, options);
                schemas.ValidationEventHandler += handler;

                try
                {
                    schemas.Compile();
                }
                catch (XsdFailFastException)
                {
                    return new XsdValidationResult(diagnostics);
                }
                catch (XmlSchemaException exception)
                {
                    AddException(diagnostics, exception);
                    return new XsdValidationResult(diagnostics);
                }
                finally
                {
                    schemas.ValidationEventHandler -= handler;
                }
            }

            return Validate(document, schemas, options, diagnostics);
        }

        private static XsdValidationResult Validate(IDocument document, XmlSchemaSet schemas, XsdValidationOptions options, List<XsdValidationDiagnostic> diagnostics)
        {
            var settings = new XmlReaderSettings
            {
                DtdProcessing = DtdProcessing.Ignore,
                Schemas = schemas,
                ValidationFlags = options.ValidationFlags,
                ValidationType = ValidationType.Schema,
                XmlResolver = null,
            };

            if (options.IsReportingWarnings)
            {
                settings.ValidationFlags |= XmlSchemaValidationFlags.ReportValidationWarnings;
            }

            settings.ValidationEventHandler += (sender, eventArguments) =>
                AddDiagnostic(diagnostics, eventArguments, options);

            try
            {
                var source = document.ToXml();
                using (var reader = XmlReader.Create(new StringReader(source), settings, document.DocumentUri))
                {
                    while (reader.Read())
                    {
                    }
                }
            }
            catch (XsdFailFastException)
            {
            }
            catch (XmlSchemaException exception)
            {
                AddException(diagnostics, exception);
            }
            catch (XmlException exception)
            {
                diagnostics.Add(new XsdValidationDiagnostic(
                    XsdValidationSeverity.Error,
                    exception.Message,
                    exception.SourceUri,
                    exception.LineNumber,
                    exception.LinePosition));
            }

            return new XsdValidationResult(diagnostics);
        }

        private static XmlReaderSettings CreateSchemaReaderSettings(XsdValidationOptions options) => new XmlReaderSettings
        {
            DtdProcessing = DtdProcessing.Prohibit,
            XmlResolver = options.SchemaResolver,
        };

        private static void AddDiagnostic(List<XsdValidationDiagnostic> diagnostics, ValidationEventArgs eventArguments, XsdValidationOptions options)
        {
            var severity = eventArguments.Severity == XmlSeverityType.Warning ?
                XsdValidationSeverity.Warning :
                XsdValidationSeverity.Error;

            if (severity == XsdValidationSeverity.Warning && !options.IsReportingWarnings)
            {
                return;
            }

            var exception = eventArguments.Exception;
            diagnostics.Add(new XsdValidationDiagnostic(
                severity,
                eventArguments.Message,
                exception?.SourceUri,
                exception?.LineNumber ?? 0,
                exception?.LinePosition ?? 0));

            if (severity == XsdValidationSeverity.Error && options.IsFailFast)
            {
                throw new XsdFailFastException();
            }
        }

        private static void AddException(List<XsdValidationDiagnostic> diagnostics, XmlSchemaException exception)
        {
            diagnostics.Add(new XsdValidationDiagnostic(
                XsdValidationSeverity.Error,
                exception.Message,
                exception.SourceUri,
                exception.LineNumber,
                exception.LinePosition));
        }

            private static void AddException(List<XsdValidationDiagnostic> diagnostics, XmlException exception)
            {
                diagnostics.Add(new XsdValidationDiagnostic(
                XsdValidationSeverity.Error,
                exception.Message,
                exception.SourceUri,
                exception.LineNumber,
                exception.LinePosition));
            }

        sealed class XsdFailFastException : Exception
        {
        }
    }
}