namespace Serilog.Preparers.CorrelationIds
{
    /// <summary>
    /// Settings for <see cref="ICorrelationIdPreparer"/>.
    /// </summary>
    public class CorrelationIdPreparerOptions
    {
        /// <summary>
        /// Determines whether to add a new correlation ID value if the header is absent.
        /// </summary>
        public bool AddValueIfHeaderAbsence { get; }

        /// <summary>
        /// The header key used to retrieve the correlation ID from the HTTP request or response headers.
        /// </summary>
        public string HeaderKey { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="CorrelationIdPreparerOptions" /> class.
        /// </summary>
        /// <param name="addValueIfHeaderAbsence">Determines whether to add a new correlation ID value if the header is absent.</param>
        /// <param name="headerKey">The header key used to retrieve the correlation ID from the HTTP request or response headers.</param>
        public CorrelationIdPreparerOptions(
            bool addValueIfHeaderAbsence,
            string headerKey)
        {
            AddValueIfHeaderAbsence = addValueIfHeaderAbsence;
            HeaderKey = headerKey;
        }
    }
}
