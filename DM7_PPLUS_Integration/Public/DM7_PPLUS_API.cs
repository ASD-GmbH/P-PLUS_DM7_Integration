using System;
using System.Threading.Tasks;
using DM7_PPLUS_Integration.Daten;

namespace DM7_PPLUS_Integration
{
    /// <summary>
    /// Instanz der DM7_PPLUS_Integrationsschnittstelle, API Version 3
    /// </summary>
    public interface DM7_PPLUS_API : IDisposable
    {
        /// <summary>
        /// Vom Server verwendete Version der GUID-basierten Merkmale
        /// </summary>
        int Auswahllisten_Version { get; }

        /// <summary>
        /// Stream mit Aktualisierungsstand der Mitarbeiterdaten
        /// </summary>
        IObservable<Stand> Stand_Mitarbeiterdaten { get; }

        /// <summary>
        /// Abruf nur der Mitarbeiterdatensätze mit Aktualisierungen zwischen den angegschichtn Ständen
        /// </summary>
        Task<Mitarbeiterdatensaetze> Mitarbeiterdaten_abrufen(Stand von, Stand bis);

        /// <summary>
        /// Abruf aller Mitarbeiterdatensätze
        /// </summary>
        Task<Mitarbeiterdatensaetze> Mitarbeiterdaten_abrufen();
    }
}
