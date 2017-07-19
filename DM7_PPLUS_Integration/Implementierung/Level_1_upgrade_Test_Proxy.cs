using System;
using System.Threading.Tasks;
using DM7_PPLUS_Integration.Daten;

namespace DM7_PPLUS_Integration.Implementierung
{
    internal class Level_1_upgrade_Test_Proxy : DM7_PPLUS_API
    {
        public Level_1_upgrade_Test_Proxy(Ebene_2_Protokoll__API_Level_unabhaengige_Uebertragung resultItem1)
        {
            Auswahllisten_Version = 4711;
        }

        public void Dispose()
        {
        }

        public int Auswahllisten_Version { get; }
        public IObservable<Stand> Stand_Mitarbeiterdaten { get; }
        public Task<Mitarbeiterdatensaetze> Mitarbeiterdaten_abrufen(Stand von, Stand bis)
        {
            return null;
        }

        public Task<Mitarbeiterdatensaetze> Mitarbeiterdaten_abrufen()
        {
            return null;
        }
    }
}