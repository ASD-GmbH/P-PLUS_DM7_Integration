using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Timers;
using DM7_PPLUS_Integration.Auswahllisten;
using DM7_PPLUS_Integration.Daten;
using DM7_PPLUS_Integration.Implementierung.Server;
using DM7_PPLUS_Integration.Implementierung.Shared;

namespace DM7_PPLUS_Integration.Implementierung.Testing
{
    public class Demo_Datenserver : PPLUS_Backend, IDisposable
    {
        private readonly Log _log;
        private readonly Random random = new Random();
        private readonly List<Mitarbeiter> mitarbeiter;
        private readonly Subject<IEnumerable<string>> _subject = new Subject<IEnumerable<string>>();
        private readonly Timer _timer;
        private int changeCounter;

        public Demo_Datenserver(Log log, TimeSpan interval)
        {
            _log = log;
            mitarbeiter =
                Enumerable.Range(0, 10)
                    .Select(_ =>
                        Neuer_Mitarbeiter())
                    .ToList();
            Aenderungen_an_Mitarbeiterstammdaten = _subject;

            _timer = new Timer(interval.TotalMilliseconds);
            _timer.Start();
            _timer.Elapsed += (s, e) =>
            {
                if (changeCounter++ <= 3)
                {
                    var selected = random.Next(0, mitarbeiter.Count - 1);
                    var ma_alt = mitarbeiter[selected];

                    var ma_neu = new Mitarbeiter(
                        ma_alt.DatensatzId,
                        ma_alt.PersonId,
                        ma_alt.ArbeitsverhaeltnisId,
                        ma_alt.Mandant,
                        ma_alt.Struktur,
                        ma_alt.Titel,
                        ma_alt.Vorname,
                        Nachname(),
                        ma_alt.Postanschrift,
                        ma_alt.Geburtstag,
                        ma_alt.Familienstand,
                        ma_alt.Konfession,
                        ma_alt.GueltigAb,
                        ma_alt.GueltigBis,
                        ma_alt.Qualifikation,
                        ma_alt.Handzeichen,
                        ma_alt.Personalnummer,
                        ma_alt.Geschlecht,
                        ma_alt.Kontakte);

                    _log.Debug($" - Ge�nderter Mitarbeiter im DemoServer: {ma_neu.Nachname}, {ma_neu.Vorname} (vormals: {ma_alt.Nachname}, {ma_neu.Vorname}) ({ma_neu.Personalnummer})");

                    mitarbeiter[selected] = ma_neu;
                    _subject.Next(new List<string> {ma_alt.DatensatzId});
                }
                else
                {
                    changeCounter = 0;
                    Trigger_Neue_Mitarbeiter();
                }
            };
        }

        private void Trigger_Neue_Mitarbeiter()
        {
            var neue_mitarbeiter = Enumerable.Range(0, random.Next(1,5)).Select(_ => Neuer_Mitarbeiter()).ToList();
            mitarbeiter.AddRange(neue_mitarbeiter);
            _subject.Next(neue_mitarbeiter.Select(_ => _.DatensatzId));
        }

        private Mitarbeiterdatensatz Neuer_Mitarbeiter()
        {
            var neuerMitarbeiter = new Mitarbeiterdatensatz(
                Guid.NewGuid().ToString(),
                Guid.NewGuid().ToString(),
                Guid.NewGuid().ToString(),
                1,
                Guid.NewGuid().ToString(),
                Auswahllisten_0.Titel.Kein,
                Vorname(),
                Nachname(),
                null,
                null,
                Auswahllisten_0.Familienstand.Unbekannt,
                Auswahllisten_0.Konfession.Keine,
                Zufaelliges_Datum(2000, 2015),
                null,
                new ReadOnlyCollection<Qualifikation>(new List<Qualifikation>()),
                "",
                Personalnummer(),
                Auswahllisten_0.Geschlecht.Maennlich,
                new ReadOnlyCollection<Kontakt>(new List<Kontakt>()));
            _log.Debug($" - Neuer Mitarbeiter im DemoServer: {neuerMitarbeiter.Nachname}, {neuerMitarbeiter.Vorname} ({neuerMitarbeiter.Personalnummer})");
            return
                neuerMitarbeiter;
        }

        private string Personalnummer() => random.Next(1000, 9999).ToString();
        private Datum Zufaelliges_Datum(int minJahr, int maxJahr) => new Datum(random.Next(1,28), random.Next(1,12), random.Next(minJahr, maxJahr));
        private Uhrzeit Zuf�llige_Uhrzeit() => Uhrzeit.HHMM(random.Next(0, 23), random.Next(0, 59));
        private bool Zuf�lliger_Wahrheitswert() => random.Next(0, 2) == 1;

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
            return mitarbeiter.Select(_ => _.DatensatzId);
        }

        public IEnumerable<Mitarbeiterdatensatz> Mitarbeiterdatensaetze_abrufen(IEnumerable<string> datensatz_ids) =>
            mitarbeiter.Where(_ => datensatz_ids.Contains(_.DatensatzId));

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
                Zuf�lliger_Wahrheitswert()
                    ? Zufaelliges_Datum(zufallstdatum.Jahr + 1, zufallstdatum.Jahr + 5)
                    : (Datum?) null,
                Zuf�llige_Uhrzeit(),
                Demo_G�ltigkeit(),
                Zuf�lliger_Wahrheitswert()
            );
        }

        private Dienst_G�ltigkeit Demo_G�ltigkeit()
        {
            return new Dienst_G�ltigkeit(
                Zuf�lliger_Wahrheitswert(),
                Zuf�lliger_Wahrheitswert(),
                Zuf�lliger_Wahrheitswert(),
                Zuf�lliger_Wahrheitswert(),
                Zuf�lliger_Wahrheitswert(),
                Zuf�lliger_Wahrheitswert(),
                Zuf�lliger_Wahrheitswert(),
                Zuf�lliger_Wahrheitswert()
            );
        }

        private (string, string) Dienst_Bezeichnung()
        {
            var bezeichnungen =
                new[]
                {
                    ("0800-1200", "Wohngr1 08.00 - 12.00"),
                    ("A", "Nachtbereitschaft"),
                    ("F", "Fr�hdienst"),
                    ("F1", "Pflege Fr�hdienst 1"),
                    ("F10", "Pflege Fr�hdienst 10"),
                    ("F18", "Fr�hdienst 18"),
                    ("F2", "Fr�htour 2"),
                    ("N", "Nachtdienst"),
                    ("NB", "Nacht mit Bereitschaft"),
                    ("NF", "Nachtdienst Fachkraft"),
                    ("S", "Sp�tdienst"),
                    ("S1", "Pflege Sp�tdienst"),
                    ("S2", "Pflege Sp�tdienst 2"),
                    ("Z", "Pflege Zwischendienst")
                };
            return bezeichnungen[random.Next(0, bezeichnungen.Length)];
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }
    }
}