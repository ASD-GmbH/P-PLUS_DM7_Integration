using System.Security.Cryptography;
using System.Threading.Tasks;
using Bare.Msg;
using NetMQ;
using NetMQ.Sockets;

namespace DM7_PPLUS_Integration.Implementierung
{
    public class Port : PPLUS_Handler
    {
        private readonly string _address;
        private readonly int _port_range_start;
        private readonly string _encryptionKey;
        private readonly Log _log;

        public Port(string address, int portRangeStart, string encryptionKey, Log log)
        {
            _address = address;
            _port_range_start = portRangeStart;
            _encryptionKey = encryptionKey;
            _log = log;
        }

        public Task<Token?> Authenticate(string user, string password)
        {
            return Task.Run<Token?>(() =>
            {
                using (var socket = new RequestSocket($"{_address}:{Adapter.Authentication_Port(_port_range_start) }"))
                using (var encryption = Encryption.From_encoded_Key(_encryptionKey))
                {
                    _log.Debug("Authenticate");
                    socket.SendFrame(encryption.Encrypt(new Authentication_Request(user, password).Encoded()));
                    var result= Encoding.Authentication_Result_Decoded(socket.ReceiveFrameBytes());

                    switch (result)
                    {
                        case Authentication_Succeeded succeeded:
                        {
                            _log.Debug("Authenticated");
                            return new Token(succeeded.Token);
                        }
                        case Authentication_Failed failed:
                        {
                            _log.Info($"Nicht authentifiziert: {failed.Reason}");
                            return null;
                        }
                        default: return null;
                    }
                }
            });
        }

        public Task<Capabilities> Capabilities()
        {
            return Task.Run(() =>
            {
                using (var socket = new RequestSocket($"{_address}:{Adapter.Capabilities_Port(_port_range_start) }"))
                {
                    _log.Debug("Capabilities laden...");
                    socket.SendFrameEmpty();
                    var capabilities = Bare.Msg.Capabilities.Decoded(socket.ReceiveFrameBytes());
                    _log.Debug("Capabilities geladen:");
                    foreach (var capability in capabilities.Value) _log.Debug($" - {capability}");
                    return capabilities;
                }
            });
        }

        public Task<Query_Result> HandleQuery(Token token, Query query)
        {
            return Task.Run(() =>
            {
                using (var socket = new RequestSocket($"{_address}:{Adapter.Query_Port(_port_range_start)}"))
                using (var encryption = Encryption.From_encoded_Key(_encryptionKey))
                {
                    _log.Debug($"QUERY: '{query.GetType()}'");
                    socket.SendFrame(new Query_Message(token.Value, encryption.Encrypt(Encoding.Query_Encoded(query))).Encoded());
                    var message = Encoding.Response_Message_Decoded(socket.ReceiveFrameBytes());

                    switch (message)
                    {
                        case Query_Failed failed:
                            return new IO_Fehler(failed.Reason);

                        case Query_Succeeded succeeded:
                        {
                            try
                            {
                                var response = Encoding.Query_Result_Decoded(encryption.Decrypt(succeeded.Value));
                                _log.Debug($"RESPONSE: '{response.GetType()}'");
                                return response;
                            }
                            catch (CryptographicException e)
                            {
                                _log.Info($"Unbekannte Verschlüsselung! Bitte gleichen Sie den benutzten Schlüssel mit P-PLUS ab: {e.Message}");
                                return new IO_Fehler("Unbekannte Verschlüsselung! Bitte gleichen Sie den benutzten Schlüssel mit P-PLUS ab");
                            }
                        }

                        default:
                            return new IO_Fehler($"'{message.GetType()}' wurde in 'HandleQuery' im Port nicht behandelt");
                    }
                }
            });
        }

        public Task<Command_Result> HandleCommand(Token token, Command command)
        {
            return Task.Run(() =>
            {
                using (var socket = new RequestSocket($"{_address}:{Adapter.Command_Port(_port_range_start)}"))
                using (var encryption = Encryption.From_encoded_Key(_encryptionKey))
                {
                    _log.Debug($"COMMAND: '{command.GetType()}'");
                    socket.SendFrame(new Command_Message(token.Value, encryption.Encrypt(Encoding.Command_Encoded(command))).Encoded());
                    var message = Encoding.Command_Response_Message_Decoded(socket.ReceiveFrameBytes());

                    switch (message)
                    {
                        case Command_Failed failed:
                            return new IO_Fehler(failed.Reason);

                        case Command_Succeeded succeeded:
                        {
                            try
                            {
                                var response = Encoding.Command_Result_Decoded(encryption.Decrypt(succeeded.Value));
                                _log.Debug($"RESPONSE: '{response.GetType()}'");
                                return response;
                            }
                            catch (CryptographicException e)
                            {
                                _log.Info($"Unbekannte Verschlüsselung! Bitte gleichen Sie den benutzten Schlüssel mit P-PLUS ab: {e.Message}");
                                return new IO_Fehler("Unbekannte Verschlüsselung! Bitte gleichen Sie den benutzten Schlüssel mit P-PLUS ab");
                            }
                        }

                        default:
                            return new IO_Fehler($"'{message.GetType()}' wurde in 'HandleCommand' im Port nicht behandelt");
                    }
                }
            });
        }
    }
}