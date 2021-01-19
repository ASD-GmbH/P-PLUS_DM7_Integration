using System;
using System.Collections.Generic;
using DM7_PPLUS_Integration.Daten;

namespace DM7_PPLUS_Integration.Implementierung.Server
{
    public interface PPLUS_Backend
    {
        int AuswahllistenVersion { get; }
        IObservable<IEnumerable<string>> Aenderungen_an_Mitarbeiterstammdaten { get; }
        IEnumerable<string> Alle_Mitarbeiterdatensaetze();
        IEnumerable<Mitarbeiterdatensatz> Mitarbeiterdatensaetze_abrufen(IEnumerable<string> datensatz_ids);
        IEnumerable<Dienst> Dienste_abrufen();
    }
}