using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using DM7_PPLUS_Integration.Auswahllisten;
using DM7_PPLUS_Integration.Daten;
using DM7_PPLUS_Integration.Implementierung.Server;

namespace DM7_PPLUS_Integration_Specs
{
    public class Test_PPLUS_Backend : PPLUS_Backend
    {
        public Test_PPLUS_Backend(int auswahllistenVersion)
        {
            AuswahllistenVersion = auswahllistenVersion;
        }

        private readonly List<Mitarbeiterdatensatz> _mitarbeiter = new List<Mitarbeiterdatensatz>();
        private readonly Subject<IEnumerable<Guid>> _staende_stream = new Subject<IEnumerable<Guid>>();

        public int AuswahllistenVersion { get; }

        public IObservable<IEnumerable<Guid>> Aenderungen_an_Mitarbeiterstammdaten => _staende_stream;

        public IEnumerable<Guid> Alle_Mitarbeiter()
        {
            return _mitarbeiter.Select(_ => _.Id);
        }

        public IEnumerable<Mitarbeiterdatensatz> Mitarbeiterdatensaetze_abrufen(IEnumerable<Guid> mitarbeiter_ids)
        {
            return _mitarbeiter.Where(_ => mitarbeiter_ids.Contains(_.Id));
        }

        public void Mitarbeiter_hinzufuegen(string name, string nachname)
        {
            var neuerMitarbeiter = Neuer_Mitarbeiter(name, nachname);
            _mitarbeiter.Add(neuerMitarbeiter);
            _staende_stream.Next(new List<Guid> { neuerMitarbeiter.Id });
        }

        private Mitarbeiterdatensatz Neuer_Mitarbeiter(string name, string nachname)
        {
            return
                new Mitarbeiterdatensatz(
                    Guid.NewGuid(),
                    Auswahllisten_0.Titel.Kein,
                    name,
                    nachname,
                    new Postanschrift(Guid.NewGuid(), "Musterstraße 1", "D-28172", "Musterstadt", "Deutschland", "Hinterhaus"),
                    new Datum(16,12,1980),
                    Auswahllisten_0.Familienstand.Unbekannt,
                    Auswahllisten_0.Konfession.Keine,
                    new Datum(1,1,2017),
                    new Datum(31,12,2018),
                    new ReadOnlyCollection<Qualifikation>(new List<Qualifikation>()),
                    "Handzeichen",
                    "1",
                    Auswahllisten_0.Geschlecht.Maennlich,
                    new ReadOnlyCollection<Kontakt>(new List<Kontakt>()));
        }
    }
}