using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DM7_PPLUS_Integration.Implementierung.Protokoll;
using DM7_PPLUS_Integration.Implementierung.Shared;

namespace DM7_PPLUS_Integration.Implementierung.Client
{
    internal class Data_Proxy : Ebene_2_Protokoll__API_Level_unabhaengige_Uebertragung
    {
        private readonly Ebene_3_Protokoll__Data _proxy;
        private readonly IDisposable _subscription;

        public Data_Proxy(Ebene_3_Protokoll__Data proxy)
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
            _proxy.Dispose();
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
}