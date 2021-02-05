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
    /// Empfängt API-Version-unabhängige Nachrichten und routet Sie in fachliche Nachrichten an die verschiedenen API Versionen
    /// </summary>
    internal class API_Router : DisposeGroupMember, Schicht_2_Protokoll__Verbindungsaufbau, Schicht_2_Protokoll__API_Version_unabhaengige_Uebertragung
    {
        private readonly DM7_PPLUS_API _backendVersion1;
        private readonly DM7_PPLUS_API _backendVersion3;
        private readonly DM7_PPLUS_API _backendVersion4;
        private readonly HashSet<int> _apiVersion = new HashSet<int>();
        private readonly int _auswahllistenversion;
        private readonly PPLUS_Authentifizierung _authentifizierung;
        private readonly Log _log;
        private readonly string _versionen;

        /// <summary>
        /// Empfängt API-Version-unabhängige Nachrichten und routet Sie in fachliche Nachrichten an die verschiedenen API Versionen
        /// </summary>
        public API_Router(
            Log log,
            int auswahllisten_version,
            PPLUS_Authentifizierung authentifizierung,
            Version_0_Test_API backend_version_0,
            DM7_PPLUS_API backend_version_1,
            DM7_PPLUS_API backend_version_3,
            DM7_PPLUS_API backend_version_4,
            DisposeGroup disposegroup) : base(disposegroup)
        {
            _log = log;
            _auswahllistenversion = auswahllisten_version;
            _authentifizierung = authentifizierung;


            if (backend_version_0 != null) _apiVersion.Add(0);
            if (backend_version_1 != null) _apiVersion.Add(1);
            if (backend_version_3 != null) _apiVersion.Add(3);
            if (backend_version_4 != null) _apiVersion.Add(4);

            _backendVersion1 = backend_version_1;
            _backendVersion3 = backend_version_3;
            _backendVersion4 = backend_version_4;

            _versionen =
                _apiVersion.Count==1
                    ? "Version " + _apiVersion.Single()
                    : "Versionen " + string.Join(", ", (_apiVersion.OrderByDescending(_ => _).Select(_ => _.ToString())));

            log.Info($"DM7 Schittstelle mit API {_versionen} bereitgestellt.");

            disposegroup.With(() => log.Debug("API Router beendet."));

            var subject = new Subject<Notification>();

            if (backend_version_3 != null)
            {
                var subscription = backend_version_3.Stand_Mitarbeiterdaten.Subscribe(
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
                if (backend_version_1 != null)
                {
                    var subscription = backend_version_1.Stand_Mitarbeiterdaten.Subscribe(
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


        public Task<ConnectionResult> Connect_Schicht_1(string credentials, int maxApiVersion, int minApiVersion)
        {
            var task = new Task<ConnectionResult>(() =>
            {
                if (_authentifizierung.Authentifizieren(credentials) == null)
                {
                    _log.Info("Unautorisierter Verbindungsversuch wurde abgelehnt.");
                    return new ConnectionFailed(ConnectionFailure.Unauthorized, "Keine Berechtigung zum Zugriff auf P-PLUS Daten.");
                }

                var versions = _apiVersion.Where(_=>_>=minApiVersion && _<=maxApiVersion).OrderByDescending(_=>_).ToList();

                var versionen =
                    minApiVersion == maxApiVersion
                        ? "Version " + maxApiVersion
                        : "Versionen " + maxApiVersion + ".." + minApiVersion;

                if (versions.Any())
                {
                    var version = versions.First();
                    _log.Info($"Verbindungsanfrage für API {versionen} erhalten, Verbindung aufgebaut mit API {version}.");
                    return new ConnectionSucceeded(version, _auswahllistenversion);
                }
                else
                {
                    _log.Info($"Verbindungsanfrage für API {versionen} konnte nicht erfüllt werden.");
                    return new ConnectionFailed(ConnectionFailure.Unable_to_provide_API_version, $"Dieser P-PLUS-Server kann nur APIs {_versionen} bereitstellen.");
                }
            });
            task.RunSynchronously();
            return task;
        }


        private static Notification Map(Stand stand, int datenquelle)
        {
            var s = (VersionsStand)stand;
            return new NotificationData(s.Session, datenquelle, s.Version);
        }

        public Task<QueryResponse> Query_Mitarbeiterdatensätze(string credentials, int api_version, Guid session, int datenquelle, long von, long bis)
        {
            var autorisierung = _authentifizierung.Authentifizieren(credentials);

            if (autorisierung == null)
            {
                return Task.FromResult((QueryResponse)new QueryFailed(QueryFailure.Unauthorized, "Ungültige oder abgelaufene Zugriffsberechtigung"));
            }

            if (!(autorisierung is StammdatenZugriff))
            {
                return Task.FromResult((QueryResponse)new QueryFailed(QueryFailure.Unauthorized, "Unzureichende Zugriffsberechtigung"));
            }

            if (api_version == 1 || api_version==3)
            {
                var mitarbeiter =
                    (_backendVersion3?? _backendVersion1).Mitarbeiterdaten_abrufen(new VersionsStand(session, von),
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

        public Task<QueryResponse> Query_Mitarbeiter(string credentials, int api_version, Guid session, Datenstand? stand)
        {
            var autorisierung = _authentifizierung.Authentifizieren(credentials);

            if (autorisierung == null)
            {
                return Task.FromResult((QueryResponse)new QueryFailed(QueryFailure.Unauthorized, "Ungültige oder abgelaufene Zugriffsberechtigung"));
            }

            if (!(autorisierung is StammdatenZugriff))
            {
                return Task.FromResult((QueryResponse)new QueryFailed(QueryFailure.Unauthorized, "Unzureichende Zugriffsberechtigung"));
            }

            if (api_version == 4)
            {
                if (stand.HasValue)
                {
                    return _backendVersion4
                        .Mitarbeiter_abrufen_ab(stand.Value)
                        .ContinueWith(task => (QueryResponse)new QueryResult(Serializer.Serialize(task.Result, Serializer.Serialize_Mitarbeiter)));
                }

                return _backendVersion4
                    .Mitarbeiter_abrufen()
                    .ContinueWith(task => (QueryResponse)new QueryResult(Serializer.Serialize(task.Result, Serializer.Serialize_Mitarbeiter)));
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        public Task<QueryResponse> Query_Mitarbeiterfotos(string credentials, int api_version, Guid session, Datenstand? stand)
        {
            var autorisierung = _authentifizierung.Authentifizieren(credentials);

            if (autorisierung == null)
            {
                return Task.FromResult((QueryResponse)new QueryFailed(QueryFailure.Unauthorized, "Ungültige oder abgelaufene Zugriffsberechtigung"));
            }

            if (!(autorisierung is StammdatenZugriff))
            {
                return Task.FromResult((QueryResponse)new QueryFailed(QueryFailure.Unauthorized, "Unzureichende Zugriffsberechtigung"));
            }

            if (api_version == 4)
            {
                if (stand.HasValue)
                {
                    return _backendVersion4
                        .Mitarbeiterfotos_abrufen_ab(stand.Value)
                        .ContinueWith(task => (QueryResponse)new QueryResult(Serializer.Serialize(task.Result, Serializer.Serialize)));
                }

                return _backendVersion4
                    .Mitarbeiterfotos_abrufen()
                    .ContinueWith(task => (QueryResponse)new QueryResult(Serializer.Serialize(task.Result, Serializer.Serialize)));
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        public Task<QueryResponse> Query_Dienste(string credentials, int api_version, Guid session, Datenstand? stand)
        {
            var autorisierung = _authentifizierung.Authentifizieren(credentials);

            if (autorisierung == null)
            {
                return Task.FromResult((QueryResponse)new QueryFailed(QueryFailure.Unauthorized, "Ungültige oder abgelaufene Zugriffsberechtigung"));
            }

            if (!(autorisierung is StammdatenZugriff))
            {
                return Task.FromResult((QueryResponse)new QueryFailed(QueryFailure.Unauthorized, "Unzureichende Zugriffsberechtigung"));
            }

            if (api_version == 4)
            {
                if (stand.HasValue)
                {
                    return _backendVersion4
                        .Dienste_abrufen_ab(stand.Value)
                        .ContinueWith(task => (QueryResponse)new QueryResult(Serializer.Serialize(task.Result, Serializer.Serialize)));
                }

                return _backendVersion4
                    .Dienste_abrufen()
                    .ContinueWith(task => (QueryResponse)new QueryResult(Serializer.Serialize(task.Result, Serializer.Serialize)));
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
        internal static byte[] Serialize<T>(Stammdaten<T> daten, Func<T, byte[]> serializer)
        {
            var res =Serialize_list(daten, serializer)
                .Concat(BitConverter.GetBytes(daten.Stand.Value))
                .ToArray();
            return res;
        }

        internal static byte[] Serialize_Mitarbeiter(Mitarbeiter mitarbeiter)
        {
            var result = new List<byte>();
            foreach (var d in Serialize(mitarbeiter)) result.AddRange(d);
            return result.ToArray();
        }

        internal static byte[] Serialize(Mitarbeiterfoto foto)
        {
            return Serialize(foto.Mitarbeiter)
                .Concat(Serialize(foto.Format))
                .Concat(Serialize(foto.Foto.Length))
                .Concat(foto.Foto)
                .ToArray();
        }

        internal static IEnumerable<byte[]> Serialize(Mitarbeiter mitarbeiter)
        {
            yield return Serialize(mitarbeiter.PPLUS_Id);
            yield return Serialize_list(mitarbeiter.DM7_Mandantenzugehörigkeiten, Serialize);
            yield return Serialize(mitarbeiter.Personalnummer);
            yield return Serialize(mitarbeiter.Titel);
            yield return Serialize(mitarbeiter.Vorname);
            yield return Serialize(mitarbeiter.Nachname);
            yield return Serialize(mitarbeiter.Postanschrift);
            yield return Serialize(mitarbeiter.Handzeichen);
            yield return Serialize(mitarbeiter.Geburtstag);
            yield return Serialize(mitarbeiter.Geschlecht);
            yield return Serialize(mitarbeiter.Konfession);
            yield return Serialize(mitarbeiter.Familienstand);
            yield return Serialize(mitarbeiter.Qualifikation);
            yield return Serialize(mitarbeiter.Kontakte);
        }

        private static byte[] Serialize(DM7_Mandantenzugehörigkeiten mandantenzugehörigkeiten)
        {
            return Serialize(mandantenzugehörigkeiten.MandantId)
                .Concat(Serialize(mandantenzugehörigkeiten.GueltigAb))
                .Concat(Serialize(mandantenzugehörigkeiten.GueltigBis))
                .ToArray();
        }

        public static byte[] Serialize(Dienst dienst)
        {
            return Serialize(dienst.Id)
                .Concat(Serialize(dienst.Mandant))
                .Concat(Serialize(dienst.Kurzbezeichnung))
                .Concat(Serialize(dienst.Bezeichnung))
                .Concat(Serialize(dienst.Gültig_ab))
                .Concat(Serialize(dienst.Gültig_bis))
                .Concat(Serialize(dienst.Beginn))
                .Concat(Serialize(dienst.Gültig_an))
                .Concat(Serialize(dienst.Gelöscht))
                .ToArray();
        }

        internal static byte[] Serialize_list<T>(ReadOnlyCollection<T> collection, Func<T, byte[]> serialize)
        {
            return BitConverter.GetBytes(collection.Count)
                .Concat(collection.SelectMany(serialize))
                .ToArray();
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

        private static byte[] Serialize(Dienst_Gültigkeit gültigkeit)
        {
            return Serialize(gültigkeit.Montag)
                .Concat(Serialize(gültigkeit.Dienstag))
                .Concat(Serialize(gültigkeit.Mittwoch))
                .Concat(Serialize(gültigkeit.Donnerstag))
                .Concat(Serialize(gültigkeit.Freitag))
                .Concat(Serialize(gültigkeit.Samstag))
                .Concat(Serialize(gültigkeit.Sonntag))
                .Concat(Serialize(gültigkeit.Feiertags))
                .ToArray();
        }

        private static byte[] Serialize(Guid id) => id.ToByteArray();
        private static byte[] Serialize(int value) => BitConverter.GetBytes(value);
        private static byte[] Serialize(bool value) => BitConverter.GetBytes(value);
        private static byte[] Serialize(string text) => PrependLength(Encoding.UTF8.GetBytes(text));
        private static byte[] Serialize(ReadOnlyCollection<int> mandanten) => BitConverter.GetBytes(mandanten.Count).Concat(mandanten.SelectMany(BitConverter.GetBytes)).ToArray();
        private static byte[] Serialize(ReadOnlyCollection<Kontakt> kontakte)
        {
            using (var stream = new MemoryStream())
            {
                stream.Write(BitConverter.GetBytes(kontakte.Count), 0, 4);

                foreach (var kontakt in kontakte)
                {
                    stream.Write(kontakt.Kontaktart.ToByteArray(), 0, 16);
                    stream.Write(kontakt.Kontaktform.ToByteArray(), 0, 16);
                    stream.WriteStringWithLengthPrefix(kontakt.Eintrag);
                }

                return stream.ToArray();
            }
        }

        private static byte[] Serialize(ReadOnlyCollection<Qualifikation> qualifikationen)
        {
            return Serialize_list(qualifikationen, Serialize);
        }

        private static byte[] Serialize(Qualifikation qualifikation)
        {
            return Serialize(qualifikation.Stufe)
                .Concat(Serialize(qualifikation.Bezeichnung))
                .Concat(Serialize(qualifikation.GueltigAb))
                .Concat(Serialize(qualifikation.GueltigBis))
                .ToArray();
        }

        private static byte[] Serialize(Datum? datum) =>
            PrependLength(
                Encoding.UTF8.GetBytes(
                    datum.HasValue
                        ? $"{datum.Value.Jahr:0000}{datum.Value.Monat:00}{datum.Value.Tag:00}"
                        : ""));
        private static byte[] Serialize(Datum datum) =>
            PrependLength(Encoding.UTF8.GetBytes($"{datum.Jahr:0000}{datum.Monat:00}{datum.Tag:00}"));

        private static byte[] Serialize(Uhrzeit uhrzeit) =>
            Serialize(uhrzeit.Stunden).Concat(Serialize(uhrzeit.Minuten)).ToArray();
        private static byte[] PrependLength(byte[] b) => BitConverter.GetBytes(b.Length).Concat(b).ToArray();
    }
}