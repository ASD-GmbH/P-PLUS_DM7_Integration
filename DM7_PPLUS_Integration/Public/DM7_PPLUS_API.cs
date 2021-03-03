using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using DM7_PPLUS_Integration.Daten;

namespace DM7_PPLUS_Integration
{
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
}
