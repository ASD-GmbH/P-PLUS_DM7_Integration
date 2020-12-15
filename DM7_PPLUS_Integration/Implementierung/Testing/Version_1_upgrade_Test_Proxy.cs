using System;
using System.Threading.Tasks;
using DM7_PPLUS_Integration.Daten;
using DM7_PPLUS_Integration.Implementierung.Protokoll;

namespace DM7_PPLUS_Integration.Implementierung.Testing
{
    internal class Version_1_upgrade_Test_Proxy : DM7_PPLUS_API
    {
        public Version_1_upgrade_Test_Proxy(Schicht_2_Protokoll__API_Version_unabhaengige_Uebertragung resultItem1)
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