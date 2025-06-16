using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using DM7_PPLUS_Integration.Daten;

namespace DM7_PPLUS_Integration
{
    public interface PPLUS_API : IDisposable
    {
        /// <summary>
        /// Vom Server verwendete Version der GUID-basierten Merkmale
        /// </summary>
        int Auswahllisten_Version { get; }

        /// <summary>
        /// Meldet wenn auf P-PLUS Seite geänderte oder neue Mitarbeiter vorliegen
        /// </summary>
        event Action Mitarbeiteränderungen_liegen_bereit;

        /// <summary>
        /// Meldet wenn auf P-PLUS Seite geänderte oder neue Dienste vorliegen
        /// </summary>
        event Action Dienständerungen_liegen_bereit;

        /// <summary>
        /// Meldet wenn auf P-PLUS Seite geänderte oder neue Dienste vorliegen
        /// </summary>
        event Action Dienstbuchungsänderungen_liegen_bereit;

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
        /// Ermittelt den Beginn des Dienstes für den gegebenen Stichtag
        /// </summary>
        /// <returns>Beginn des Dienstes</returns>
        Task<Uhrzeit?> Dienstbeginn_am(Datum stichtag, int dienstId);

        /// <summary>
        /// Abruf aller Dienstbuchungen gültig im gegebenen Zeitraum für den gegebenen Mandanten (mandantId)
        /// <param name="von">Startdatum des Zeitraums</param>
        /// <param name="bis">Statusende des Zeitraums</param>
        /// <param name="mandantId">MandantId in der die Dienstbuchung geplant ist</param>
        /// </summary>
        Task<Dictionary<Datum, ReadOnlyCollection<Dienstbuchung>>> Dienstbuchungen_im_Zeitraum(Datum von, Datum bis, Guid mandantId);

        /// <summary>
        /// Abruf aller Abwesenheiten für Mitarbeiter im Zeitraum relevant für den gegebenen Mandanten (mandantId)
        /// </summary>
        /// <param name="von">Startdatum des Zeitraums</param>
        /// <param name="bis">Statusende des Zeitraums</param>
        /// <param name="mandantId">MandantId für den die Abwesenheiten relevant sind</param>
        /// <returns></returns>
        Task<ReadOnlyCollection<Abwesenheit>> Abwesenheiten_im_Zeitraum(Datum von, Datum bis, Guid mandantId);

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
