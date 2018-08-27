using System;
using System.Collections.ObjectModel;

namespace DM7_PPLUS_Integration.Daten
{
    /// <summary>
    /// Mitarbeiter für die Kommunikation zwischen ASD P-PLUS und DM7
    /// </summary>
    public struct Mitarbeiterdatensatz
    {

        //ReadOnlyCollection<int> mandant,
        public Mitarbeiterdatensatz(string datensatzId, Guid personId, int mandant, Guid struktur, Guid titel, string vorname, string nachname, Postanschrift? postanschrift, Datum? geburtstag, Guid familienstand, Guid konfession, Datum eintritt, Datum? austritt, ReadOnlyCollection<Qualifikation> qualifikation, string handzeichen, string personalnummer, Guid geschlecht, ReadOnlyCollection<Kontakt> kontakte)
        {
            DatensatzId = datensatzId;
            PersonId = personId;
            Mandant = mandant;
            Struktur = struktur;
            Titel = titel;
            Vorname = vorname;
            Nachname = nachname;
            Postanschrift = postanschrift;
            Geburtstag = geburtstag;
            Familienstand = familienstand;
            Konfession = konfession;
            Eintritt = eintritt;
            Austritt = austritt;
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
        /// Primärschlüssel
        /// </summary>
        public readonly Guid PersonId;

        /// <summary>
        /// DM7 Mandanten, in P-PLUS repräsentiert als Strukturen auf denen der Mitarbeiter arbeitet.
        /// </summary>
        public readonly int Mandant;

        /// <summary>
        /// Struktur, für die der Datensatz gültig ist
        /// </summary>
        public readonly Guid Struktur;

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
        public readonly Datum Eintritt;

        /// <summary>
        /// Austrittsdatum des Mitarbeiters
        /// </summary>
        public readonly Datum? Austritt;

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