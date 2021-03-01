using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using DM7_PPLUS_Integration.Daten;

namespace DM7_PPLUS_Integration
{
    public interface Soll_Ist_Abgleich_Verarbeitungsergebnis {}

    public readonly struct Verarbeitet : Soll_Ist_Abgleich_Verarbeitungsergebnis {}

    public readonly struct Dienstplanabschluss_verhindert_Verarbeitung : Soll_Ist_Abgleich_Verarbeitungsergebnis
    {
        public readonly ReadOnlyCollection<Dienstplanabschluss> Dienstplanabschlüsse;

        public Dienstplanabschluss_verhindert_Verarbeitung(ReadOnlyCollection<Dienstplanabschluss> dienstplanabschlüsse)
        {
            Dienstplanabschlüsse = dienstplanabschlüsse;
        }
    }

    public readonly struct Dienstplanabschluss
    {
        public readonly Guid MitarbeiterId;
        public readonly Guid MandantId;
        public readonly Datum Datum;

        public Dienstplanabschluss(Guid mitarbeiterId, Guid mandantId, Datum datum)
        {
            MitarbeiterId = mitarbeiterId;
            MandantId = mandantId;
            Datum = datum;
        }
    }

    public interface DM7_PPLUS_API
    {
        /// <summary>
        /// Vom Server verwendete Version der GUID-basierten Merkmale
        /// </summary>
        int Auswahllisten_Version { get; }

        /// <summary>
        /// Abruf aller Mitarbeiterdatensätze
        /// </summary>
        Task<Stammdaten<Mitarbeiter>> Mitarbeiter_abrufen();

        /// <summary>
        /// Abruf der Mitarbeiterdatensätze die seit dem angegebenen Datenstand aktualisiert wurden
        /// </summary>
        Task<Stammdaten<Mitarbeiter>> Mitarbeiter_abrufen_ab(Datenstand stand);

        /// <summary>
        /// Abruf nur der Dienste mit Aktualisierungen seit dem angegebenen Datenstand
        /// </summary>
        Task<Stammdaten<Dienst>> Dienste_abrufen_ab(Datenstand stand);

        /// <summary>
        /// Abruf aller Dienste
        /// </summary>
        Task<Stammdaten<Dienst>> Dienste_abrufen();

        /// <summary>
        /// Abruf aller Dienstbuchungen gültig zum gegebenen Stichtag für den gegebenen Mandanten (mandantId)
        /// <param name="stichtag">Datum zu dem die Dienstbuchung beginnt</param>
        /// <param name="mandantId">MandantId in der die Dienstbuchung geplant ist</param>
        /// </summary>
        Task<ReadOnlyCollection<Dienstbuchung>> Dienstbuchungen_zum_Stichtag(Datum stichtag, Guid mandantId);

        /// <summary>
        /// Abruf aller Abwesenheiten für Mitarbeiter am Stichtag relevant für den gegebenen Mandanten (mandantId)
        /// </summary>
        /// <param name="stichtag"></param>
        /// <param name="mandantId">MandantId für den die Abwesenheiten relevant sind</param>
        /// <returns></returns>
        Task<ReadOnlyCollection<Abwesenheit>> Abwesenheiten_zum_Stichtag(Datum stichtag, Guid mandantId);

        // Entscheidung fällt am 02.03. von Willenborg & Fischer
        //Task<bool> Dienstplan_abgeschlossen_am(Datum datum);

        /// <summary>
        /// Dienstplan über die Freigabe des Soll/Ist Abgleichs informieren, um die Daten in den Dienstplan einzupflegen
        /// </summary>
        /// <param name="abgleich">Die Daten von dem Soll/Ist Abgleich</param>
        /// <returns>Ergebnis von der Verarbeitung seitens P-PLUS</returns>
        Task<Soll_Ist_Abgleich_Verarbeitungsergebnis> Soll_Ist_Abgleich_freigeben(Soll_Ist_Abgleich abgleich);
    }

    public readonly struct Soll_Ist_Abgleich
    {
        // public readonly Datum // ggf. nötig für wieder aufheben von Freigaben. Warten auf Ergebnis von Crone/Willenborg
        public readonly ReadOnlyCollection<Ungeplante_Tour> Ungeplante_Touren_ohne_Tourenstamm;
        public readonly ReadOnlyCollection<Geplante_Tour> Geplante_Touren;
        public readonly ReadOnlyCollection<Nicht_gefahrene_Tour> Nicht_gefahrene_Touren; // Wird noch mit Dennis und Anke/Fischer geklärt

        public Soll_Ist_Abgleich(ReadOnlyCollection<Ungeplante_Tour> ungeplante_Touren_ohne_Tourenstamm, ReadOnlyCollection<Geplante_Tour> geplante_Touren, ReadOnlyCollection<Nicht_gefahrene_Tour> nicht_gefahrene_Touren)
        {
            Ungeplante_Touren_ohne_Tourenstamm = ungeplante_Touren_ohne_Tourenstamm;
            Geplante_Touren = geplante_Touren;
            Nicht_gefahrene_Touren = nicht_gefahrene_Touren; // Wird noch mit Dennis und Anke/Fischer geklärt
        }
    }

    public readonly struct Ungeplante_Tour
    {
        public readonly Guid MitarbeiterId;
        public readonly Guid MandantId;
        public readonly string Bezeichnung; // Welche Rolle spielt die Bezeichnung in P-PLUS?
        public readonly Zeitpunkt Beginn; // Ergibt sich die Beginnzeit aus dem Beginn des ersten Einsatzes?
        public readonly ReadOnlyCollection<Einsatz> Einsätze;
        public readonly ReadOnlyCollection<Pause> Pausen;

        public Ungeplante_Tour(Guid mitarbeiterId, Guid mandantId, string bezeichnung, Zeitpunkt beginn, ReadOnlyCollection<Einsatz> einsätze, ReadOnlyCollection<Pause> pausen)
        {
            MitarbeiterId = mitarbeiterId;
            MandantId = mandantId;
            Bezeichnung = bezeichnung;
            Beginn = beginn;
            Einsätze = einsätze;
            Pausen = pausen;
        }
    }

    public readonly struct Geplante_Tour
    {
        public readonly Guid Mitarbeiter;
        public readonly Guid Mandant;
        public readonly int Dienst;
        public readonly Zeitpunkt Beginn; // Ergibt sich die Beginnzeit aus dem Beginn des ersten Einsatzes?
        public readonly ReadOnlyCollection<Einsatz> Einsätze;
        public readonly ReadOnlyCollection<Pause> Pausen;

        public Geplante_Tour(Guid mitarbeiter, Guid mandant, int dienst, Zeitpunkt beginn, ReadOnlyCollection<Einsatz> einsätze, ReadOnlyCollection<Pause> pausen)
        {
            Mitarbeiter = mitarbeiter;
            Mandant = mandant;
            Dienst = dienst;
            Beginn = beginn;
            Einsätze = einsätze;
            Pausen = pausen;
        }
    }

    public readonly struct Nicht_gefahrene_Tour
    {
        public readonly Guid Mitarbeiter;
        public readonly Guid Mandant;
        public readonly int Dienst;
        public readonly Datum Datum;

        public Nicht_gefahrene_Tour(Guid mitarbeiter, Guid mandant, int dienst, Datum datum)
        {
            Mitarbeiter = mitarbeiter;
            Mandant = mandant;
            Dienst = dienst;
            Datum = datum;
        }
    }


    // Klient-Einsatz hat auf DM7 Site Leistngen, die für PPLUS alle als Pflege gelten und zusammengefasst werden.
    // oder Sonstige Zeit
    //    Je nach Art: Pause oder andere, sonstige Zeiten (z.B. Tanken, Apothekenbesuch etc.)
    // oder Unterbrechung

    // abstract class Einsatz
    //  Anfahrt
    //  Abfahrt

    // class KlientEinsatz : Einsatz

    // class Sonstige_Zeit : Einsatz
    //       public guid Leistung

    // class Pause : Einsatz

    // class Unterbrechung : Einsatz

    public readonly struct Einsatz
    {

        public readonly Guid Leistung;
        public readonly Relative_Zeit Beginn;
        public readonly uint Dauer_in_Minuten;
        public readonly uint Anfahrtsdauer_in_Minuten;
        public readonly uint Abfahrtsdauer_in_Minuten;

        public Einsatz(Guid leistung, Relative_Zeit beginn, uint dauer_in_Minuten, uint anfahrtsdauer_in_Minuten, uint abfahrtsdauer_in_Minuten)
        {
            Leistung = leistung;
            Beginn = beginn;
            Dauer_in_Minuten = dauer_in_Minuten;
            Anfahrtsdauer_in_Minuten = anfahrtsdauer_in_Minuten;
            Abfahrtsdauer_in_Minuten = abfahrtsdauer_in_Minuten;
        }
    }

    public readonly struct Pause
    {
        public readonly Relative_Zeit Beginn;
        public readonly uint Dauer_in_Minuten;
        public readonly uint Anfahrtsdauer_in_Minuten;
        public readonly uint Abfahrtsdauer_in_Minuten;

        public Pause(Relative_Zeit beginn, uint dauer_in_Minuten, uint anfahrtsdauer_in_Minuten, uint abfahrtsdauer_in_Minuten)
        {
            Beginn = beginn;
            Dauer_in_Minuten = dauer_in_Minuten;
            Anfahrtsdauer_in_Minuten = anfahrtsdauer_in_Minuten;
            Abfahrtsdauer_in_Minuten = abfahrtsdauer_in_Minuten;
        }
    }

    public readonly struct Relative_Zeit
    {
        public readonly Uhrzeit Zeit;
        public readonly bool Folgetag;

        public Relative_Zeit(Uhrzeit zeit, bool folgetag)
        {
            Zeit = zeit;
            Folgetag = folgetag;
        }
    }
}
