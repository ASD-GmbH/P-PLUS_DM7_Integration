using System;

namespace DM7_PPLUS_Integration.Daten
{
    /// <summary>
    /// Kontaktdaten einer Person oder Organisation
    /// </summary>
    public readonly struct Kontakt
    {
        public Kontakt(Guid kontaktart, Guid kontaktform, string eintrag, bool hauptkontakt)
        {
            Kontaktart = kontaktart;
            Kontaktform = kontaktform;
            Eintrag = eintrag;
            Hauptkontakt = hauptkontakt;
        }

        /// <summary>
        /// Kontaktart, z.B. Telefon, Email, Fax, ...
        /// </summary>
        public readonly Guid Kontaktart;

        /// <summary>
        /// Kontaktform, z.B. Privat, Geschaeftlich, Bereitschaft, ...
        /// </summary>
        public readonly Guid Kontaktform;

        /// <summary>
        /// Eintrag, z.B. 04474-94800, info@dm-edv.de, ...
        /// </summary>
        public readonly string Eintrag;

        /// <summary>
        /// Gibt an, ob dieser Kontakt der hinterlegte Hauptkontakt ist
        /// </summary>
        public readonly bool Hauptkontakt;
    }
}