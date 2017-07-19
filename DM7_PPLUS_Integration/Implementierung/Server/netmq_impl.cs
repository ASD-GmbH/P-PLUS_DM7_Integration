using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DM7_PPLUS_Integration.Implementierung.Protokoll;
using DM7_PPLUS_Integration.Implementierung.Shared;
using DM7_PPLUS_Integration.Implementierung.Testing;

namespace DM7_PPLUS_Integration.Implementierung.Server
{
    public class NetMQ_Client : Ebene_3_Protokoll__Netzwerkuebertragung, IDisposable
    {

        public NetMQ_Client(string networkaddress)
        {
            Notifications = new Subject<byte[]>();
        }

        // sendet serialisierte Nachrichten über ZeroMQ an NetMQ_Server
        public void Dispose()
        {
        }

        public Task<byte[]> Request(byte[] request)
        {
            throw new NotImplementedException();
        }

        public IObservable<byte[]> Notifications { get; }
    }

    public class NetMQ_Server : IDisposable
    {
        private readonly Ebene_3_Protokoll__Netzwerkuebertragung _backend;

        public NetMQ_Server(Ebene_3_Protokoll__Netzwerkuebertragung backend, string tcp)
        {
            _backend = backend;
        }

        // empfängt serialisierte Nachrichten über ZeroMQ und gibt sie weiter an das Backend
        public Task<byte[]> Respond_to(byte[] request)
        {
            return _backend.Request(request);
        }

        public void Dispose()
        {
        }
    }


}