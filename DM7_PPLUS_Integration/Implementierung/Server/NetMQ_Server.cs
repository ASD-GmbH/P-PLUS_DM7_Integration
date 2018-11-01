﻿using System;
using System.Security.Cryptography;
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
        private readonly RSAParameters _rsakey;

        /// <summary>
        /// Empfängt serialisierte Nachrichten über ZeroMQ und gibt sie weiter an das Backend
        /// </summary>
        public static void Start(Ebene_3_Protokoll__Service service, Ebene_3_Protokoll__Data backend, string connectionstring, string privateKey, Log log, DisposeGroup disposegroup)
        {
            // ReSharper disable once ObjectCreationAsStatement
            new NetMQ_Server(service, backend, connectionstring, privateKey, log, disposegroup);
        }
        private NetMQ_Server(Ebene_3_Protokoll__Service service, Ebene_3_Protokoll__Data backend, string connectionstring, string privateKey, Log log, DisposeGroup disposegroup) : base(disposegroup)
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

        private void Init_publisher_socket(Ebene_3_Protokoll__Data backend, string connectionstring, DisposeGroup disposegroup)
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
                    publisherSocket.SendFrame(new byte[] {Constants.NETMQ_WIREPROTOCOL_2}, true);
                    // TODO RELEASE: implement encryption
                    publisherSocket.SendFrame(notification);
                },
                ex => { throw new ConnectionErrorException($"Interner Fehler im Notificationstream: {ex.Message}", ex); }));
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


            try
            {

                var data = e.Socket.ReceiveFrameBytes();
                Guard_unsupported_protocol(data[0]);

                var key = DecryptKey(e.Socket.ReceiveFrameBytes());
                var iv = e.Socket.ReceiveFrameBytes();

                using (var aes = new CryptoService(key, iv))
                {

                    switch (data[1])
                    {
                        case Constants.CHANNEL_1_SERVICE:
                        {
                            var request = aes.Decrypt(e.Socket.ReceiveFrameBytes());
                            _log.Debug($"NetMQ Service Request ({request.Length} bytes)...");
                            var response = _service.ServiceRequest(request).Result;
                            _log.Debug($"NetMQ Response ({response.Length} bytes)...");
                            e.Socket.SendFrame(aes.Encrypt(response));
                            break;
                        }

                        case Constants.CHANNEL_2_DATA:
                        {
                            var request = aes.Decrypt(e.Socket.ReceiveFrameBytes());
                            _log.Debug($"NetMQ Data Request ({request.Length} bytes)...");
                            var response = _backend.Request(request).Result;
                            _log.Debug($"NetMQ Response ({response.Length} bytes)...");
                            e.Socket.SendFrame(aes.Encrypt(response));
                            break;
                        }

                        default:
                            throw new ConnectionErrorException($"Unbekannter NetMQ Server Kanal: {data[0].ToString()}");
                    }
                }
            }
            catch (System.Security.Cryptography.CryptographicException ex)
            {
                _log.Info($"Fehler beim Bearbeiten einer Anfrage: Ungültiger Schlüssel.");
            }
            catch (Exception ex)
            {
                _log.Info($"Fehler beim Bearbeiten einer Anfrage: {ex.Message}.\r\n\r\n{ex.ToString()}");
            }
        }

        private byte[] DecryptKey(byte[] encryptedKey)
        {
            // TODO RELEASE: cache rsa decrypted keys
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