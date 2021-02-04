using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DM7_PPLUS_Integration.Implementierung.Protokoll;
using DM7_PPLUS_Integration.Implementierung.Shared;

namespace DM7_PPLUS_Integration.Implementierung.Server
{
    internal class Data_Adapter : DisposeGroupMember, Schicht_3_Protokoll__Data
    {
        private readonly Schicht_2_Protokoll__API_Version_unabhaengige_Uebertragung _backend;

        public Data_Adapter(Schicht_2_Protokoll__API_Version_unabhaengige_Uebertragung backend, Log log, DisposeGroup disposegroup) : base(disposegroup)
        {
            _backend = backend;
            var subject = new Subject<byte[]>();
            var subscription = backend.Notifications.Subscribe(new Observer<Notification>(
                notification =>
                {
                    var datagram = Serialize_Notification(notification);
                    subject.Next(datagram);
                },
                ex => { throw new ConnectionErrorException($"Interner Fehler im Notificationstream: {ex.Message}", ex); }));
            disposegroup.With(subscription);
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

        // empfängt serialisierte Nachrichten und gibt sie als API-Version-unabhängige Nachrichten an das Backend weiter

        public Task<byte[]> Request(byte[] request)
        {
            var guidbuffer = new byte[16];
            var position = 0;

            var credentialsbufferlength = BitConverter.ToInt32(request, position);
            position += 4;
            var credentials = System.Text.Encoding.UTF8.GetString(request, position, credentialsbufferlength);
            position += credentialsbufferlength;

            Array.Copy(request, position, guidbuffer, 0, 16);
            var session = new Guid(guidbuffer);
            position += 16;

            var api_version = BitConverter.ToInt32(request, position);
            position += 4;

            var datenquelle = BitConverter.ToInt32(request, position);
            position += 4;

            var von = BitConverter.ToInt64(request, position);
            position += 8;

            var bis = BitConverter.ToInt64(request, position);

            return
                _backend
                    .Query_Mitarbeiterdatensätze(credentials, api_version, session, datenquelle, von, bis)
                    .ContinueWith(task => Serialize_QueryResponse(task.Result));
        }

        // empfängt serialisierte Nachrichten und gibt sie als API-Version-unabhängige Nachrichten an das Backend weiter

        public Task<byte[]> Request_Dienste(byte[] request)
        {
            var guidbuffer = new byte[16];
            var position = 0;

            var credentialsbufferlength = BitConverter.ToInt32(request, position);
            position += 4;
            var credentials = System.Text.Encoding.UTF8.GetString(request, position, credentialsbufferlength);
            position += credentialsbufferlength;

            Array.Copy(request, position, guidbuffer, 0, 16);
            var session = new Guid(guidbuffer);
            position += 16;

            var api_version = BitConverter.ToInt32(request, position);

            return
                _backend
                    .Query_Dienste(credentials, api_version, session)
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