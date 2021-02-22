using System;
using System.Security.Cryptography;
using System.Threading;
using Bare.Msg;
using NetMQ;
using NetMQ.Sockets;

namespace DM7_PPLUS_Integration.Implementierung
{
    public class Adapter : IDisposable
    {
        public static int Query_Port(int portRangeStart) => portRangeStart;
        public static int Authentication_Port(int portRangeStart) => portRangeStart + 1;
        public static int Capabilities_Port(int portRangeStart) => portRangeStart + 2;

        private readonly PPLUS_Handler _pplusHandler;
        private readonly string _encryptionKey;
        private readonly NetMQPoller _poller;
        private readonly Thread _thread;

        public Adapter(string address, int port_range_start, PPLUS_Handler pplusHandler, string encryptionKey)
        {
            _pplusHandler = pplusHandler;
            _encryptionKey = encryptionKey;

            _poller = new NetMQPoller();
            _thread = new Thread(() =>
            {
                using (var querySocket = new RouterSocket())
                using (var capability_socket = new RouterSocket())
                using (var authenticationSocket = new RouterSocket())
                {
                    querySocket.Bind($"tcp://{address}:{Query_Port(port_range_start)}");
                    querySocket.ReceiveReady += (_, args) => Handle_Query(args.Socket);

                    capability_socket.Bind($"tcp://{address}:{Capabilities_Port(port_range_start)}");
                    capability_socket.ReceiveReady += (_, args) => Handle_Capabilities(args.Socket);

                    authenticationSocket.Bind($"tcp://{address}:{Authentication_Port(port_range_start)}");
                    authenticationSocket.ReceiveReady += (_, args) => Authenticate(args.Socket);

                    using (_poller)
                    {
                        _poller.Add(querySocket);
                        _poller.Add(capability_socket);
                        _poller.Add(authenticationSocket);
                        _poller.Run();
                    }
                }
            });
            _thread.Start();
        }

        private void Handle_Capabilities(NetMQSocket socket)
        {
            var caller = socket.ReceiveFrameBytes();
            socket.SkipFrame();
            socket.SkipFrame();

            var capabilities = _pplusHandler.Capabilities().Result;

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
                var message = Query_Message.Decoded(socket.ReceiveFrameBytes());

                Response_Message response;
                try
                {
                    var query = Encoding.Query_Decoded(encryption.Decrypt(message.Query));
                    var result = _pplusHandler.HandleQuery(Message_mapper.Token_aus(message), query).Result;
                    response = new Query_Succeeded(encryption.Encrypt(Encoding.Query_Result_Encoded(result)));
                }
                catch (CryptographicException)
                {
                    response = new Query_Failed("Unbekannte Verschlüsselung! Bitte gleichen Sie den benutzten Schlüssel mit P-PLUS ab.");
                }

                Send_Response(caller, response, socket);
            }
        }

        private static void Send_Response(byte[] caller, Response_Message response, NetMQSocket socket)
        {
            socket.SendMoreFrame(caller);
            socket.SendMoreFrameEmpty();
            socket.SendFrame(Encoding.Response_Message_Encoded(response));
        }

        private void Authenticate(NetMQSocket socket)
        {
            var caller = socket.ReceiveFrameBytes();
            socket.SkipFrame();

            using (var encryption = Encryption.From_encoded_Key(_encryptionKey))
            {
                Bare.Msg.Authentication_Result message;
                try
                {
                    var request = Authentication_Request.Decoded(encryption.Decrypt(socket.ReceiveFrameBytes()));
                    var token = _pplusHandler.Authenticate(request.User, request.Password).Result;
                    message =
                        token.HasValue
                            ? (Bare.Msg.Authentication_Result) new Authentication_Succeeded(token.Value.Value)
                            : new Authentication_Failed("Zugriff nicht zugelassen.");
                }
                catch (CryptographicException)
                {
                    message = new Authentication_Failed("Unbekannte Verschlüsselung! Bitte gleichen Sie den benutzten Schlüssel mit P-PLUS ab.");
                }

                socket.SendMoreFrame(caller);
                socket.SendMoreFrameEmpty();
                socket.SendFrame(Encoding.Authentication_Result_Encoded(message));
            }
        }

        public void Dispose()
        {
            _poller.Dispose();
            var gracefullyStopped = _thread.Join(TimeSpan.FromSeconds(10));
            if (!gracefullyStopped) _thread.Abort();
        }
    }
}