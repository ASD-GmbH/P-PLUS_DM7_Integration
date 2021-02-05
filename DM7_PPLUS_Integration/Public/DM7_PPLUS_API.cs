using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using DM7_PPLUS_Integration.Daten;

namespace DM7_PPLUS_Integration
{
    public class Stammdaten<T> : ReadOnlyCollection<T>
    {
        public readonly Datenstand Stand;

        public Stammdaten(IList<T> list, Datenstand stand) : base(list)
        {
            Stand = stand;
        }
    }
 
    /// <summary>
    /// Instanz der DM7_PPLUS_Integrationsschnittstelle, API Version 4
    /// </summary>
    public interface DM7_PPLUS_API : IDisposable
    {
        [Obsolete]
        IObservable<Stand> Stand_Mitarbeiterdaten { get; }
        [Obsolete]
        Task<Mitarbeiterdatensaetze> Mitarbeiterdaten_abrufen(Stand von, Stand bis);
        [Obsolete]
        Task<Mitarbeiterdatensaetze> Mitarbeiterdaten_abrufen();

        // ^^^^^^^^^^
        // LEGACY
        // ^^^^^^^^^^


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

    public readonly struct Datenstand
    {
        public readonly ulong Value;

        public Datenstand(ulong value)
        {
            Value = value;
        }

        public string Serialisiert()
        {
            return Value.ToString();
        }

        public static Datenstand Aus_serialisiertem_Wert(string value)
        {
            return new Datenstand(ulong.Parse(value));
        }
    }
}
