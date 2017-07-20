using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DM7_PPLUS_Integration.Implementierung.Protokoll;
using DM7_PPLUS_Integration.Implementierung.Shared;
using DM7_PPLUS_Integration.Implementierung.Testing;
using NetMQ;
using NetMQ.Sockets;

namespace DM7_PPLUS_Integration.Implementierung.Server
{
    internal static class Constants
    {
        public const int NETMQ_WIREPROTOCOL_1 = 1;
        public const int CHANNEL_1_SERVICE = 1;
        public const int CHANNEL_2_DATA = 2;

        public const int CONNECTION_RESPONSE_OK = 1;
        public const int CONNECTION_RESPONSE_FAILURE = 2;

        public const int SERVICE_PROTOCOL_1 = 1;
        public const int SERVICE_CONNECT = 1;
    }

    public class NetMQ_Client : Ebene_3_Protokoll__Netzwerkuebertragung, Ebene_3_Protokoll__Netzwerkuebertragung_Service, IDisposable
    {
        private RequestSocket _request_socket;
        private readonly Subject<byte[]> _notifications;

        public NetMQ_Client(string networkaddress)
        {
            _notifications = new Subject<byte[]>();
            _request_socket = new NetMQ.Sockets.RequestSocket(networkaddress);
        }

        // sendet serialisierte Nachrichten über ZeroMQ an NetMQ_Server
        public void Dispose()
        {
            var disposable1 = _request_socket;
            _request_socket = null;
            if (disposable1 != null) disposable1.Dispose();
        }

        Task<byte[]> Ebene_3_Protokoll__Netzwerkuebertragung_Service.ServiceRequest(byte[] request)
        {
            return Task(Constants.CHANNEL_1_SERVICE, request);
        }

        Task<byte[]> Ebene_3_Protokoll__Netzwerkuebertragung.Request(byte[] request)
        {
            return Task(Constants.CHANNEL_2_DATA, request);
        }

        private Task<byte[]> Task(byte channel, byte[] request)
        {
            var task = new Task<byte[]>(() =>
            {
                _request_socket.SendFrame(new byte[2] {Constants.NETMQ_WIREPROTOCOL_1, channel}, true);
                _request_socket.SendFrame(request, false);
                return _request_socket.ReceiveFrameBytes();
            });
            task.RunSynchronously();
            return task;
        }

        IObservable<byte[]> Ebene_3_Protokoll__Netzwerkuebertragung.Notifications => _notifications;
    }

    /// <summary>
    /// Empfängt serialisierte Nachrichten über ZeroMQ und gibt sie weiter an das Backend
    /// </summary>
    public class NetMQ_Server : IDisposable
    {
        private readonly Ebene_3_Protokoll__Netzwerkuebertragung _backend;
        private ResponseSocket _response_socket;

        /// <summary>
        /// Empfängt serialisierte Nachrichten über ZeroMQ und gibt sie weiter an das Backend
        /// </summary>
        public NetMQ_Server(Ebene_3_Protokoll__Netzwerkuebertragung backend, string connectionstring)
        {
            _backend = backend;
            _response_socket = new NetMQ.Sockets.ResponseSocket(connectionstring);
            var poller = new NetMQPoller();
            _response_socket.ReceiveReady += _response_socket_ReceiveReady;
            poller.Add(_response_socket);
            poller.RunAsync();
        }

        private void _response_socket_ReceiveReady(object sender, NetMQSocketEventArgs e)
        {
            _backend.Request(e.Socket.ReceiveFrameBytes()).ContinueWith(task => e.Socket.SendFrame(task.Result));
        }

        public void Dispose()
        {
            var disposable1 = _response_socket;
            _response_socket = null;
            if (disposable1 != null) disposable1.Dispose();
        }
    }


}