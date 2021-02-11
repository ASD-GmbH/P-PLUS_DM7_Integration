using System;
using System.Threading;
using Bare.Msg;
using NetMQ;
using NetMQ.Sockets;

namespace DM7_PPLUS_Integration.Implementierung.V2
{
    public class Adapter : IDisposable
    {
        private readonly Backend _backend;
        private readonly NetMQPoller _poller;
        private readonly Thread _thread;

        public Adapter(string address, int port_range_start, Backend backend)
        {
            _backend = backend;

            _poller = new NetMQPoller();
            _thread = new Thread(() =>
            {
                using (var socket = new RouterSocket())
                {
                    socket.Bind($"tcp://{address}:{port_range_start}");
                    socket.ReceiveReady += (_, args) => Handle_Query(args.Socket);

                    using (_poller)
                    {
                        _poller.Add(socket);
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
                _backend.HandleQuery(Encoding.Query_Decoded(socket.ReceiveFrameBytes())).Result,
                socket);
        }

        private static void Send_Message(byte[] caller, Response response, NetMQSocket socket)
        {
            socket.SendFrame(caller, true);
            socket.SendFrameEmpty(true);
            socket.SendFrame(Encoding.Response_Encoded(response));
        }

        public void Dispose()
        {
            _poller.Dispose();
            var gracefullyStopped = _thread.Join(TimeSpan.FromSeconds(10));
            if (!gracefullyStopped) _thread.Abort();
        }
    }
}