using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using DM7_PPLUS_Integration.Daten;
using DM7_PPLUS_Integration.Implementierung.Shared;

namespace DM7_PPLUS_Integration.Implementierung.Server
{
    /// <summary>
    /// Stellt die Version 1 der API zur Verfügung und erfüllt Anfragen nach Daten und Notifications
    /// </summary>
    internal sealed class API_Version_1_Adapter : DisposeGroupMember, DM7_PPLUS_API
    {
        private readonly PPLUS_Backend _backend;
        private readonly Guid _session;
        private long _stand;

        private readonly Dictionary<long, IEnumerable<string>> _Mitarbeiter_je_Stand =
            new Dictionary<long, IEnumerable<string>>();


        public API_Version_1_Adapter(PPLUS_Backend backend, Action<Exception> onException, Log log, IDisposable disposegroup) : base(disposegroup)
        {
            _session = Guid.NewGuid();
            log.Debug($"Server ID {_session.ToString()} ");
            _backend = backend;
            _Mitarbeiter_je_Stand.Add(_stand, _backend.Alle_Mitarbeiterdatensaetze());

            Stand_Mitarbeiterdaten = new Subject<Stand>();
            _backend.Aenderungen_an_Mitarbeiterstammdaten.Subscribe(
                new Observer<IEnumerable<string>>(
                    datensatz =>
                    {
                        _stand++;
                        _Mitarbeiter_je_Stand.Add(_stand, datensatz);
                        Announce();
                    },
                    onException));
        }

        public int Auswahllisten_Version => _backend.AuswahllistenVersion;

        public IObservable<Stand> Stand_Mitarbeiterdaten { get; }

        private VersionsStand Ab_Initio => new VersionsStand(_session, -1);
        private VersionsStand Aktueller_Stand => new VersionsStand(_session, _stand);

        public Task<Mitarbeiterdatensaetze> Mitarbeiterdaten_abrufen(Stand von, Stand bis)
        {
            var vvon = (VersionsStand) von;
            var vbis = (VersionsStand) bis;

            if (vvon.Session != vbis.Session || vvon.Session != _session)
            {
                vvon = Ab_Initio;
                vbis = Aktueller_Stand;
            }

            bool teilmenge = !vvon.Equals(Ab_Initio);

            var mitarbeiter =
                _Mitarbeiter_je_Stand
                    .Where(_ => !teilmenge || _.Key > vvon.Version && _.Key <= vbis.Version)
                    .SelectMany(_ => _.Value)
                    .Distinct();
            var task = new Task<Mitarbeiterdatensaetze>(() => new Mitarbeiterdatensaetze(
                teilmenge,
                new VersionsStand(_session, _stand),
                new ReadOnlyCollection<Mitarbeiter>(_backend.Mitarbeiterdatensaetze_abrufen(mitarbeiter).ToList()),
                new ReadOnlyCollection<Mitarbeiterfoto>(new List<Mitarbeiterfoto>())));
            task.RunSynchronously();
            return task;
        }

        public Task<Mitarbeiterdatensaetze> Mitarbeiterdaten_abrufen()
        {
            return Mitarbeiterdaten_abrufen(Ab_Initio, Aktueller_Stand);
        }

        public Task<ReadOnlyCollection<Dienst>> Dienste_abrufen()
        {
            throw new NotSupportedException();
        }

        public void Announce()
        {
            ((Subject<Stand>)Stand_Mitarbeiterdaten).Next(new VersionsStand(_session, _stand));
        }

        public Task<Stammdaten<Mitarbeiter>> Mitarbeiter_abrufen()
        {
            throw new NotImplementedException();
        }

        public Task<Stammdaten<Mitarbeiter>> Mitarbeiter_abrufen_ab(Datenstand stand)
        {
            throw new NotImplementedException();
        }

        public Task<Stammdaten<Mitarbeiterfoto>> Mitarbeiterfotos_abrufen()
        {
            throw new NotImplementedException();
        }

        public Task<Stammdaten<Mitarbeiterfoto>> Mitarbeiterfotos_abrufen_ab(Datenstand stand)
        {
            throw new NotImplementedException();
        }

        public Task<Stammdaten<Dienst>> Dienste_abrufen_ab(Datenstand stand)
        {
            throw new NotImplementedException();
        }

        Task<Stammdaten<Dienst>> DM7_PPLUS_API.Dienste_abrufen()
        {
            throw new NotImplementedException();
        }
    }
}