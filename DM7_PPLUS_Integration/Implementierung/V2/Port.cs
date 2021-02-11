using System.Threading.Tasks;
using Bare.Msg;
using NetMQ;
using NetMQ.Sockets;

namespace DM7_PPLUS_Integration.Implementierung.V2
{
    public class Port : Backend
    {
        private readonly string _address;
        private readonly int _port_range_start;

        public Port(string address, int portRangeStart)
        {
            _address = address;
            _port_range_start = portRangeStart;
        }

        public Task<Response> HandleQuery(Query query)
        {
            return Task.Run(() =>
            {
                using (var socket = new RequestSocket($"{_address}:{_port_range_start}"))
                {
                    socket.SendFrame(Encoding.Query_Encoded(query));
                    return Encoding.Response_Decoded(socket.ReceiveFrameBytes());
                }
            });
        }

        public void Dispose()
        {
        }
    }
}