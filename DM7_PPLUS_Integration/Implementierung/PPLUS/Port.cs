using System;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using DM7_PPLUS_Integration.Messages.PPLUS;
using NetMQ;
using NetMQ.Sockets;

namespace DM7_PPLUS_Integration.Implementierung.PPLUS
{
    public class Port : PPLUS_Handler
    {
        private readonly string _address;
        private readonly int _port_range_start;
        private readonly string _encryptionKey;
        private readonly Log _log;
        private readonly NetMQPoller _poller;
        private readonly Thread _thread;

        public Port(string address, int portRangeStart, string encryptionKey, Log log)
        {
            _address = address;
            _port_range_start = portRangeStart;
            _encryptionKey = encryptionKey;
            _log = log;

            _poller = new NetMQPoller();
            _thread = new Thread(() =>
            {
                using (var notification_socket = new SubscriberSocket($"{_address}:{Adapter.Notification_Port(_port_range_start)}"))
                {
                    notification_socket.Subscribe(Adapter.Dienste_Topic);
                    notification_socket.Subscribe(Adapter.Mitarbeiter_Topic);
                    notification_socket.ReceiveReady += (_, e) => Notification_empfangen(e.Socket);

                    using (_poller)
                    {
                        _poller.Add(notification_socket);
                        _poller.Run();
                    }
                }
            });
            _thread.Start();
            
        }

        private void Notification_empfangen(NetMQSocket socket)
        {
            var topic = socket.ReceiveFrameString();
            socket.SkipFrame(); // Benachrichtigungen haben keinen Payload

            new Thread(() =>
            {
                if (string.Equals(topic, Adapter.Dienste_Topic)) Dienständerungen_liegen_bereit?.Invoke();
                if (string.Equals(topic, Adapter.Mitarbeiter_Topic)) Mitarbeiteränderungen_liegen_bereit?.Invoke();
            }).Start();
        }

        public Task<Token?> Authenticate(string user, string password, TimeSpan? timeout = null)
        {
            return Task.Run<Token?>(() =>
            {
                using (var socket = new RequestSocket($"{_address}:{Adapter.Authentication_Port(_port_range_start)}"))
                using (var encryption = Encryption.From_encoded_Key(_encryptionKey))
                {
                    _log.Debug("Authenticate");
                    socket.SendFrame(encryption.Encrypt(new AuthenticationRequest(user, password).Encoded()));
                    var result = Encoding.AuthenticationResult_Decoded(ReceiveBytes(socket, timeout));

                    switch (result)
                    {
                        case AuthenticationSucceeded succeeded:
                        {
                            _log.Debug("Authenticated");
                            return new Token(succeeded.Token);
                        }
                        case AuthenticationFailed failed:
                        {
                            _log.Info($"Nicht authentifiziert: {failed.Reason}");
                            return null;
                        }
                        default: return null;
                    }
                }
            });
        }

        public Task<Capabilities> Capabilities(TimeSpan? timeout = null)
        {
            return Task.Run(() =>
            {
                using (var socket = new RequestSocket($"{_address}:{Adapter.Capabilities_Port(_port_range_start)}"))
                {
                    _log.Debug("Capabilities laden...");
                    socket.SendFrameEmpty();
                    var capabilities = Messages.PPLUS.Capabilities.Decoded(ReceiveBytes(socket, timeout));
                    _log.Debug("Capabilities geladen:");
                    foreach (var capability in capabilities.Value) _log.Debug($" - {capability}");
                    return capabilities;
                }
            });
        }

        public Task<QueryResult> HandleQuery(Token token, Query query, TimeSpan? timeout = null)
        {
            return Task.Run(() =>
            {
                using (var socket = new RequestSocket($"{_address}:{Adapter.Query_Port(_port_range_start)}"))
                using (var encryption = Encryption.From_encoded_Key(_encryptionKey))
                {
                    _log.Debug($"QUERY: '{query.GetType()}'");
                    socket.SendFrame(new QueryMessage(token.Value, encryption.Encrypt(Encoding.Query_Encoded(query))).Encoded());
                    var message = Encoding.ResponseMessage_Decoded(ReceiveBytes(socket, timeout));

                    switch (message)
                    {
                        case QueryFailed failed:
                            return new IOFehler(failed.Reason);

                        case QuerySucceeded succeeded:
                        {
                            try
                            {
                                var response = Encoding.QueryResult_Decoded(encryption.Decrypt(succeeded.Value));
                                _log.Debug($"RESPONSE: '{response.GetType()}'");
                                return response;
                            }
                            catch (CryptographicException e)
                            {
                                _log.Info($"Unbekannte Verschlüsselung! Bitte gleichen Sie den benutzten Schlüssel mit P-PLUS ab: {e.Message}");
                                return new IOFehler("Unbekannte Verschlüsselung! Bitte gleichen Sie den benutzten Schlüssel mit P-PLUS ab");
                            }
                        }

                        default:
                            return new IOFehler($"'{message.GetType()}' wurde in 'HandleQuery' im Port nicht behandelt");
                    }
                }
            });
        }

        public Task<CommandResult> HandleCommand(Token token, Command command, TimeSpan? timeout = null)
        {
            return Task.Run(() =>
            {
                using (var socket = new RequestSocket($"{_address}:{Adapter.Command_Port(_port_range_start)}"))
                using (var encryption = Encryption.From_encoded_Key(_encryptionKey))
                {
                    _log.Debug($"COMMAND: '{command.GetType()}'");
                    socket.SendFrame(new CommandMessage(token.Value, encryption.Encrypt(Encoding.Command_Encoded(command))).Encoded());
                    var message = Encoding.CommandResponseMessage_Decoded(ReceiveBytes(socket, timeout));

                    switch (message)
                    {
                        case CommandFailed failed:
                            return new IOFehler(failed.Reason);

                        case CommandSucceeded succeeded:
                        {
                            try
                            {
                                var response = Encoding.CommandResult_Decoded(encryption.Decrypt(succeeded.Value));
                                _log.Debug($"RESPONSE: '{response.GetType()}'");
                                return response;
                            }
                            catch (CryptographicException e)
                            {
                                _log.Info($"Unbekannte Verschlüsselung! Bitte gleichen Sie den benutzten Schlüssel mit P-PLUS ab: {e.Message}");
                                return new IOFehler("Unbekannte Verschlüsselung! Bitte gleichen Sie den benutzten Schlüssel mit P-PLUS ab");
                            }
                        }

                        default:
                            return new IOFehler($"'{message.GetType()}' wurde in 'HandleCommand' im Port nicht behandelt");
                    }
                }
            });
        }

        public event Action Mitarbeiteränderungen_liegen_bereit;
        public event Action Dienständerungen_liegen_bereit;

        private static byte[] ReceiveBytes(NetMQSocket socket, TimeSpan? timeout = null)
        {
            if (!timeout.HasValue) return socket.ReceiveFrameBytes();
            if (socket.TryReceiveFrameBytes(timeout.Value, out var bytes)) return bytes;
            throw new TimeoutException();
        }

        public void Dispose()
        {
            _poller?.Stop();
            var gracefullyStopped = _thread.Join(TimeSpan.FromSeconds(10));
            if (!gracefullyStopped) _thread.Abort();
        }
    }
}