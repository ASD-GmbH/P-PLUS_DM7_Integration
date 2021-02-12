using System;
using System.Threading.Tasks;
using DM7_PPLUS_Integration.Daten;

namespace DM7_PPLUS_Integration
{
    public interface DM7_PPLUS_API : IDisposable
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
        /// Abruf aller Mitarbeiterfotos
        /// </summary>
        /// <returns></returns>
        Task<Stammdaten<Mitarbeiterfoto>> Mitarbeiterfotos_abrufen();

        /// <summary>
        /// Abruf nur der Mitarbeiterfotos mit Aktualisierungen seit dem angegebenen Datenstand
        /// </summary>
        Task<Stammdaten<Mitarbeiterfoto>> Mitarbeiterfotos_abrufen_ab(Datenstand stand);

        /// <summary>
        /// Abruf nur der Dienste mit Aktualisierungen seit dem angegebenen Datenstand
        /// </summary>
        Task<Stammdaten<Dienst>> Dienste_abrufen_ab(Datenstand stand);

        /// <summary>
        /// Abruf aller Dienste
        /// </summary>
        Task<Stammdaten<Dienst>> Dienste_abrufen();
    }
}
