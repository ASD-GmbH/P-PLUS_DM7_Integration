using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DM7_PPLUS_Integration.Implementierung.Protokoll;
using DM7_PPLUS_Integration.Implementierung.Shared;

namespace DM7_PPLUS_Integration.Implementierung.Client
{
    internal class Data_Proxy : DisposeGroupMember, Schicht_2_Protokoll__API_Version_unabhaengige_Uebertragung
    {
        private readonly Schicht_3_Protokoll__Data _proxy;

        public Data_Proxy(Schicht_3_Protokoll__Data proxy, DisposeGroup disposegroup) : base(disposegroup)
        {
            _proxy = proxy;
            disposegroup.With(() => _proxy.Dispose());

            var subject = new Subject<Notification>();
            var subscription = proxy.Notifications.Subscribe(new Observer<byte[]>(
                datagram =>
                {
                    var notification = Deserialize(datagram);
                    subject.Next(notification);
                },
                ex =>
                {
                    throw new ConnectionErrorException($"Interner Fehler im Notificationstream: {ex.Message}", ex);
                }));
            disposegroup.With(() => subscription.Dispose());

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

        public Task<QueryResponse> Query(string credentials, int api_version, Guid session, int datenquelle, long von, long bis)
        {
            var credentialsBuffer = System.Text.Encoding.UTF8.GetBytes(credentials);
            var query = new List<byte[]>
            {
                BitConverter.GetBytes(credentialsBuffer.Length),
                credentialsBuffer,
                session.ToByteArray(),
                BitConverter.GetBytes(api_version),
                BitConverter.GetBytes(datenquelle),
                BitConverter.GetBytes(von),
                BitConverter.GetBytes(bis)
            };

            return _proxy
                .Request(query.Concat())
                .ContinueWith(task => (QueryResponse) new QueryResult(task.Result));
        }

        public Task<QueryResponse> Query_Dienste(string credentials, int api_version, Guid session)
        {
            var credentialsBuffer = System.Text.Encoding.UTF8.GetBytes(credentials);
            var query = new List<byte[]>
            {
                BitConverter.GetBytes(credentialsBuffer.Length),
                credentialsBuffer,
                session.ToByteArray(),
                BitConverter.GetBytes(api_version)
            };

            return _proxy
                .Request_Dienste(query.Concat())
                .ContinueWith(task => (QueryResponse)new QueryResult(task.Result));
        }

        public IObservable<Notification> Notifications { get; }
    }
}