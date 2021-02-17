using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace DM7_PPLUS_Integration
{
    public class Stammdaten<T> : ReadOnlyCollection<T>
    {
        public readonly Datenstand Stand;

        public Stammdaten(IList<T> daten, Datenstand stand) : base(daten)
        {
            Stand = stand;
        }
    }
}