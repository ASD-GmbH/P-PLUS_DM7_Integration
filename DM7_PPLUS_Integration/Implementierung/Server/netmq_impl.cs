using System;
using System.Threading.Tasks;
using DM7_PPLUS_Integration.Implementierung.Protokoll;
using DM7_PPLUS_Integration.Implementierung.Shared;
using NetMQ;
using NetMQ.Sockets;

namespace DM7_PPLUS_Integration.Implementierung.Server
{
    internal static class Constants
    {
        public const int NETMQ_WIREPROTOCOL_1 = 1;
        public const int NETMQ_NOTIFICATIONPROTOCOL_1 = 1;
        public const int CHANNEL_1_SERVICE = 1;
        public const int CHANNEL_2_DATA = 2;

        public const int CONNECTION_RESPONSE_OK = 1;
        public const int CONNECTION_RESPONSE_FAILURE = 2;

        public const int SERVICE_PROTOCOL_1 = 1;
        public const int SERVICE_CONNECT = 1;
    }

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
        /// <param name="networkaddress"></param>
        /// <param name="log"></param>
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
                _log.Debug($"NetMQ Request Socket wird geschlossen...");
                disposable1.Dispose();
                _log.Debug($"NetMQ Request Socket geschlossen.");
            }

            var disposable2 = _subscriber_socket;
            _subscriber_socket = null;
            if (disposable2 != null)
            {
                _log.Debug($"NetMQ Subscriber Socket wird geschlossen...");
                disposable2.Dispose();
                _log.Debug($"NetMQ Subscriber Socket geschlossen.");
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

    /// <summary>
    /// Empfängt serialisierte Nachrichten über ZeroMQ und gibt sie weiter an das Backend
    /// </summary>
    public class NetMQ_Server : IDisposable
    {
        private readonly Ebene_3_Protokoll__Data _backend;
        private readonly Ebene_3_Protokoll__Service _service;
        private readonly Log _log;
        private ResponseSocket _response_socket;
        private PublisherSocket _publisherSocket;
        private NetMQPoller _poller;
        private IDisposable _subscription;

        /// <summary>
        /// Empfängt serialisierte Nachrichten über ZeroMQ und gibt sie weiter an das Backend
        /// </summary>
        public NetMQ_Server(Ebene_3_Protokoll__Service service, Ebene_3_Protokoll__Data backend, string connectionstring, Log log)
        {
            _log = log;
            _backend = backend;
            _service = service;

            _response_socket = new ResponseSocket(connectionstring);
            _poller = new NetMQPoller();
            _response_socket.ReceiveReady += _response_socket_ReceiveReady;
            _poller.Add(_response_socket);
            log.Debug($"NetMQ response socket wird bereitgestellt ({connectionstring})...");
            _poller.RunAsync();

            var next_available_port = Next_available_port(connectionstring);
            log.Debug($"NetMQ publisher socket wird bereitgestellt ({next_available_port})...");
            _publisherSocket = new PublisherSocket(next_available_port);
            _subscription = backend.Notifications.Subscribe(new Observer<byte[]>(
                notification =>
                {
                    log.Debug("-- zmq");
                    _publisherSocket.SendFrame(new byte[] { Constants.NETMQ_WIREPROTOCOL_1 }, true);
                    _publisherSocket.SendFrame(notification);
                },
                ex => { throw new ConnectionErrorException($"Interner Fehler im Notificationstream: {ex.Message}", ex); }));

            log.Info("NetMQ Server wurde gestartet");
        }

        public static string Next_available_port(string connectionstring)
        {
            var elements = connectionstring.Split(':');
            if (elements.Length != 3) throw new ConnectionErrorException($"Ungültiger Connectionport in NetMQ pushlisher: {connectionstring}");
            return $"{elements[0]}:{elements[1]}:{int.Parse(elements[2]) + 1}";
        }

        private void _response_socket_ReceiveReady(object sender, NetMQSocketEventArgs e)
        {
            _log.Debug("NetMQ Nachricht wird gelesen...");
            var data = e.Socket.ReceiveFrameBytes();
            Guard_unsupported_protocol(data[0]);
            switch (data[1])
            {
                case Constants.CHANNEL_1_SERVICE:
                {
                    var request = e.Socket.ReceiveFrameBytes();
                    _log.Debug($"NetMQ Service Request ({request.Length} bytes)...");
                    var response = _service.ServiceRequest(request).Result;
                    _log.Debug($"NetMQ Response ({response.Length} bytes)...");
                    e.Socket.SendFrame(response);
                }
                    break;
                case Constants.CHANNEL_2_DATA:
                {
                    var request = e.Socket.ReceiveFrameBytes();
                    _log.Debug($"NetMQ Data Request ({request.Length} bytes)...");
                    var response = _backend.Request(request).Result;
                    _log.Debug($"NetMQ Response ({response.Length} bytes)...");
                    e.Socket.SendFrame(response);
                }
                    break;
                default:
                    throw new ConnectionErrorException($"Unbekannter NetMQ Server Kanal: {data[0].ToString()}");
            }
        }

        private void Guard_unsupported_protocol(int protocol)
        {
            if (protocol != Constants.NETMQ_WIREPROTOCOL_1) throw new UnsupportedVersionException($"NetMQ Protokoll Version {protocol.ToString()} wird von dieser P-PLUS Version nicht unterstützt.");
        }

        public void Dispose()
        {
            _log.Info("NetMQ Server wird beendet...");

            var disposable2 = _poller;
            _poller = null;
            if (disposable2 != null)
            {
                _log.Debug("NetMQ Poller wird angehalten...");
                disposable2.Stop();
                _log.Debug("NetMQ Poller wird geschlossen...");
                disposable2.Dispose();
            }
            else
            {
                _log.Info("NetMQ Poller war bereits geschlossen!");
            }


            var disposable1 = _response_socket;
            _response_socket = null;

            if (disposable1 != null)
            {
                _log.Debug("NetMQ Request socket wird geschlossen...");
                disposable1.Dispose();
            }
            else
            {
                _log.Info("NetMQ Request socket war bereits geschlossen!");
            }

            var disposable3 = _publisherSocket;
            _publisherSocket = null;

            if (disposable3 != null)
            {
                _log.Debug("NetMQ Publisher socket wird geschlossen...");
                disposable3.Dispose();
            }
            else
            {
                _log.Info("NetMQ Publisher socket war bereits geschlossen!");
            }


            _log.Info("NetMQ Server wurde beendet.");
        }
    }


}