using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DM7_PPLUS_Integration.Implementierung.Protokoll;
using DM7_PPLUS_Integration.Implementierung.Shared;
using DM7_PPLUS_Integration.Implementierung.Testing;

namespace DM7_PPLUS_Integration.Implementierung.Server
{

    /// <summary>
    /// Empfängt API-Level-unabhängige Nachrichten und routet Sie in fachliche Nachrichten an die verschiedenen API Versionen
    /// </summary>
    internal class API_Router : DisposeGroupMember, Ebene_2_Protokoll__Verbindungsaufbau, Ebene_2_Protokoll__API_Level_unabhaengige_Uebertragung
    {
        private readonly DM7_PPLUS_API _backendLevel1;
        private readonly int _maxApiLevel;
        private readonly int _minApiLevel;
        private readonly int _auswahllistenversion;
        private readonly Log _log;
        private readonly Guid _session;

        /// <summary>
        /// Empfängt API-Level-unabhängige Nachrichten und routet Sie in fachliche Nachrichten an die verschiedenen API Versionen
        /// </summary>
        public API_Router(Log log, Guid session, int auswahllisten_version, Level_0_Test_API backend_level_0,  DM7_PPLUS_API backend_level_1/*, DM_PPLUS_API_2 backend_level_2, ...*/, DisposeGroup disposegroup) : base(disposegroup)
        {
            _log = log;
            _session = session;
            _auswahllistenversion = auswahllisten_version;

            _maxApiLevel = 0;
            if (backend_level_1 != null) _maxApiLevel = 1;

            _minApiLevel = 0;
            if (backend_level_0 == null) _minApiLevel = 1;

            _backendLevel1 = backend_level_1;

            var versionen =
                _minApiLevel == _maxApiLevel
                    ? "Version " + _maxApiLevel
                    : "Versionen " + _maxApiLevel + ".." + _minApiLevel;

            log.Info(string.Format("DM7 Schittstelle mit API {0} bereitgestellt.", versionen));

            disposegroup.With(() => log.Debug("API Router beendet."));

            var subject = new Subject<Notification>();

            if (backend_level_1 != null)
            {
                var subscription = backend_level_1.Stand_Mitarbeiterdaten.Subscribe(
                    new Observer<Stand>(
                        s =>
                        {
                            subject.Next(Map(s, Datenquellen.Mitarbeiter));
                        },
                        ex => subject.Error(ex)));
                disposegroup.With(subscription);
            }

            Notifications = subject;
        }


        public Task<ConnectionResult> Connect_Ebene_1(string login, int maxApiLevel, int minApiLevel)
        {
            var task = new Task<ConnectionResult>(() =>
            {
                var max = Math.Min(maxApiLevel, _maxApiLevel);
                var min = Math.Max(minApiLevel, _minApiLevel);

                var level = max >= min ? (int?) max : null;

                var versionen =
                    minApiLevel == maxApiLevel
                        ? "Version " + maxApiLevel
                        : "Versionen " + maxApiLevel + ".." + minApiLevel;

                if (level.HasValue)
                {
                    _log.Info($"Verbindungsanfrage für API {versionen} erhalten, Verbindung aufgebaut mit API {level.Value}.");
                    return new ConnectionSucceeded(max, _auswahllistenversion, _session);
                }
                else
                {
                    _log.Info($"Verbindungsanfrage für API {versionen} konnte nicht erfüllt werden.");
                    var range = (_maxApiLevel == _minApiLevel) ? $"des Levels {_maxApiLevel}" : $"von Level {_minApiLevel} bis {_maxApiLevel}";
                    return new ConnectionFailed(ConnectionFailure.Unable_to_provide_API_level, $"Dieser P-PLUS-Server kann nur APIs {range} bereitstellen.");
                }
            });
            task.RunSynchronously();
            return task;
        }


        private Notification Map(Stand stand, int datenquelle)
        {
            var s = (VersionsStand)stand;
            return new NotificationData(s.Session, datenquelle, s.Version);
        }

        public Task<QueryResponse> Query(int api_level, Guid session, int datenquelle, long von, long bis)
        {
            if (api_level == 1)
            {
                var mitarbeiter =
                    _backendLevel1.Mitarbeiterdaten_abrufen(new VersionsStand(session, von),
                        new VersionsStand(session, bis));
                return mitarbeiter.ContinueWith(task =>
                {
                    var result = new List<byte[]>
                    {
                        session.ToByteArray(),
                        BitConverter.GetBytes(((VersionsStand) task.Result.Stand).Version),
                        new[] {mitarbeiter.Result.Teilmenge ? (byte) 1 : (byte) 0},
                        BitConverter.GetBytes(mitarbeiter.Result.Mitarbeiter.Count)
                    };

                    foreach (var ma in mitarbeiter.Result.Mitarbeiter)
                    {
                        var nachname = System.Text.Encoding.UTF8.GetBytes(ma.Vorname);
                        var vorname = System.Text.Encoding.UTF8.GetBytes(ma.Nachname);
                        result.Add(BitConverter.GetBytes(nachname.Length));
                        result.Add(BitConverter.GetBytes(vorname.Length));
                        result.Add(nachname);
                        result.Add(vorname);
                    }

                    var resultarray = new byte[result.Sum(_ => _.Length)];

                    var pos = 0;
                    foreach (var ar in result)
                    {
                        Array.Copy(ar, 0, resultarray, pos, ar.Length);
                        pos += ar.Length;
                    }

                    return (QueryResponse) new QueryResult(resultarray);
                });
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        public IObservable<Notification> Notifications { get; }
    }


}