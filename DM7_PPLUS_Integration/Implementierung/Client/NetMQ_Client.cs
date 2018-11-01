using System;
using System.IO;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using DM7_PPLUS_Integration.Implementierung.Protokoll;
using DM7_PPLUS_Integration.Implementierung.Server;
using DM7_PPLUS_Integration.Implementierung.Shared;
using NetMQ;
using NetMQ.Sockets;

namespace DM7_PPLUS_Integration.Implementierung.Client
{
    internal class NetMQ_Client : DisposeGroupMember, Ebene_3_Protokoll__Data, Ebene_3_Protokoll__Service
    {
        private readonly Log _log;
        private readonly RequestSocket _request_socket;
        private readonly Subject<byte[]> _notifications;
        private readonly byte[] _key;
        private readonly byte[] _encryptedKey;

        /// <summary>
        ///  Sendet serialisierte Nachrichten �ber ZeroMQ an NetMQ_Server
        /// </summary>
        public NetMQ_Client(string networkaddress, /*byte[] publicKey, */Log log, DisposeGroup disposegroup, CancellationToken cancellationToken_Verbindung) : base(disposegroup)
        {
            _log = log;

            using (var aes = new AesManaged())
            {
                aes.GenerateKey();
                _key = aes.Key;
                _encryptedKey = aes.Key; // TODO RELEASE: implement RSA encryption
            }

            _notifications = new Subject<byte[]>();

            _log.Debug($"NetMQ Request Socket ({networkaddress}) wird verbunden.");
            _request_socket = new RequestSocket(networkaddress);
            disposegroup.With(() =>
            {
                _log.Debug("NetMQ Request Socket wird geschlossen...");
                _request_socket.Dispose();
                _log.Debug("NetMQ Request Socket geschlossen.");
            });

            var next_available_port = NetMQ_Server.Next_available_port(networkaddress);
            _log.Debug($"NetMQ Subscriber Socket ({next_available_port}) wird verbunden.");
            var subscriberSocket = new SubscriberSocket(next_available_port);
            subscriberSocket.Subscribe("");
            subscriberSocket.ReceiveReady += _subscriber_socket_receive_ready;
            disposegroup.With(() =>
            {
                _log.Debug("NetMQ Subscriber Socket wird geschlossen...");
                subscriberSocket.Dispose();
                _log.Debug("NetMQ Subscriber Socket geschlossen.");
            });

            var poller = new NetMQPoller {subscriberSocket};
            poller.RunAsync();
            disposegroup.With(() =>
            {
                _log.Debug("NetMQ Poller wird angehalten...");
                poller.Stop();
                _log.Debug("NetMQ Poller wird geschlossen...");
                poller.Dispose();
            });

            cancellationToken_Verbindung.Register(disposegroup.Dispose);
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
            if (protocol != Constants.NETMQ_WIREPROTOCOL_2) throw new UnsupportedVersionException($"NetMQ Protokoll Version {protocol.ToString()} wird von dieser P-PLUS Version nicht unterst�tzt.");
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
                using (var aes = new CryptoService(_key))
                {
                    _log.Debug($"NetMQ sende Request (channel {channel}, {request.Length} bytes)...");
                    _request_socket.SendFrame(new byte[] {Constants.NETMQ_WIREPROTOCOL_2, channel}, true);
                    _request_socket.SendFrame(_encryptedKey, true);
                    _request_socket.SendFrame(aes.IV, true);
                    _request_socket.SendFrame(aes.Encrypt(request));
                    var response = aes.Decrypt(_request_socket.ReceiveFrameBytes());
                    _log.Debug($"NetMQ Response empfangen ({response.Length} bytes)...");
                    return response;
                }
            });
            task.RunSynchronously();
            return task;
        }

        IObservable<byte[]> Ebene_3_Protokoll__Data.Notifications => _notifications;
    }
}