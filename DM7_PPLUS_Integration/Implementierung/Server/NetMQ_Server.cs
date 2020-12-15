using System;
using System.Collections.Generic;
using System.IO;
using System.Security;
using System.Security.Cryptography;
using System.Threading.Tasks;
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
        private readonly Schicht_3_Protokoll__Data _backend;
        private readonly Schicht_3_Protokoll__Service _service;
        private readonly Log _log;
        private readonly RSAParameters _rsakey;

        /// <summary>
        /// Empfängt serialisierte Nachrichten über ZeroMQ und gibt sie weiter an das Backend
        /// </summary>
        public static void Start(Schicht_3_Protokoll__Service service, Schicht_3_Protokoll__Data backend, string connectionstring, string privateKey, Log log, DisposeGroup disposegroup)
        {
            // ReSharper disable once ObjectCreationAsStatement
            new NetMQ_Server(service, backend, connectionstring, privateKey, log, disposegroup);
        }
        private NetMQ_Server(Schicht_3_Protokoll__Service service, Schicht_3_Protokoll__Data backend, string connectionstring, string privateKey, Log log, DisposeGroup disposegroup) : base(disposegroup)
        {
            disposegroup.With(() => _log.Info("NetMQ Server wurde beendet."));

            using (var rsa = new RSACryptoServiceProvider())
            {
                var xml = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(privateKey));
                rsa.FromXmlString(xml);
                _rsakey = rsa.ExportParameters(true);
            }

            _log = log;
            _backend = backend;
            _service = service;

            var responseSocket = Init_response_socket(connectionstring, disposegroup);

            Init_publisher_socket(backend, connectionstring, disposegroup);

            Init_poller(disposegroup, responseSocket);

            _log.Info("NetMQ Server wurde gestartet");
            disposegroup.With(() => _log.Info("NetMQ Server wird beendet..."));
        }

        private void Init_poller(DisposeGroup disposegroup, ResponseSocket responseSocket)
        {
            var poller = new NetMQPoller();
            disposegroup.With(() =>
            {
                _log.Debug("NetMQ Poller wird geschlossen...");
                poller.Dispose();
            });
            poller.Add(responseSocket);
            poller.RunAsync();
        }

        private static readonly SHA256 hash = SHA256.Create();
        private void Init_publisher_socket(Schicht_3_Protokoll__Data backend, string connectionstring, DisposeGroup disposegroup)
        {
            var publisher_port = Next_available_port(connectionstring);
            _log.Debug($"NetMQ publisher socket wird bereitgestellt ({publisher_port})...");
            var publisherSocket = new PublisherSocket(publisher_port);
            disposegroup.With(() =>
            {
                _log.Debug("NetMQ Publisher socket wird geschlossen...");
                publisherSocket.Dispose();
            });

            var subscription = backend.Notifications.Subscribe(new Observer<byte[]>(
                notification =>
                {
                    publisherSocket.SendFrame(new byte[] {Constants.NETMQ_NOTIFICATIONPROTOCOL_2}, true);
                    publisherSocket.SendFrame(notification, true);
                    using (var data = new MemoryStream(notification))
                    using (var rsa = new RSACryptoServiceProvider())
                    {
                        rsa.ImportParameters(_rsakey);
                        publisherSocket.SendFrame(rsa.SignData(data, hash));
                    }
                },
                ex => throw new ConnectionErrorException($"Interner Fehler im Notificationstream: {ex.Message}", ex)));
            disposegroup.With(subscription);
        }

        private ResponseSocket Init_response_socket(string connectionstring, DisposeGroup disposegroup)
        {
            _log.Debug($"NetMQ Request socket wird bereitgestellt ({connectionstring})...");
            var responseSocket = new ResponseSocket(connectionstring);
            responseSocket.ReceiveReady += _response_socket_ReceiveReady;
            disposegroup.With(() =>
            {
                _log.Debug("NetMQ Request socket wird geschlossen...");
                responseSocket.Dispose();
            });
            return responseSocket;
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

            var frames = new Queue<byte[]>(e.Socket.ReceiveMultipartBytes());

            try
            {
                var data = frames.Dequeue();
                Guard_unsupported_protocol(data[0]);

                var key = DecryptKey(frames.Dequeue());
                var iv = frames.Dequeue();

                using (var aes = new CryptoService(key, iv))
                {

                    switch (data[1])
                    {
                        case Constants.CHANNEL_1_SERVICE:
                            Handle(e, aes, frames, "Service Request", _service.ServiceRequest);
                            break;

                        case Constants.CHANNEL_2_DATA:
                            Handle(e, aes, frames, "Data Request", _backend.Request);
                            break;

                        default:
                            throw new ConnectionErrorException($"Unbekannter NetMQ Server Kanal: {data[0].ToString()}");
                    }
                }
            }
            catch (CryptographicException)
            {
                var info = "Fehler beim Bearbeiten einer Anfrage: Ungültiger Schlüssel.";
                _log.Info(info);
                e.Socket.SendFrame(new byte[] { 9, 9, 9 });
            }
            catch (Exception ex)
            {
                _log.Info($"Fehler beim Bearbeiten einer Anfrage: {ex.Message}.\r\n\r\n{ex}");
                e.Socket.SendFrame(new byte[] { 9, 9, 0 });
            }
        }

        private void Handle(NetMQSocketEventArgs e, CryptoService aes, Queue<byte[]> frames, string requestinfo, Func<byte[], Task<byte[]>> requestHandler)
        {
            var request = aes.Decrypt(frames.Dequeue());
            try
            {
                _log.Debug($"NetMQ {requestinfo} ({request.Length} bytes)...");
                var response = requestHandler(request).Result;
                _log.Debug($"NetMQ Response ({response.Length} bytes)...");
                e.Socket.SendFrame(aes.Encrypt(response));
            }
            catch (SecurityException ex)
            {
                _log.Info(ex.Message);
                var response = ErrorResponse(ConnectionFailure.Unauthorized, ex.Message);
                e.Socket.SendFrame(aes.Encrypt(response));
            }
            catch (Exception ex)
            {
                var error = $"Fehler beim Bearbeiten eines Service-Requests: {ex.Message}";
                _log.Info($"{error}\r\n{ex}");
                var response = ErrorResponse(ConnectionFailure.Internal_Server_Error, error);
                e.Socket.SendFrame(aes.Encrypt(response));
            }
        }

        private static byte[] ErrorResponse(ConnectionFailure errorCode, string message)
        {
            var info = System.Text.Encoding.UTF8.GetBytes(message);
            var response = new List<byte[]>
            {
                new byte[] {Constants.CONNECTION_RESPONSE_FAILURE},
                BitConverter.GetBytes((int) errorCode),
                BitConverter.GetBytes(info.Length),
                info
            }.Concat();
            return response;
        }

        private byte[] DecryptKey(byte[] encryptedKey)
        {
            using (var rsa = new RSACryptoServiceProvider())
            {
                rsa.ImportParameters(_rsakey);
                return rsa.Decrypt(encryptedKey, false);
            }
        }

        private void Guard_unsupported_protocol(int protocol)
        {
            if (protocol != Constants.NETMQ_WIREPROTOCOL_2) throw new UnsupportedVersionException($"NetMQ Protokoll Version {protocol.ToString()} wird von dieser P-PLUS Version nicht unterstützt.");
        }
    }


}