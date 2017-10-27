﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using DM7_PPLUS_Integration.Daten;
using DM7_PPLUS_Integration.Implementierung.Shared;

namespace DM7_PPLUS_Integration.Implementierung.Server
{
    /// <summary>
    /// Stellt die Version 2 der API zur Verfügung und erfüllt Anfragen nach Daten und Notifications
    /// </summary>
    internal sealed class API_Level_2_Adapter : DisposeGroupMember, DM7_PPLUS_API
    {
        private readonly PPLUS_Backend _backend;
        private readonly Guid _session;
        private long _stand;

        private readonly Dictionary<long, IEnumerable<Guid>> _Mitarbeiter_je_Stand =
            new Dictionary<long, IEnumerable<Guid>>();


        public API_Level_2_Adapter(PPLUS_Backend backend, Action<Exception> onException, Log log, IDisposable disposegroup) : base(disposegroup)
        {
            _session = Guid.NewGuid();
            log.Debug($"Server ID {_session.ToString()} ");
            _backend = backend;
            _Mitarbeiter_je_Stand.Add(_stand, _backend.Alle_Mitarbeiter());

            Stand_Mitarbeiterdaten = new Subject<Stand>();
            _backend.Aenderungen_an_Mitarbeiterstammdaten.Subscribe(
                new Observer<IEnumerable<Guid>>(
                    mitarbeiter =>
                    {
                        _stand++;
                        _Mitarbeiter_je_Stand.Add(_stand, mitarbeiter);
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
                new ReadOnlyCollection<Mitarbeiterdatensatz>(_backend.Mitarbeiterdatensaetze_abrufen(mitarbeiter)
                    .ToList()),
                new ReadOnlyCollection<Mitarbeiterfoto>(new List<Mitarbeiterfoto>())));
            task.RunSynchronously();
            return task;
        }

        public Task<Mitarbeiterdatensaetze> Mitarbeiterdaten_abrufen()
        {
            return Mitarbeiterdaten_abrufen(Ab_Initio, Aktueller_Stand);
        }

        public void Announce()
        {
            ((Subject<Stand>)Stand_Mitarbeiterdaten).Next(new VersionsStand(_session, _stand));
        }
    }
}