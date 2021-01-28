using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace DM7_PPLUS_Integration.Daten
{
    /// <summary>
    /// Mitarbeiter für die Kommunikation zwischen ASD P-PLUS und DM7
    /// </summary>
    public struct Mitarbeiter // bisher Mitarbeiterdatensatz
    {
        public Mitarbeiter(Guid mitarbeiterId, ReadOnlyCollection<DM7_Mandantenzugehörigkeiten> mandantenzugehörigkeiten, string personalnummer, Guid titel, string vorname, string nachname, Postanschrift? postanschrift, string handzeichen, Datum? geburtstag, Guid geschlecht, Guid konfession, Guid familienstand, ReadOnlyCollection<Qualifikation> qualifikation, ReadOnlyCollection<Kontakt> kontakte)
        {
            Guard_Pflichtfelder(mandantenzugehörigkeiten, nachname, handzeichen, geschlecht);

            PPLUS_Id = mitarbeiterId; 
            DM7_Mandantenzugehörigkeiten = mandantenzugehörigkeiten; // Mindestens ein Eintrag vorhanden (ohne Typsicherheit)
            Personalnummer = personalnummer;
            Titel = titel;
            Vorname = vorname;
            Nachname = nachname; // pflicht
            Postanschrift = postanschrift;
            Handzeichen = handzeichen; // pflicht
            Kontakte = kontakte; 
            Geburtstag = geburtstag;
            Geschlecht = geschlecht; // pflicht
            Konfession = konfession;
            Familienstand = familienstand;
            Qualifikation = qualifikation; // Klären, wer die Datenhoheit hat
        }

        private static void Guard_Pflichtfelder(ReadOnlyCollection<DM7_Mandantenzugehörigkeiten> mandantenzugehörigkeiten, string nachname, string handzeichen, Guid geschlecht)
        {
            if (!mandantenzugehörigkeiten.Any()
                || string.IsNullOrWhiteSpace(nachname)
                || string.IsNullOrWhiteSpace(handzeichen)
                || geschlecht == Guid.Empty)
            {
                throw new ArgumentException("Pflichtfelder nicht eingehalten.");
            }
        }

        /// <summary>
        /// Primärschlüssel
        /// </summary>
        public readonly Guid PPLUS_Id;

        /// <summary>
        /// DM7 Mandanten, in P-PLUS repräsentiert als Strukturen auf denen der Mitarbeiter arbeitet.
        /// </summary>
        public readonly ReadOnlyCollection<DM7_Mandantenzugehörigkeiten> DM7_Mandantenzugehörigkeiten;

        /// <summary>
        /// Titel des Mitarbeiters, ergibt mit Geschlecht zusammen die Anrede
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