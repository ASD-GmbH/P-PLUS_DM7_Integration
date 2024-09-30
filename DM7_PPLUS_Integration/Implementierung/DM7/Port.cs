using System;
using System.Security.Cryptography;
using System.Threading.Tasks;
using DM7_PPLUS_Integration.Messages.DM7;
using NetMQ;
using NetMQ.Sockets;

namespace DM7_PPLUS_Integration.Implementierung.DM7
{
    public class Port
    {
        private readonly string _address;
        private readonly int _port_range_start;
        private readonly string _encryption_key;
        private readonly Log _log;
        private readonly TimeSpan? _timeout;

        public Port(string address, int portRangeStart, string encryptionKey, Log log, TimeSpan? timeout)
        {
            _address = address;
            _port_range_start = portRangeStart;
            _encryption_key = encryptionKey;
            _log = log;
            _timeout = timeout;
        }

        public Task<Capabilities> Capabilities()
        {
            return Task.Run(() =>
            {
                using (var socket = new RequestSocket($"{_address}:{Adapter.Capabilities_Port(_port_range_start)}"))
                {
                    _log.Debug("Capabilities laden...");
                    socket.SendFrameEmpty();
                    var capabilities = Messages.DM7.Capabilities.Decoded(ReceiveBytes(socket, _timeout));
                    _log.Debug("Capabilities geladen:");
                    foreach (var capability in capabilities.Value) _log.Debug($" - {capability}");
                    return capabilities;
                }
            });
        }

        public Task<Query_result> Handle_Query(Query query)
        {
            return Task.Run(() =>
            {
                using (var socket = new RequestSocket($"{_address}:{Adapter.Query_Port(_port_range_start)}"))
                using (var encryption = Encryption.From_encoded_Key(_encryption_key))
                {
                    _log.Debug($"QUERY: '{query.GetType()}'");
                    socket.SendFrame(new Query_message(encryption.Encrypt(Encoding.Query_Encoded(query))).Encoded());
                    var response = Encoding.Response_message_Decoded(ReceiveBytes(socket, _timeout));

                    switch (response)
                    {
                        case Query_succeeded succeeded:
                        {
                            try
                            {
                                var result = Encoding.Query_result_Decoded(encryption.Decrypt(succeeded.Value));
                                _log.Debug($"RESPONSE: '{result.GetType()}'");
                                return result;
                            }
                            catch (CryptographicException e)
                            {
                                _log.Info($"Unbekannte Verschlüsselung! Bitte gleichen Sie den benutzten Schlüssel mit DM7 ab: {e.Message}");
                                return new IO_fehler("Unbekannte Verschlüsselung! Bitte gleichen Sie den benutzten Schlüssel mit DM7 ab");
                            }
                        }

                        case Query_failed failed:
                            return new IO_fehler(failed.Reason);

                        default:
                            return new IO_fehler($"'{response.GetType()}' wurde in 'HandleQuery' im Port nicht behandelt");
                    }
                }
            });
        }

        private static byte[] ReceiveBytes(NetMQSocket socket, TimeSpan? timeout)
        {
            if (!timeout.HasValue) return socket.ReceiveFrameBytes();
            if (socket.TryReceiveFrameBytes(timeout.Value, out var bytes)) return bytes;
            throw new TimeoutException();
        }
    }
}