using System;
using System.Security.Cryptography;
using System.Threading;
using DM7_PPLUS_Integration.Messages;
using NetMQ;
using NetMQ.Sockets;

namespace DM7_PPLUS_Integration.Implementierung
{
    public class Adapter : IDisposable
    {
        public static int Authentication_Port(int portRangeStart) => portRangeStart;
        public static int Capabilities_Port(int portRangeStart) => portRangeStart + 1;
        public static int Query_Port(int portRangeStart) => portRangeStart + 2;
        public static int Command_Port(int portRangeStart) => portRangeStart + 3;
        public static int Notification_Port(int portRangeStart) => portRangeStart + 4;
        public const string Mitarbeiter_Topic = "Mitarbeiter";
        public const string Dienste_Topic = "Dienste";

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
                using (var authentication_socket = new RouterSocket())
                using (var capability_socket = new RouterSocket())
                using (var query_socket = new RouterSocket())
                using (var command_socket = new RouterSocket())
                using (var notification_socket = new PublisherSocket())
                {
                    authentication_socket.Bind($"tcp://{address}:{Authentication_Port(port_range_start)}");
                    authentication_socket.ReceiveReady += (_, args) => Authenticate(args.Socket);

                    capability_socket.Bind($"tcp://{address}:{Capabilities_Port(port_range_start)}");
                    capability_socket.ReceiveReady += (_, args) => Handle_Capabilities(args.Socket);

                    query_socket.Bind($"tcp://{address}:{Query_Port(port_range_start)}");
                    query_socket.ReceiveReady += (_, args) => Handle_Query(args.Socket);

                    command_socket.Bind($"tcp://{address}:{Command_Port(port_range_start)}");
                    command_socket.ReceiveReady += (_, args) => Handle_Command(args.Socket);

                    notification_socket.Bind($"tcp://{address}:{Notification_Port(port_range_start)}");
                    pplusHandler.Mitarbeiteränderungen_liegen_bereit += () => notification_socket.SendMoreFrame(Mitarbeiter_Topic).SendFrameEmpty();
                    pplusHandler.Dienständerungen_liegen_bereit += () => notification_socket.SendMoreFrame(Dienste_Topic).SendFrameEmpty();

                    using (_poller)
                    {
                        _poller.Add(authentication_socket);
                        _poller.Add(capability_socket);
                        _poller.Add(query_socket);
                        _poller.Add(command_socket);
                        _poller.Add(notification_socket);
                        _poller.Run();
                    }
                }
            });
            _thread.Start();
        }

        private void Authenticate(NetMQSocket socket)
        {
            var caller = socket.ReceiveFrameBytes();
            socket.SkipFrame();

            using (var encryption = Encryption.From_encoded_Key(_encryptionKey))
            {
                AuthenticationResult message;
                try
                {
                    var request = AuthenticationRequest.Decoded(encryption.Decrypt(socket.ReceiveFrameBytes()));
                    var token = _pplusHandler.Authenticate(request.User, request.Password).Result;
                    message =
                        token.HasValue
                            ? (AuthenticationResult) new AuthenticationSucceeded(token.Value.Value)
                            : new AuthenticationFailed("Zugriff nicht zugelassen.");
                }
                catch (CryptographicException)
                {
                    message = new AuthenticationFailed("Unbekannte Verschlüsselung! Bitte gleichen Sie den benutzten Schlüssel mit P-PLUS ab.");
                }

                socket.SendMoreFrame(caller);
                socket.SendMoreFrameEmpty();
                socket.SendFrame(Encoding.AuthenticationResult_Encoded(message));
            }
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
                var message = QueryMessage.Decoded(socket.ReceiveFrameBytes());

                ResponseMessage response;
                try
                {
                    var query = Encoding.Query_Decoded(encryption.Decrypt(message.Query));
                    var result = _pplusHandler.HandleQuery(Message_mapper.Token_aus(message), query).Result;
                    response = new QuerySucceeded(encryption.Encrypt(Encoding.QueryResult_Encoded(result)));
                }
                catch (CryptographicException)
                {
                    response = new QueryFailed("Unbekannte Verschlüsselung! Bitte gleichen Sie den benutzten Schlüssel mit P-PLUS ab.");
                }

                socket.SendMoreFrame(caller);
                socket.SendMoreFrameEmpty();
                socket.SendFrame(Encoding.ResponseMessage_Encoded(response));
            }
        }

        private void Handle_Command(NetMQSocket socket)
        {
            using (var encryption = Encryption.From_encoded_Key(_encryptionKey))
            {
                var caller = socket.ReceiveFrameBytes();
                socket.SkipFrame();
                var message = CommandMessage.Decoded(socket.ReceiveFrameBytes());

                CommandResponseMessage response;
                try
                {
                    var command = Encoding.Command_Decoded(encryption.Decrypt(message.Command));
                    var result = _pplusHandler.HandleCommand(Message_mapper.Token_aus(message), command).Result;
                    response = new CommandSucceeded(encryption.Encrypt(Encoding.CommandResult_Encoded(result)));
                }
                catch (CryptographicException)
                {
                    response = new CommandFailed("Unbekannte Verschlüsselung! Bitte gleichen Sie den benutzten Schlüssel mit P-PLUS ab.");
                }

                socket.SendMoreFrame(caller);
                socket.SendMoreFrameEmpty();
                socket.SendFrame(Encoding.CommandResponseMessage_Encoded(response));
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