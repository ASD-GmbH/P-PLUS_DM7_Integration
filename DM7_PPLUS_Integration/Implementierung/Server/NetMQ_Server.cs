using DM7_PPLUS_Integration.Implementierung.Protokoll;
using DM7_PPLUS_Integration.Implementierung.Shared;
using NetMQ;
using NetMQ.Sockets;

namespace DM7_PPLUS_Integration.Implementierung.Server
{
    /// <summary>
    /// Empfängt serialisierte Nachrichten über ZeroMQ und gibt sie weiter an das Backend
    /// </summary>
    internal class NetMQ_Server : DisposeGroupMember
    {
        private readonly Ebene_3_Protokoll__Data _backend;
        private readonly Ebene_3_Protokoll__Service _service;
        private readonly Log _log;
        private readonly ResponseSocket _response_socket;
        private readonly PublisherSocket _publisherSocket;
        private readonly NetMQPoller _poller;

        /// <summary>
        /// Empfängt serialisierte Nachrichten über ZeroMQ und gibt sie weiter an das Backend
        /// </summary>
        public static void Start(Ebene_3_Protokoll__Service service, Ebene_3_Protokoll__Data backend, string connectionstring, Log log, DisposeGroup disposegroup)
        {
            // ReSharper disable once ObjectCreationAsStatement
            new NetMQ_Server(service, backend, connectionstring, log, disposegroup);
        }
        private NetMQ_Server(Ebene_3_Protokoll__Service service, Ebene_3_Protokoll__Data backend, string connectionstring, Log log, DisposeGroup disposegroup) : base(disposegroup)
        {
            disposegroup.With(() => _log.Info("NetMQ Server wurde beendet."));

            _log = log;
            _backend = backend;
            _service = service;

            log.Debug($"NetMQ Request socket wird bereitgestellt ({connectionstring})...");
            _response_socket = new ResponseSocket(connectionstring);
            _response_socket.ReceiveReady += _response_socket_ReceiveReady;
            disposegroup.With(() =>
            {
                _log.Debug("NetMQ Request socket wird geschlossen...");
                _response_socket.Dispose();
            });

            var publisher_port = Next_available_port(connectionstring);
            log.Debug($"NetMQ publisher socket wird bereitgestellt ({publisher_port})...");
            _publisherSocket = new PublisherSocket(publisher_port);
            disposegroup.With(() =>
            {
                _log.Debug("NetMQ Publisher socket wird geschlossen...");
                _publisherSocket.Dispose();
            });

            var subscription = backend.Notifications.Subscribe(new Observer<byte[]>(
                notification =>
                {
                    _publisherSocket.SendFrame(new byte[] { Constants.NETMQ_WIREPROTOCOL_1 }, true);
                    _publisherSocket.SendFrame(notification);
                },
                ex => { throw new ConnectionErrorException($"Interner Fehler im Notificationstream: {ex.Message}", ex); }));
            disposegroup.With(subscription);

            _poller = new NetMQPoller();
            disposegroup.With(() =>
            {
                _log.Debug("NetMQ Poller wird geschlossen...");
                _poller.Dispose();
            });
            _poller.Add(_response_socket);
            _poller.RunAsync();

            log.Info("NetMQ Server wurde gestartet");
            disposegroup.With(() => _log.Info("NetMQ Server wird beendet..."));
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
    }


}