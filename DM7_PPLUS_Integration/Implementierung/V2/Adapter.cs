using System;
using System.Threading;
using Bare.Msg;
using NetMQ;
using NetMQ.Sockets;

namespace DM7_PPLUS_Integration.Implementierung.V2
{
    public class Adapter : IDisposable
    {
        public static int Query_Port(int portRangeStart) => portRangeStart;
        public static int Authentication_Port(int portRangeStart) => portRangeStart + 1;

        private readonly PPLUS_Handler _pplusHandler;
        private readonly NetMQPoller _poller;
        private readonly Thread _thread;

        public Adapter(string address, int port_range_start, PPLUS_Handler pplusHandler)
        {
            _pplusHandler = pplusHandler;

            _poller = new NetMQPoller();
            _thread = new Thread(() =>
            {
                using (var querySocket = new RouterSocket())
                using (var authenticationSocket = new RouterSocket())
                {
                    querySocket.Bind($"tcp://{address}:{Query_Port(port_range_start)}");
                    querySocket.ReceiveReady += (_, args) => Handle_Query(args.Socket);

                    authenticationSocket.Bind($"tcp://{address}:{Authentication_Port(port_range_start)}");
                    authenticationSocket.ReceiveReady += (_, args) => Authenticate(args.Socket);

                    using (_poller)
                    {
                        _poller.Add(querySocket);
                        _poller.Add(authenticationSocket);
                        _poller.Run();
                    }
                }
            });
            _thread.Start();
        }

        private void Handle_Query(NetMQSocket socket)
        {
            var caller = socket.ReceiveFrameBytes();
            socket.SkipFrame();
            Send_Message(
                caller,
                _pplusHandler.HandleQuery(Query_Message.Decoded(socket.ReceiveFrameBytes())).Result,
                socket);
        }

        private static void Send_Message(byte[] caller, Response response, NetMQSocket socket)
        {
            socket.SendMoreFrame(caller);
            socket.SendMoreFrameEmpty();
            socket.SendFrame(Encoding.Response_Encoded(response));
        }

        private void Authenticate(NetMQSocket socket)
        {
            var caller = socket.ReceiveFrameBytes();
            socket.SkipFrame();
            var request = Authentication_Request.Decoded(socket.ReceiveFrameBytes());
            var token = _pplusHandler.Authenticate(request.User, request.Password).Result;
            socket.SendMoreFrame(caller);
            socket.SendMoreFrameEmpty();
            socket.SendFrame(new Bare.Msg.Authentication_Result(token?.Value).Encoded());
        }

        public void Dispose()
        {
            _poller.Dispose();
            var gracefullyStopped = _thread.Join(TimeSpan.FromSeconds(10));
            if (!gracefullyStopped) _thread.Abort();
        }
    }
}