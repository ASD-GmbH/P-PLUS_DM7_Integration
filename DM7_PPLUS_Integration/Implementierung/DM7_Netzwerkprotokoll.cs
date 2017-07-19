using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using DM7_PPLUS_Integration.Daten;

namespace DM7_PPLUS_Integration.Implementierung
{
    public interface Ebene_2_Protokoll__Verbindungsaufbau : IDisposable
    {
        Task<ConnectionResult> Connect(string login, int maxApiLevel, int minApiLevel);
    }

    public interface Ebene_2_Protokoll__API_Level_unabhaengige_Uebertragung : IDisposable
    {
        Task<QueryResponse> Query(int api_level, Guid session, int datenquelle, long von, long bis);
        IObservable<Notification> Notifications { get; }
    }

    public interface Notification { }

    public class NotificationData : Notification
    {
        public readonly Guid Session;
        public readonly int Datenquelle;
        public readonly long Version;

        public NotificationData(Guid session, int datenquelle, long version)
        {
            Session = session;
            Datenquelle = datenquelle;
            Version = version;
        }
    }

    public enum Reason { Unknown_reason, API_Level_no_longer_available }

    public class NotificationsClosed : Notification
    {
        public NotificationsClosed(Reason reason, string info)
        {
            Reason = reason;
            Info = info;
        }

        public readonly Reason Reason;
        public readonly string Info;
    }

    public interface QueryResponse { }

    public class QueryResult : QueryResponse
    {
        public QueryResult(byte[] data)
        {
            Data = data;
        }

        public readonly byte[] Data;
    }
    public enum QueryFailure { Unknown_reason, API_Level_no_longer_available, Internal_Server_Error }

    public class QueryFailed : QueryResponse
    {
        public QueryFailed(QueryFailure reason, string info)
        {
            Reason = reason;
            Info = info;
        }

        public readonly QueryFailure Reason;
        public readonly string Info;
    }

    public interface ConnectionResult { }

    public class ConnectionSucceeded : ConnectionResult
    {
        public readonly int Api_Level;
        public readonly int Auswahllistenversion;
        public readonly Guid Session;

        public ConnectionSucceeded(int apiLevel, int auswahllistenversion, Guid session)
        {
            Api_Level = apiLevel;
            Auswahllistenversion = auswahllistenversion;
            Session = session;
        }
    }
    public enum ConnectionFailure  { Unsupported_Connection_Protocol, Unable_to_provide_API_level, Internal_Server_Error }

    public class ConnectionFailed : ConnectionResult
    {
        public ConnectionFailed(ConnectionFailure reason, string info)
        {
            Reason = reason;
            Info = info;
        }

        public readonly ConnectionFailure Reason;
        public readonly string Info;
    }

    public interface Ebene_3_Protokoll__Netzwerkuebertragung : IDisposable
    {
        Task<byte[]> Request(byte[] request);
        IObservable<byte[]> Notifications { get; }
    }


    /// <summary>
    /// Implementiert die API level 1 und übersetzt die Anfragen in API-Level-unabhängige Nachrichten
    /// </summary>
    public class API_Level_1_Proxy : DM7_PPLUS_API
    {
        private const int API_LEVEL = 1;

        private readonly Ebene_2_Protokoll__API_Level_unabhaengige_Uebertragung _ebene_2_Proxy;
        private readonly Guid _session;
        private readonly IDisposable _subscription;

        public API_Level_1_Proxy(Ebene_2_Protokoll__API_Level_unabhaengige_Uebertragung ebene2Proxy, Guid session, int auswahllistenversion)
        {
            _ebene_2_Proxy = ebene2Proxy;
            _session = session;
            Auswahllisten_Version = auswahllistenversion;
            var subject = new Subject<Stand>();
            _subscription = ebene2Proxy.Notifications.Subscribe(new Observer<Notification>(
                no =>
                {
                    var data = no as NotificationData;
                    if (data != null) { subject.Next(new VersionsStand(data.Session, data.Version)); }
                    else if (no is NotificationsClosed) { subject.Completed(); }
                    else subject.Error(new ConnectionErrorException($"Interner Fehler im Notificationstream. Unbekannte Nachricht: {no.GetType().Name}"));
                },
                ex => { throw new ConnectionErrorException($"Interner Fehler im Notificationstream: {ex.Message}", ex); } ));
            Stand_Mitarbeiterdaten = subject;
        }

        public void Dispose()
        {
            _subscription.Dispose();
        }

        public int Auswahllisten_Version { get; }

        public IObservable<Stand> Stand_Mitarbeiterdaten { get; }

        public Task<Mitarbeiterdatensaetze> Mitarbeiterdaten_abrufen(Stand von, Stand bis)
        {
            return
                _ebene_2_Proxy
                    .Query(API_LEVEL, _session, Datenquellen.Mitarbeiter, ((VersionsStand)von).Version, ((VersionsStand)bis).Version)
                    .ContinueWith(
                        task =>
                        {
                            var result = task.Result as QueryResult;
                            if (result != null)
                            {
                                return Deserialisiere_Mitarbeiterdatensaetze(result.Data);
                            }
                            else
                            {
                                var failed = task.Result as QueryFailed;
                                if (failed != null)
                                {
                                    if (failed.Reason == QueryFailure.Internal_Server_Error)
                                    {
                                        throw new ConnectionErrorException($"Die Datenabfrage 'Mitarbeiterdaten_abrufen' ist auf dem Server fehlgeschlagen: {failed.Info}.");
                                    }
                                    if (failed.Reason == QueryFailure.Unknown_reason)
                                    {
                                        throw new ConnectionErrorException($"Die Datenabfrage 'Mitarbeiterdaten_abrufen' ist fehlgeschlagen: {failed.Info}.");
                                    }
                                }
                            }

                            throw new ConnectionErrorException($"Die Datenabfrage 'Mitarbeiterdaten_abrufen' ist fehlgeschlagen: Unbekanntes Protokoll ({task.Result.GetType().Name}).");
                        }
                    );
        }

        private Mitarbeiterdatensaetze Deserialisiere_Mitarbeiterdatensaetze(byte[] resultData)
        {
            var guidbuffer = new byte[16];

            var position = 0;

            Array.Copy(resultData, guidbuffer, 16);
            var session = new Guid(guidbuffer);
            position += 16;
            var version = BitConverter.ToInt64(resultData, position);
            position += 8;
            var anzahl_Mitarbeiterdatensaetze = BitConverter.ToInt32(resultData, position);
            position += 4;

            var mitarbeiter = new List<Mitarbeiterdatensatz>();

            for (var i = 0; i < anzahl_Mitarbeiterdatensaetze; i++)
            {
                var laenge_nachname = BitConverter.ToInt32(resultData, position);
                position += 4;
                var laenge_vorname = BitConverter.ToInt32(resultData, position);
                position += 4;

                var nachname = System.Text.Encoding.UTF8.GetString(resultData, position, laenge_nachname);
                position += laenge_nachname;

                var vorname = System.Text.Encoding.UTF8.GetString(resultData, position, laenge_vorname);
                position += laenge_vorname;

                var ma =
                    new Mitarbeiterdatensatz(
                        Guid.NewGuid(),
                        Guid.Empty,
                        vorname,
                        nachname,
                        null,
                        null,
                        Guid.Empty,
                        Guid.Empty,
                        new Datum(1, 1, 2017),
                        null,
                        new ReadOnlyCollection<Qualifikation>(new List<Qualifikation>()),
                        "",
                        "1",
                        Guid.Empty,
                        new ReadOnlyCollection<Kontakt>(new List<Kontakt>()));

                mitarbeiter.Add(ma);
            }

            return
                new Mitarbeiterdatensaetze(
                    false,
                    new VersionsStand(
                        session,
                        version),
                    new ReadOnlyCollection<Mitarbeiterdatensatz>(mitarbeiter),
                    new ReadOnlyCollection<Mitarbeiterfoto>(new List<Mitarbeiterfoto>()));
        }

        public Task<Mitarbeiterdatensaetze> Mitarbeiterdaten_abrufen()
        {
            var stand = VersionsStand.AbInitio(_session);
            return Mitarbeiterdaten_abrufen(stand, stand);
        }
    }

    /// <summary>
    /// Implementiert die API level 2 und übersetzt die Anfragen in API-Level-unabhängige Nachrichten
    /// (Derzeit nur zum Test der Versionsverhandlung. Reuse für echte Anforderungen Level 2)
    /// </summary>
    public class API_Level_2_Proxy : DM7_PPLUS_API
    {
        private const int API_LEVEL = 1;

        private readonly Ebene_2_Protokoll__API_Level_unabhaengige_Uebertragung _ebene_2_Proxy;
        private readonly Guid _session;
        private readonly IDisposable _subscription;

        public API_Level_2_Proxy(Ebene_2_Protokoll__API_Level_unabhaengige_Uebertragung ebene2Proxy, Guid session, int auswahllistenversion)
        {
            _ebene_2_Proxy = ebene2Proxy;
            _session = session;
            Auswahllisten_Version = auswahllistenversion;
            var subject = new Subject<Stand>();
            _subscription = ebene2Proxy.Notifications.Subscribe(new Observer<Notification>(
                no =>
                {
                    var data = no as NotificationData;
                    if (data != null) { subject.Next(new VersionsStand(data.Session, data.Version)); }
                    else if (no is NotificationsClosed) { subject.Completed(); }
                    else subject.Error(new ConnectionErrorException($"Interner Fehler im Notificationstream. Unbekannte Nachricht: {no.GetType().Name}"));
                },
                ex => { throw new ConnectionErrorException($"Interner Fehler im Notificationstream: {ex.Message}", ex); } ));
            Stand_Mitarbeiterdaten = subject;
        }

        public void Dispose()
        {
            _subscription.Dispose();
        }

        public int Auswahllisten_Version { get; }

        public IObservable<Stand> Stand_Mitarbeiterdaten { get; }

        public Task<Mitarbeiterdatensaetze> Mitarbeiterdaten_abrufen(Stand von, Stand bis)
        {
            return
                _ebene_2_Proxy
                    .Query(API_LEVEL, _session, Datenquellen.Mitarbeiter, ((VersionsStand)von).Version, ((VersionsStand)bis).Version)
                    .ContinueWith(
                        task =>
                        {
                            var result = task.Result as QueryResult;
                            if (result != null)
                            {
                                return Deserialisiere_Mitarbeiterdatensaetze(result.Data);
                            }
                            else
                            {
                                var failed = task.Result as QueryFailed;
                                if (failed != null)
                                {
                                    if (failed.Reason == QueryFailure.Internal_Server_Error)
                                    {
                                        throw new ConnectionErrorException($"Die Datenabfrage 'Mitarbeiterdaten_abrufen' ist auf dem Server fehlgeschlagen: {failed.Info}.");
                                    }
                                    if (failed.Reason == QueryFailure.Unknown_reason)
                                    {
                                        throw new ConnectionErrorException($"Die Datenabfrage 'Mitarbeiterdaten_abrufen' ist fehlgeschlagen: {failed.Info}.");
                                    }
                                }
                            }

                            throw new ConnectionErrorException($"Die Datenabfrage 'Mitarbeiterdaten_abrufen' ist fehlgeschlagen: Unbekanntes Protokoll ({task.Result.GetType().Name}).");
                        }
                    );
        }

        private Mitarbeiterdatensaetze Deserialisiere_Mitarbeiterdatensaetze(byte[] resultData)
        {
            var guidbuffer = new byte[16];

            var position = 0;

            Array.Copy(resultData, guidbuffer, 16);
            var session = new Guid(guidbuffer);
            position += 16;
            var version = BitConverter.ToInt64(resultData, position);
            position += 8;
            var anzahl_Mitarbeiterdatensaetze = BitConverter.ToInt32(resultData, position);
            position += 4;

            var mitarbeiter = new List<Mitarbeiterdatensatz>();

            for (var i = 0; i < anzahl_Mitarbeiterdatensaetze; i++)
            {
                var laenge_nachname = BitConverter.ToInt32(resultData, position);
                position += 4;
                var laenge_vorname = BitConverter.ToInt32(resultData, position);
                position += 4;

                var nachname = System.Text.Encoding.UTF8.GetString(resultData, position, laenge_nachname);
                position += laenge_nachname;

                var vorname = System.Text.Encoding.UTF8.GetString(resultData, position, laenge_vorname);
                position += laenge_vorname;

                var ma =
                    new Mitarbeiterdatensatz(
                        Guid.NewGuid(),
                        Guid.Empty,
                        vorname,
                        nachname,
                        null,
                        null,
                        Guid.Empty,
                        Guid.Empty,
                        new Datum(1, 1, 2017),
                        null,
                        new ReadOnlyCollection<Qualifikation>(new List<Qualifikation>()),
                        "",
                        "1",
                        Guid.Empty,
                        new ReadOnlyCollection<Kontakt>(new List<Kontakt>()));

                mitarbeiter.Add(ma);
            }

            return
                new Mitarbeiterdatensaetze(
                    false,
                    new VersionsStand(
                        session,
                        version),
                    new ReadOnlyCollection<Mitarbeiterdatensatz>(mitarbeiter),
                    new ReadOnlyCollection<Mitarbeiterfoto>(new List<Mitarbeiterfoto>()));
        }

        public Task<Mitarbeiterdatensaetze> Mitarbeiterdaten_abrufen()
        {
            var stand = VersionsStand.AbInitio(_session);
            return Mitarbeiterdaten_abrufen(stand, stand);
        }
    }

    public class Serialization_Proxy : Ebene_2_Protokoll__API_Level_unabhaengige_Uebertragung
    {
        private readonly Ebene_3_Protokoll__Netzwerkuebertragung _proxy;
        private readonly IDisposable _subscription;

        public Serialization_Proxy(Ebene_3_Protokoll__Netzwerkuebertragung proxy)
        {
            _proxy = proxy;
            var subject = new Subject<Notification>();
            _subscription = proxy.Notifications.Subscribe(new Observer<byte[]>(
                datagram =>
                {
                    var notification = Deserialize(datagram);
                    subject.Next(notification);
                },
                ex => { throw new ConnectionErrorException($"Interner Fehler im Notificationstream: {ex.Message}", ex); }));

            Notifications = subject;
        }

        private Notification Deserialize(byte[] datagram)
        {
            var guidbuffer = new byte[16];
            var position = 0;

            Array.Copy(datagram, guidbuffer, 16);
            var session = new Guid(guidbuffer);
            position += 16;
            var datenquelle = BitConverter.ToInt32(datagram, position);
            position += 4;
            var version = BitConverter.ToInt64(datagram, position);

            return new NotificationData(session,datenquelle,version);
        }

        // übersetzt API-Level-unabhängige Nachrichten in serialisierte Nachrichten und sendet diese über den Proxy
        public void Dispose()
        {
            _subscription.Dispose();
        }

        public Task<QueryResponse> Query(int api_level, Guid session, int datenquelle, long von, long bis)
        {
            var query = new List<byte[]>
            {
                session.ToByteArray(),
                BitConverter.GetBytes(api_level),
                BitConverter.GetBytes(datenquelle),
                BitConverter.GetBytes(von),
                BitConverter.GetBytes(bis)
            };

            return _proxy
                .Request(query.Concat())
                .ContinueWith(task => (QueryResponse) new QueryResult(task.Result));
        }

        public IObservable<Notification> Notifications { get; }
    }

    //public class NetMQ_Client : Ebene_3_Protokoll__Netzwerkuebertragung
    //{
    //    private readonly NetMQ_Server _netMqServer;

    //    public NetMQ_Client(NetMQ_Server netMQ_Server) // TODO: parameter entfernen, NetMQ messages an den NetMQ-Server senden.
    //    {
    //        _netMqServer = netMQ_Server;
    //        Notifications = new Subject<byte[]>();
    //    }

    //    // sendet serialisierte Nachrichten über ZeroMQ an NetMQ_Server
    //    public void Dispose()
    //    {
    //    }

    //    public Task<byte[]> Request(byte[] request)
    //    {
    //        return _netMqServer.Respond_to(request);
    //    }

    //    public IObservable<byte[]> Notifications { get; }
    //}

    //public class NetMQ_Server
    //{
    //    private readonly Ebene_3_Protokoll__Netzwerkuebertragung _backend;

    //    public NetMQ_Server(Ebene_3_Protokoll__Netzwerkuebertragung backend)
    //    {
    //        _backend = backend;
    //    }

    //    // empfängt serialisierte Nachrichten über ZeroMQ und gibt sie weiter an das Backend
    //    public Task<byte[]> Respond_to(byte[] request)
    //    {
    //        return _backend.Request(request);
    //    }
    //}

    public class Serialization_Adapter : Ebene_3_Protokoll__Netzwerkuebertragung
    {
        private readonly Ebene_2_Protokoll__API_Level_unabhaengige_Uebertragung _backend;
        private readonly IDisposable _subscription;

        public Serialization_Adapter(Ebene_2_Protokoll__API_Level_unabhaengige_Uebertragung backend)
        {
            _backend = backend;
            var subject = new Subject<byte[]>();
            _subscription = backend.Notifications.Subscribe(new Observer<Notification>(
                notification =>
                {
                    var datagram = Serialize_Notification(notification);
                    subject.Next(datagram);
                },
                ex => throw new ConnectionErrorException($"Interner Fehler im Notificationstream: {ex.Message}", ex)));
            Notifications = subject;
        }

        private byte[] Serialize_Notification(Notification notification)
        {
            if (notification is NotificationData)
            {
                var data = (NotificationData) notification;
                var a_01 = data.Session.ToByteArray();
                var a_02 = BitConverter.GetBytes(data.Datenquelle);
                var a_03 = BitConverter.GetBytes(data.Version);

                return new List<byte[]> {a_01, a_02, a_03}.Concat();

            }
            throw new NotImplementedException();
        }

        // empfängt serialisierte Nachrichten und gibt sie als API-Level-unabhängige Nachrichten an das Backend weiter
        public void Dispose()
        {
            _subscription.Dispose();
        }

        public Task<byte[]> Request(byte[] request)
        {
            var guidbuffer = new byte[16];
            var position = 0;
            Array.Copy(request, guidbuffer, 16);
            var session = new Guid(guidbuffer);
            position += 16;
            var api_level = BitConverter.ToInt32(request, position);
            position += 4;
            var datenquelle = BitConverter.ToInt32(request, position);
            position += 4;
            var von = BitConverter.ToInt64(request, position);
            position += 8;
            var bis = BitConverter.ToInt64(request, position);

            return
                _backend
                .Query(api_level, session, datenquelle, von, bis)
                .ContinueWith(task => Serialize_QueryResponse(task.Result));
        }

        private byte[] Serialize_QueryResponse(QueryResponse response)
        {
            if (response is QueryResult)
            {
                return ((QueryResult) response).Data;
            }
            throw new NotImplementedException();

        }

        public IObservable<byte[]> Notifications { get; }
    }

    /// <summary>
    /// Empfängt API-Level-unabhängige Nachrichten und routet Sie in fachliche Nachrichten an die verschiedenen API Versionen
    /// </summary>
    public class API_Router : Ebene_2_Protokoll__Verbindungsaufbau, Ebene_2_Protokoll__API_Level_unabhaengige_Uebertragung
    {
        private readonly DM7_PPLUS_API _backendLevel1;
        private readonly IDisposable _subscription;
        private readonly int _maxApiLevel = 1;
        private readonly int _minApiLevel = 1;
        private readonly int _auswahllistenversion;
        private readonly Guid _session;

        /// <summary>
        /// Empfängt API-Level-unabhängige Nachrichten und routet Sie in fachliche Nachrichten an die verschiedenen API Versionen
        /// </summary>
        public API_Router(Guid session, int auswahllisten_version, Level_0_Test_API backend_level_0,  DM7_PPLUS_API backend_level_1/*, DM_PPLUS_API_2 backend_level_2, ...*/)
        {
            _session = session;
            _auswahllistenversion = auswahllisten_version;

            _maxApiLevel = 0;
            if (backend_level_1 != null) _maxApiLevel = 1;

            _minApiLevel = 0;
            if (backend_level_0 == null) _minApiLevel = 1;

            _backendLevel1 = backend_level_1;

            var subject = new Subject<Notification>();

            if (backend_level_1 != null)
            {
                _subscription = backend_level_1.Stand_Mitarbeiterdaten.Subscribe(
                    new Observer<Stand>(
                        s => subject.Next(Map(s, Datenquellen.Mitarbeiter)),
                        ex => subject.Error(ex)));
            }

            Notifications = subject;
        }


        public Task<ConnectionResult> Connect(string login, int maxApiLevel, int minApiLevel)
        {
            var task = new Task<ConnectionResult>(() =>
            {
                var max = Math.Min(maxApiLevel, _maxApiLevel);
                var min = Math.Max(minApiLevel, _minApiLevel);
                if (max >= min) return new ConnectionSucceeded(max, _auswahllistenversion, _session);
                var range = (_maxApiLevel == _minApiLevel) ? $"des Levels {_maxApiLevel}" : $"von Level {_minApiLevel} bis {_maxApiLevel}";
                return new ConnectionFailed(ConnectionFailure.Unable_to_provide_API_level, $"Dieser P-PLUS-Server kann nur APIs {range} bereitstellen.");
            });
            task.RunSynchronously();
            return task;
        }


        private Notification Map(Stand stand, int datenquelle)
        {
            var s = (VersionsStand)stand;
            return new NotificationData(s.Session, datenquelle, s.Version);
        }

        public void Dispose()
        {
            _subscription.Dispose();
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
                    var result = new List<byte[]>();

                    result.Add(session.ToByteArray());
                    result.Add(BitConverter.GetBytes(((VersionsStand) task.Result.Stand).Version));
                    result.Add(BitConverter.GetBytes(mitarbeiter.Result.Mitarbeiter.Count));

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