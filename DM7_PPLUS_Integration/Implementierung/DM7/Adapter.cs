using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using DM7_PPLUS_Integration.Messages.DM7;
using NetMQ;
using NetMQ.Sockets;

namespace DM7_PPLUS_Integration.Implementierung.DM7
{
    public class Adapter : IDisposable
    {
        public static int Capabilities_Port(int port_range_start) => port_range_start;
        public static int Query_Port(int port_range_start) => port_range_start + 1;

        private readonly DM7_Stammdaten _stammdaten;
        private readonly string _encryptionKey;
        private readonly Thread _thread;
        private readonly NetMQPoller _poller;

        public Adapter(string address, int port_range_start, DM7_Stammdaten stammdaten, string encryptionKey)
        {
            _stammdaten = stammdaten;
            _encryptionKey = encryptionKey;

            _poller = new NetMQPoller();
            _thread = new Thread(() =>
            {
                using (var capability_socket = new RouterSocket())
                using (var query_socket = new RouterSocket())
                {
                    capability_socket.Bind($"tcp://{address}:{Capabilities_Port(port_range_start)}");
                    capability_socket.ReceiveReady += (_, args) => Handle_Capabilities(args.Socket);

                    query_socket.Bind($"tcp://{address}:{Query_Port(port_range_start)}");
                    query_socket.ReceiveReady += (_, args) => Handle_Query(args.Socket);

                    using (_poller)
                    {
                        _poller.Add(capability_socket);
                        _poller.Add(query_socket);
                        _poller.Run();
                    }
                }
            });
            _thread.Start();
        }

        private static void Handle_Capabilities(NetMQSocket socket)
        {
            var caller = socket.ReceiveFrameBytes();
            socket.SkipFrame();
            socket.SkipFrame();

            var capabilities = new Capabilities(new List<Capability>
            {
                Capability.ALLE_LEISTUNGEN_V1,
                Capability.ALLE_MANDANTEN_V1
            });

            socket.SendMoreFrame(caller);
            socket.SendMoreFrameEmpty();
            socket.SendFrame(capabilities.Encoded());
        }

        private void Handle_Query(NetMQSocket socket)
        {
            using (var encryption = Encryption.From_encoded_Key(_encryptionKey))
            {
                var caller = socket.ReceiveFrameBytes();
                socket.SkipFrame();
                var message = QueryMessage.Decoded(socket.ReceiveFrameBytes());

                ResponseMessage response;
                try
                {
                    var query = Encoding.Query_Decoded(encryption.Decrypt(message.Query));
                    var result = Handle_Query(query).Result;
                    response = new QuerySucceeded(encryption.Encrypt(Encoding.QueryResult_Encoded(result)));
                }
                catch (CryptographicException)
                {
                    response = new QueryFailed("Unbekannte Verschlüsselung! Bitte gleichen Sie den benutzten Schlüssel mit DM7 ab.");
                }

                socket.SendMoreFrame(caller);
                socket.SendMoreFrameEmpty();
                socket.SendFrame(Encoding.ResponseMessage_Encoded(response));
            }
        }

        private Task<QueryResult> Handle_Query(Query query)
        {
            return Task.Run(() =>
            {
                switch (query)
                {
                    case AlleLeistungenV1 _:
                    {
                        try
                        {
                            return Message_mapper.Von_Leistungen(_stammdaten.Alle_Leistungen().Result);
                        }
                        catch (Exception e)
                        {
                            return new IOFehler($"Abfrage der Leistungen führte zum Problem: {e.Message}");
                        }
                    }

                    case AlleMandantenV1 _:
                    {
                        try
                        {
                            return (QueryResult) Message_mapper.Von_Mandanten(_stammdaten.Alle_Mandanten().Result);
                        }
                        catch (Exception e)
                        {
                            return new IOFehler($"Abfrage der Mandanten führte zum Problem: {e.Message}");
                        }
                    }

                    default:
                        return new IOFehler($"Query {query.GetType()} wird nicht behandelt");
                }
            });
        }

        public void Dispose()
        {
            _poller?.Stop();
            var gracefullyStopped = _thread.Join(TimeSpan.FromSeconds(10));
            if (!gracefullyStopped) _thread.Abort();
        }
    }
}