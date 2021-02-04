using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using DM7_PPLUS_Integration.Auswahllisten;
using DM7_PPLUS_Integration.Daten;
using DM7_PPLUS_Integration.Implementierung.Server;


namespace DM7_PPLUS_Integration.Implementierung.Testing
{
    public class Demo_Datenserver : PPLUS_Backend, IDisposable
    {
        private readonly Log _log;
        private readonly Random random = new Random();
        private readonly List<Mitarbeiter> mitarbeiter;

        public Demo_Datenserver(Log log, TimeSpan interval)
        {
            _log = log;
            mitarbeiter =
                Enumerable.Range(0, 10)
                    .Select(_ =>
                        Neuer_Mitarbeiter())
                    .ToList();
        }


        private Mitarbeiter Neuer_Mitarbeiter()
        {
            var neuerMitarbeiter = new Mitarbeiter(
                Guid.NewGuid(),
                new ReadOnlyCollection<DM7_Mandantenzugehörigkeiten>(new List<DM7_Mandantenzugehörigkeiten> { new DM7_Mandantenzugehörigkeiten(1, Zufaelliges_Datum(2014, 2016), null)}),
                Personalnummer(),
                Auswahllisten_0.Titel.Kein,
                Vorname(),
                Nachname(),
                null,
                "HEI",
                Zufaelliges_Datum(2000, 2015),
                Auswahllisten_0.Geschlecht.Maennlich,
                Auswahllisten_0.Konfession.Keine,
                Auswahllisten_0.Familienstand.Unbekannt,
                new ReadOnlyCollection<Qualifikation>(new List<Qualifikation>()),
                new ReadOnlyCollection<Kontakt>(new List<Kontakt>()));
            _log.Debug($" - Neuer Mitarbeiter im DemoServer: {neuerMitarbeiter.Nachname}, {neuerMitarbeiter.Vorname} ({neuerMitarbeiter.Personalnummer})");
            return
                neuerMitarbeiter;
        }

        private string Personalnummer() => random.Next(1000, 9999).ToString();
        private Datum Zufaelliges_Datum(int minJahr, int maxJahr) => new Datum(random.Next(1,28), random.Next(1,12), random.Next(minJahr, maxJahr));
        private Uhrzeit Zufällige_Uhrzeit() => Uhrzeit.HHMM(random.Next(0, 23), random.Next(0, 59));
        private bool Zufälliger_Wahrheitswert() => random.Next(0, 2) == 1;

        private string Nachname()
        {
            var namen = "Lane,Dane,Alonso,Nigel,Harlan,Benjamin,Rogelio,Archie,Maynard,Ulysses,Fritz,Tod,Cesar,Abe,Loren,Francis,Nicholas,Lorenzo,Wilbur,Hector".Split(',');
            return namen[random.Next(0, namen.Length)];
        }

        private string Vorname()
        {
            var namen = "David,Boyd,Ollie,Hilton,Francesco,Hugh,Milford,Kasey,Rupert,King,Huey,Von,Roscoe,Dino,Warner,Stewart,Peter,Dannie,Edgar,Eusebio".Split(',');
            return namen[random.Next(0, namen.Length)];
        }

        public int AuswahllistenVersion => 0;
        public IObservable<IEnumerable<string>> Aenderungen_an_Mitarbeiterstammdaten { get; }

        public IEnumerable<string> Alle_Mitarbeiterdatensaetze()
        {
            return mitarbeiter.Select(_ => _.PPLUS_Id.ToString());
        }


        IEnumerable<Mitarbeiter> PPLUS_Backend.Mitarbeiterdatensaetze_abrufen(IEnumerable<string> datensatz_ids)
        {
            throw new NotImplementedException();
        }

        public Stammdaten<Mitarbeiter> Mitarbeiter_abrufen()
        {
            throw new NotImplementedException();
        }

        public Stammdaten<Mitarbeiterfoto> Mitarbeiterfotos_abrufen()
        {
            throw new NotImplementedException();
        }

        Stammdaten<Dienst> PPLUS_Backend.Dienste_abrufen()
        {
            throw new NotImplementedException();
        }

        public Stammdaten<Mitarbeiter> Mitarbeiter_abrufen_ab(Datenstand stand)
        {
            throw new NotImplementedException();
        }

        public Stammdaten<Mitarbeiterfoto> Mitarbeiterfotos_abrufen_ab(Datenstand stand)
        {
            throw new NotImplementedException();
        }

        public Stammdaten<Dienst> Dienste_abrufen_ab(Datenstand stand)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<Mitarbeiter> Mitarbeiterdatensaetze_abrufen(IEnumerable<Guid> pplus_ids) =>
            mitarbeiter.Where(_ => pplus_ids.Contains(_.PPLUS_Id));

        public IEnumerable<Dienst> Dienste_abrufen()
        {
            return Enumerable.Range(1, random.Next(1, 20)).Select(Neuer_Demo_Dienst);
        }

        private Dienst Neuer_Demo_Dienst(int id)
        {
            var (kurzbezeichnung, bezeichnung) = Dienst_Bezeichnung();
            var zufallstdatum = Zufaelliges_Datum(2010, 2021);
            return new Dienst(
                id,
                Guid.NewGuid(),
                kurzbezeichnung,
                bezeichnung,
                zufallstdatum,
                Zufälliger_Wahrheitswert()
                    ? Zufaelliges_Datum(zufallstdatum.Jahr + 1, zufallstdatum.Jahr + 5)
                    : (Datum?) null,
                Zufällige_Uhrzeit(),
                Demo_Gültigkeit(),
                Zufälliger_Wahrheitswert()
            );
        }

        private Dienst_Gültigkeit Demo_Gültigkeit()
        {
            return new Dienst_Gültigkeit(
                Zufälliger_Wahrheitswert(),
                Zufälliger_Wahrheitswert(),
                Zufälliger_Wahrheitswert(),
                Zufälliger_Wahrheitswert(),
                Zufälliger_Wahrheitswert(),
                Zufälliger_Wahrheitswert(),
                Zufälliger_Wahrheitswert(),
                Zufälliger_Wahrheitswert()
            );
        }

        private (string, string) Dienst_Bezeichnung()
        {
            var bezeichnungen =
                new[]
                {
                    ("0800-1200", "Wohngr1 08.00 - 12.00"),
                    ("A", "Nachtbereitschaft"),
                    ("F", "Frühdienst"),
                    ("F1", "Pflege Frühdienst 1"),
                    ("F10", "Pflege Frühdienst 10"),
                    ("F18", "Frühdienst 18"),
                    ("F2", "Frühtour 2"),
                    ("N", "Nachtdienst"),
                    ("NB", "Nacht mit Bereitschaft"),
                    ("NF", "Nachtdienst Fachkraft"),
                    ("S", "Spätdienst"),
                    ("S1", "Pflege Spätdienst"),
                    ("S2", "Pflege Spätdienst 2"),
                    ("Z", "Pflege Zwischendienst")
                };
            return bezeichnungen[random.Next(0, bezeichnungen.Length)];
        }
    }
}