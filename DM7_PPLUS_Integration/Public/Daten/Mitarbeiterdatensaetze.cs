using System;
using System.Collections.ObjectModel;

namespace DM7_PPLUS_Integration.Daten
{
    public struct Mitarbeiterdatensaetze
    {
        public Mitarbeiterdatensaetze(bool teilmenge, Stand stand, ReadOnlyCollection<Mitarbeiterdatensatz> mitarbeiter, ReadOnlyCollection<Mitarbeiterfoto> fotos)
        {
            Teilmenge = teilmenge;
            Stand = stand;
            Mitarbeiter = mitarbeiter;
            Fotos = fotos;
        }

        /// <summary>
        /// false, falls alle Mitarbeiterdatensätze übertragen wurden; true falls aufgrund der Aktualisierungsstände nur eine Teilmenge übertragen wurde
        /// </summary>
        public readonly bool Teilmenge;

        /// <summary>
        /// Aktualisierungsstand der übertragenen Mitarbeiterdatensätze
        /// </summary>
        public readonly Stand Stand;

        /// <summary>
        /// Mitarbeiterdatensätze, (falls Teilmenge==true: die sich zwischen den angeforderten Aktualisierungsständen geändert haben; falls Teilmenge==false: alle Mitarbeiterdatensätze)
        /// </summary>
        public readonly ReadOnlyCollection<Mitarbeiterdatensatz> Mitarbeiter;

        /// <summary>
        /// Fotos der Mitarbeiter, (falls Teilmenge==true: die sich zwischen den angeforderten Aktualisierungsständen geändert haben; falls Teilmenge==false: alle Mitarbeiterfotos)
        /// </summary>
        public readonly ReadOnlyCollection<Mitarbeiterfoto> Fotos;
    }
}