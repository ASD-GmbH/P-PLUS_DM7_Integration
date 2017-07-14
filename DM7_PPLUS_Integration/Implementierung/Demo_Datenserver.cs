using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Timers;
using DM7_PPLUS_Integration.Auswahllisten;
using DM7_PPLUS_Integration.Daten;

namespace DM7_PPLUS_Integration.Implementierung
{
    public class Demo_Datenserver : PPLUS_Backend
    {
        private readonly Random random = new Random();
        private readonly List<Mitarbeiterdatensatz> mitarbeiter;
        private readonly Subject<IEnumerable<Guid>> _subject = new Subject<IEnumerable<Guid>>();
        private Timer _timer;
        private int changeCounter = 0;

        public Demo_Datenserver()
        {
            mitarbeiter =
                Enumerable.Range(0, 10)
                    .Select(_ =>
                        Neuer_Mitarbeiter())
                    .ToList();
            Aenderungen_an_Mitarbeiterstammdaten = _subject;

            _timer = new Timer(TimeSpan.FromSeconds(1).TotalMilliseconds);
            _timer.Start();
            _timer.Elapsed += (s, e) =>
            {
                if (changeCounter++ <= 3)
                {
                    var selected = random.Next(0, mitarbeiter.Count - 1);
                    var ma = mitarbeiter[selected];
                    mitarbeiter[selected] = new Mitarbeiterdatensatz(
                        ma.Id,
                        ma.Titel,
                        ma.Vorname,
                        Nachname(),
                        ma.Postanschrift,
                        ma.Geburtstag,
                        ma.Familienstand,
                        ma.Konfession,
                        ma.Eintritt,
                        ma.Austritt,
                        ma.Qualifikation,
                        ma.Handzeichen,
                        ma.Personalnummer,
                        ma.Geschlecht,
                        ma.Kontakte);
                    _subject.Next(new List<Guid> {ma.Id});
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
            return
                new Mitarbeiterdatensatz(
                    Guid.NewGuid(),
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

        public IObservable<IEnumerable<Guid>> Aenderungen_an_Mitarbeiterstammdaten { get; }

        public IEnumerable<Guid> Alle_Mitarbeiter()
        {
            return mitarbeiter.Select(_ => _.Id);
        }

        public IEnumerable<Mitarbeiterdatensatz> Mitarbeiterdatensaetze_abrufen(IEnumerable<Guid> mitarbeiter_ids) =>
            mitarbeiter.Where(_ => mitarbeiter_ids.Contains(_.Id));
    }
}