using System;
using System.Threading.Tasks;
using DM7_PPLUS_Integration.Implementierung.Protokoll;
using DM7_PPLUS_Integration.Implementierung.Server;
using DM7_PPLUS_Integration.Implementierung.Shared;
using NetMQ;
using NetMQ.Sockets;

namespace DM7_PPLUS_Integration.Implementierung.Client
{
    public class NetMQ_Client : Ebene_3_Protokoll__Data, Ebene_3_Protokoll__Service
    {
        private readonly Log _log;
        private RequestSocket _request_socket;
        private readonly Subject<byte[]> _notifications;
        private SubscriberSocket _subscriber_socket;
        private NetMQPoller _poller;

        /// <summary>
        ///  Sendet serialisierte Nachrichten über ZeroMQ an NetMQ_Server
        /// </summary>
        public NetMQ_Client(string networkaddress, Log log)
        {
            _log = log;
            _notifications = new Subject<byte[]>();

            _log.Debug($"NetMQ Request Socket ({networkaddress}) wird verbunden.");
            _request_socket = new RequestSocket(networkaddress);

            var next_available_port = NetMQ_Server.Next_available_port(networkaddress);
            _log.Debug($"NetMQ Subscriber Socket ({next_available_port}) wird verbunden.");
            _subscriber_socket = new SubscriberSocket(next_available_port);
            _subscriber_socket.Subscribe("");

            _poller = new NetMQPoller();
            _subscriber_socket.ReceiveReady += _subscriber_socket_receive_ready;
            _poller.Add(_subscriber_socket);
            _poller.RunAsync();
        }

        private void _subscriber_socket_receive_ready(object sender, NetMQSocketEventArgs e)
        {
            _log.Debug("NetMQ notification wird gelesen...");
            var data = e.Socket.ReceiveMultipartBytes(2);
            Guard_unsupported_protocol(data[0][0]);
            var notification = data[1];
            _log.Debug($"NetMQ notification ({notification.Length} bytes)...");
            _notifications.Next(notification);
        }

        private void Guard_unsupported_protocol(int protocol)
        {
            if (protocol != Constants.NETMQ_WIREPROTOCOL_1) throw new UnsupportedVersionException($"NetMQ Protokoll Version {protocol.ToString()} wird von dieser P-PLUS Version nicht unterstützt.");
        }

        public void Dispose()
        {
            var disposable0 = _poller;
            _poller = null;
            if (disposable0 != null)
            {
                _log.Debug("NetMQ Poller wird angehalten...");
                disposable0.Stop();
                _log.Debug("NetMQ Poller wird geschlossen...");
                disposable0.Dispose();
            }
            else
            {
                _log.Info("NetMQ Poller war bereits geschlossen!");
            }

            var disposable1 = _request_socket;
            _request_socket = null;
            if (disposable1 != null)
            {
                _log.Debug("NetMQ Request Socket wird geschlossen...");
                disposable1.Dispose();
                _log.Debug("NetMQ Request Socket geschlossen.");
            }

            var disposable2 = _subscriber_socket;
            _subscriber_socket = null;
            if (disposable2 != null)
            {
                _log.Debug("NetMQ Subscriber Socket wird geschlossen...");
                disposable2.Dispose();
                _log.Debug("NetMQ Subscriber Socket geschlossen.");
            }
        }

        Task<byte[]> Ebene_3_Protokoll__Service.ServiceRequest(byte[] request)
        {
            return Task(Constants.CHANNEL_1_SERVICE, request);
        }

        Task<byte[]> Ebene_3_Protokoll__Data.Request(byte[] request)
        {
            return Task(Constants.CHANNEL_2_DATA, request);
        }

        private Task<byte[]> Task(byte channel, byte[] request)
        {
            var task = new Task<byte[]>(() =>
            {
                _log.Debug($"NetMQ sende Request (channel {channel}, {request.Length} bytes)...");
                _request_socket.SendFrame(new byte[] {Constants.NETMQ_WIREPROTOCOL_1, channel}, true);
                _request_socket.SendFrame(request);
                var response = _request_socket.ReceiveFrameBytes();
                _log.Debug($"NetMQ Response empfangen ({response.Length} bytes)...");
                return response;
            });
            task.RunSynchronously();
            return task;
        }

        IObservable<byte[]> Ebene_3_Protokoll__Data.Notifications => _notifications;
    }
}