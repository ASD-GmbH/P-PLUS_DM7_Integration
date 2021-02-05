using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using DM7_PPLUS_Integration.Daten;
using DM7_PPLUS_Integration.Implementierung.Protokoll;

namespace DM7_PPLUS_Integration.Implementierung.Testing
{
    internal class Version_1_upgrade_Test_Proxy : DM7_PPLUS_API
    {
        public Version_1_upgrade_Test_Proxy(Schicht_2_Protokoll__API_Version_unabhaengige_Uebertragung _)
        {
            Auswahllisten_Version = 4711;
            Stand_Mitarbeiterdaten = null;
        }

        public void Dispose()
        {
        }

        public int Auswahllisten_Version { get; }
        public Task<Stammdaten<Mitarbeiter>> Mitarbeiter_abrufen()
        {
            return null;
        }

        public Task<Stammdaten<Mitarbeiter>> Mitarbeiter_abrufen_ab(Datenstand stand)
        {
            return null;
        }

        public Task<Stammdaten<Mitarbeiterfoto>> Mitarbeiterfotos_abrufen()
        {
            return null;
        }

        public Task<Stammdaten<Mitarbeiterfoto>> Mitarbeiterfotos_abrufen_ab(Datenstand stand)
        {
            return null;
        }

        public Task<Stammdaten<Dienst>> Dienste_abrufen_ab(Datenstand stand)
        {
            return null;
        }

        Task<Stammdaten<Dienst>> DM7_PPLUS_API.Dienste_abrufen()
        {
            return null;
        }

        public IObservable<Stand> Stand_Mitarbeiterdaten { get; }
        public Task<Mitarbeiterdatensaetze> Mitarbeiterdaten_abrufen(Stand von, Stand bis)
        {
            return null;
        }

        public Task<Mitarbeiterdatensaetze> Mitarbeiterdaten_abrufen()
        {
            return null;
        }

        public Task<ReadOnlyCollection<Dienst>> Dienste_abrufen()
        {
            return null;
        }
    }
}