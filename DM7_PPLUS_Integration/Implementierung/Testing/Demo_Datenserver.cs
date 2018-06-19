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
        private readonly List<Mitarbeiterdatensatz> mitarbeiter;
        private readonly Subject<IEnumerable<Guid>> _subject = new Subject<IEnumerable<Guid>>();
        private readonly Timer _timer;
        private int changeCounter = 0;

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

                    var ma_neu = new Mitarbeiterdatensatz(
                        ma_alt.Id,
                        ma_alt.Mandanten,
                        ma_alt.Titel,
                        ma_alt.Vorname,
                        Nachname(),
                        ma_alt.Postanschrift,
                        ma_alt.Geburtstag,
                        ma_alt.Familienstand,
                        ma_alt.Konfession,
                        ma_alt.Eintritt,
                        ma_alt.Austritt,
                        ma_alt.Qualifikation,
                        ma_alt.Handzeichen,
                        ma_alt.Personalnummer,
                        ma_alt.Geschlecht,
                        ma_alt.Kontakte);

                    _log.Debug($" - Geänderter Mitarbeiter im DemoServer: {ma_neu.Nachname}, {ma_neu.Vorname} (vormals: {ma_alt.Nachname}, {ma_neu.Vorname}) ({ma_neu.Personalnummer})");

                    mitarbeiter[selected] = ma_neu;
                    _subject.Next(new List<Guid> {ma_alt.Id});
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
            _subject.Next(neue_mitarbeiter.Select(_ => _.Id));
        }

        private Mitarbeiterdatensatz Neuer_Mitarbeiter()
        {
            var neuerMitarbeiter = new Mitarbeiterdatensatz(
                Guid.NewGuid(),
                new ReadOnlyCollection<int>(new List<int>{1}),
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
        public IObservable<IEnumerable<Guid>> Aenderungen_an_Mitarbeiterstammdaten { get; }

        public IEnumerable<Guid> Alle_Mitarbeiter()
        {
            return mitarbeiter.Select(_ => _.Id);
        }

        public IEnumerable<Mitarbeiterdatensatz> Mitarbeiterdatensaetze_abrufen(IEnumerable<Guid> mitarbeiter_ids) =>
            mitarbeiter.Where(_ => mitarbeiter_ids.Contains(_.Id));

        public void Dispose()
        {
            _timer?.Dispose();
        }
    }
}