using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DM7_PPLUS_Integration.Implementierung.Protokoll;
using DM7_PPLUS_Integration.Implementierung.Shared;

namespace DM7_PPLUS_Integration.Implementierung.Server
{
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

        // empf�ngt serialisierte Nachrichten und gibt sie als API-Level-unabh�ngige Nachrichten an das Backend weiter
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
}