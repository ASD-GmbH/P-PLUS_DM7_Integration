using System;
using System.Collections.ObjectModel;

namespace DM7_PPLUS_Integration.Daten
{
    /// <summary>
    /// Mitarbeiter für die Kommunikation zwischen ASD P-PLUS und DM7
    /// </summary>
    public struct Mitarbeiterdatensatz
    {

        public Mitarbeiterdatensatz(string datensatzId, string personId, string arbeitsverhaeltnisId, int mandant, string struktur, Guid titel, string vorname, string nachname, Postanschrift? postanschrift, Datum? geburtstag, Guid familienstand, Guid konfession, Datum gueltigAb, Datum? gueltigBis, ReadOnlyCollection<Qualifikation> qualifikation, string handzeichen, string personalnummer, Guid geschlecht, ReadOnlyCollection<Kontakt> kontakte)
        {
            DatensatzId = datensatzId;
            PersonId = personId;
            ArbeitsverhaeltnisId = arbeitsverhaeltnisId;
            Mandant = mandant;
            Struktur = struktur;
            Titel = titel;
            Vorname = vorname;
            Nachname = nachname;
            Postanschrift = postanschrift;
            Geburtstag = geburtstag;
            Familienstand = familienstand;
            Konfession = konfession;
            GueltigAb = gueltigAb;
            GueltigBis = gueltigBis;
            Qualifikation = qualifikation;
            Handzeichen = handzeichen;
            Personalnummer = personalnummer;
            Geschlecht = geschlecht;
            Kontakte = kontakte;
        }

        /// <summary>
        /// Primärschlüssel
        /// </summary>
        public readonly string DatensatzId;

        /// <summary>
        /// Id des Mitarbeiters (P-PLUS Entität Person)
        /// </summary>
        public readonly string PersonId;

        /// <summary>
        /// Id des Arbeitsverhältnisses (P-PLUS Entität Mitarbeiter)
        /// </summary>
        public readonly string ArbeitsverhaeltnisId;

        /// <summary>
        /// DM7 Mandanten, in P-PLUS repräsentiert als Strukturen auf denen der Mitarbeiter arbeitet.
        /// </summary>
        public readonly int Mandant;

        /// <summary>
        /// Id der Struktur, für die der Datensatz gültig ist
        /// </summary>
        public readonly string Struktur;

        /// <summary>
        /// Titel des Mitarbeiters, muss ggf. in DM7 als Auswahllisteneintrag gepflegt werden
        /// </summary>
        public readonly Guid Titel;

        /// <summary>
        /// Vorname des Mitarbeiters
        /// </summary>
        public readonly string Vorname;

        /// <summary>
        /// Nachname des Mitarbeiters
        /// </summary>
        public readonly string Nachname;

        /// <summary>
        /// Postanschrift des Mitarbeiters
        /// </summary>
        public readonly Postanschrift? Postanschrift;

        /// <summary>
        /// Geburtstag des Mitarbeiters
        /// </summary>
        public readonly Datum? Geburtstag;

        /// <summary>
        /// Familienstand des Mitarbeiters, muss ggf. in DM7 als Auswahllisteneintrag gepflegt werden
        /// </summary>
        public readonly Guid Familienstand;

        /// <summary>
        /// Konfession des Mitarbeiters, muss ggf. in DM7 als Auswahllisteneintrag gepflegt werden
        /// </summary>
        public readonly Guid Konfession;

        /// <summary>
        /// Eintrittsdatum des Mitarbeiters
        /// </summary>
        public readonly Datum GueltigAb;

        /// <summary>
        /// Austrittsdatum des Mitarbeiters
        /// </summary>
        public readonly Datum? GueltigBis;

        /// <summary>
        /// Qualifikation des Mitarbeiters, muss ggf. in DM7 als Auswahllisteneintrag gepflegt werden
        /// </summary>
        public readonly ReadOnlyCollection<Qualifikation> Qualifikation;

        /// <summary>
        /// Handzeichen des Mitarbeiters
        /// </summary>
        public readonly string Handzeichen;

        /// <summary>
        /// Personalnummer des Mitarbeiters
        /// </summary>
        public readonly string Personalnummer;

        /// <summary>
        /// Geschlecht des Mitarbeiters, muss ggf. in DM7 als Auswahllisteneintrag gepflegt werden
        /// </summary>
        public readonly Guid Geschlecht;

        /// <summary>
        /// Kontaktmöglichkeiten des Mitarbeiters
        /// </summary>
        public readonly ReadOnlyCollection<Kontakt> Kontakte;
    }
}