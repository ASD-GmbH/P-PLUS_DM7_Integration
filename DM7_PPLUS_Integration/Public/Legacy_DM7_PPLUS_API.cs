using System;
using System.Threading.Tasks;
using DM7_PPLUS_Integration.Daten;

namespace DM7_PPLUS_Integration
{
    /// <summary>
    /// </summary>
    public interface Legacy_DM7_PPLUS_API : IDisposable
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
        /// Abruf nur der Mitarbeiterdatensätze mit Aktualisierungen zwischen den angegebenen Ständen
        /// </summary>
        Task<Mitarbeiterdatensaetze> Mitarbeiterdaten_abrufen(Stand von, Stand bis);

        /// <summary>
        /// Abruf aller Mitarbeiterdatensätze
        /// </summary>
        Task<Mitarbeiterdatensaetze> Mitarbeiterdaten_abrufen();
    }
}