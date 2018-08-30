using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DM7_PPLUS_Integration.Daten;
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
        private readonly DM7_PPLUS_API _backendLevel3;
        private readonly HashSet<int> _apiLevel = new HashSet<int>();
        private readonly int _auswahllistenversion;
        private readonly Log _log;
        private readonly string _versionen;

        /// <summary>
        /// Empfängt API-Level-unabhängige Nachrichten und routet Sie in fachliche Nachrichten an die verschiedenen API Versionen
        /// </summary>
        public API_Router(
            Log log,
            int auswahllisten_version,
            Level_0_Test_API backend_level_0,
            DM7_PPLUS_API backend_level_1,
            DM7_PPLUS_API backend_level_3,
            DisposeGroup disposegroup) : base(disposegroup)
        {
            _log = log;
            _auswahllistenversion = auswahllisten_version;


            if (backend_level_0 != null) _apiLevel.Add(0);
            if (backend_level_1 != null) _apiLevel.Add(1);
            if (backend_level_3 != null) _apiLevel.Add(3);

            _backendLevel1 = backend_level_1;
            _backendLevel3 = backend_level_3;

            _versionen =
                _apiLevel.Count==1
                    ? "Version " + _apiLevel.Single()
                    : "Versionen " + String.Join(", ", (_apiLevel.OrderByDescending(_ => _).Select(_ => _.ToString())));

            log.Info(string.Format("DM7 Schittstelle mit API {0} bereitgestellt.", _versionen));

            disposegroup.With(() => log.Debug("API Router beendet."));

            var subject = new Subject<Notification>();

            if (backend_level_3 != null)
            {
                var subscription = backend_level_3.Stand_Mitarbeiterdaten.Subscribe(
                    new Observer<Stand>(
                        s =>
                        {
                            subject.Next(Map(s, Datenquellen.Mitarbeiter));
                        },
                        ex => subject.Error(ex)));
                disposegroup.With(subscription);
            }
            else
            {
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
            }

            Notifications = subject;
        }


        public Task<ConnectionResult> Connect_Ebene_1(string login, int maxApiLevel, int minApiLevel)
        {
            var task = new Task<ConnectionResult>(() =>
            {
                var levels = _apiLevel.Where(_=>_>=minApiLevel && _<=maxApiLevel).OrderByDescending(_=>_).ToList();

                var versionen =
                    minApiLevel == maxApiLevel
                        ? "Version " + maxApiLevel
                        : "Versionen " + maxApiLevel + ".." + minApiLevel;

                if (levels.Any())
                {
                    var level = levels.First();
                    _log.Info($"Verbindungsanfrage für API {versionen} erhalten, Verbindung aufgebaut mit API {level}.");
                    return new ConnectionSucceeded(level, _auswahllistenversion);
                }
                else
                {
                    _log.Info($"Verbindungsanfrage für API {versionen} konnte nicht erfüllt werden.");
                    return new ConnectionFailed(ConnectionFailure.Unable_to_provide_API_level, $"Dieser P-PLUS-Server kann nur APIs {_versionen} bereitstellen.");
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
            if (api_level == 1 || api_level==2)
            {
                var mitarbeiter =
                    (_backendLevel3?? _backendLevel1).Mitarbeiterdaten_abrufen(new VersionsStand(session, von),
                        new VersionsStand(session, bis));

                return mitarbeiter.ContinueWith(task =>
                {
                    var result = new List<byte[]>
                    {
                        ((VersionsStand) task.Result.Stand).Session.ToByteArray(),
                        BitConverter.GetBytes(((VersionsStand) task.Result.Stand).Version),
                        new[] {mitarbeiter.Result.Teilmenge ? (byte) 1 : (byte) 0},
                        BitConverter.GetBytes(mitarbeiter.Result.Mitarbeiter.Count)
                    };

                    foreach (var ma in mitarbeiter.Result.Mitarbeiter)
                    {
                        result.AddRange(Serializer.Serialize(ma));
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

    internal static class Serializer
    {
        internal static IEnumerable<byte[]> Serialize(Mitarbeiterdatensatz mitarbeiter)
        {
            yield return Serialize(mitarbeiter.DatensatzId);
            yield return Serialize(mitarbeiter.PersonId);
            yield return Serialize(mitarbeiter.Mandant);
            yield return Serialize(mitarbeiter.Struktur);
            yield return Serialize(mitarbeiter.Titel);
            yield return Serialize(mitarbeiter.Vorname);
            yield return Serialize(mitarbeiter.Nachname);
            yield return Serialize(mitarbeiter.Postanschrift);
            yield return Serialize(mitarbeiter.Geburtstag);
            yield return Serialize(mitarbeiter.Familienstand);
            yield return Serialize(mitarbeiter.Konfession);
            yield return Serialize(mitarbeiter.GueltigAb);
            yield return Serialize(mitarbeiter.GueltigBis);
            yield return Serialize(mitarbeiter.Qualifikation);
            yield return Serialize(mitarbeiter.Handzeichen);
            yield return Serialize(mitarbeiter.Personalnummer);
            yield return Serialize(mitarbeiter.Geschlecht);
            yield return Serialize(mitarbeiter.Kontakte);
        }

        private static byte[] Serialize(Postanschrift? anschrift)
        {
            if (anschrift.HasValue)
            {
                using (var stream = new MemoryStream())
                {
                    stream.Write(anschrift.Value.Id.ToByteArray(), 0, 16);
                    stream.WriteStringWithLengthPrefix(anschrift.Value.Adresszusatz);
                    stream.WriteStringWithLengthPrefix(anschrift.Value.Strasse);
                    stream.WriteStringWithLengthPrefix(anschrift.Value.Postleitzahl);
                    stream.WriteStringWithLengthPrefix(anschrift.Value.Ort);
                    stream.WriteStringWithLengthPrefix(anschrift.Value.Land);
                    return PrependLength(stream.ToArray());
                }
            }
            else
            {
                return PrependLength(new byte[0]);
            }
        }

        private static byte[] Serialize(Guid id) => id.ToByteArray();
        private static byte[] Serialize(int value) => BitConverter.GetBytes(value);
        private static byte[] Serialize(string text) => PrependLength(Encoding.UTF8.GetBytes(text));
        private static byte[] Serialize(ReadOnlyCollection<int> mandanten) => BitConverter.GetBytes(mandanten.Count).Concat(mandanten.SelectMany(BitConverter.GetBytes)).ToArray();
        private static byte[] Serialize(ReadOnlyCollection<Kontakt> kontakte) => new byte[0];
        private static byte[] Serialize(ReadOnlyCollection<Qualifikation> qualifikationen) => new byte[0];
        private static byte[] Serialize(Datum? datum) =>
            PrependLength(
                Encoding.UTF8.GetBytes(
                    datum.HasValue
                        ? $"{datum.Value.Jahr.ToString("0000")}{datum.Value.Monat.ToString("00")}{datum.Value.Tag.ToString("00")}"
                        : ""));
        private static byte[] Serialize(Datum datum) =>
            PrependLength(
                Encoding.UTF8.GetBytes($"{datum.Jahr.ToString("0000")}{datum.Monat.ToString("00")}{datum.Tag.ToString("00")}"));
        private static byte[] PrependLength(byte[] b) => BitConverter.GetBytes(b.Length).Concat(b).ToArray();
    }
}