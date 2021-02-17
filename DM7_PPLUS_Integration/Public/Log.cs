namespace DM7_PPLUS_Integration
{
    public interface Log
    {
        /// <summary>
        /// Statusmeldungen geeignet zur Anzeige
        /// </summary>
        void Info(string text);

        /// <summary>
        /// Informationsmeldungen nur für Kundendienst
        /// </summary>
        void Debug(string text);
    }
}