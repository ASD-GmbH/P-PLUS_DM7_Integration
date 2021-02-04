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
        IEnumerable<Mitarbeiter> Mitarbeiterdatensaetze_abrufen(IEnumerable<string> datensatz_ids);

        Stammdaten<Mitarbeiter> Mitarbeiter_abrufen();
        Stammdaten<Mitarbeiterfoto> Mitarbeiterfotos_abrufen();
        Stammdaten<Dienst> Dienste_abrufen();
        Stammdaten<Mitarbeiter> Mitarbeiter_abrufen_ab(Datenstand stand);
        Stammdaten<Mitarbeiterfoto> Mitarbeiterfotos_abrufen_ab(Datenstand stand);
        Stammdaten<Dienst> Dienste_abrufen_ab(Datenstand stand);
    }
}