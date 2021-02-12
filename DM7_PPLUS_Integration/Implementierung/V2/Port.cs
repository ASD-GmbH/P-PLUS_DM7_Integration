using System.Threading.Tasks;
using Bare.Msg;
using NetMQ;
using NetMQ.Sockets;

namespace DM7_PPLUS_Integration.Implementierung.V2
{
    public class Port : PPLUS_Handler
    {
        private readonly string _address;
        private readonly int _port_range_start;
        private readonly Log _log;

        public Port(string address, int portRangeStart, Log log)
        {
            _address = address;
            _port_range_start = portRangeStart;
            _log = log;
        }

        public Task<Response> HandleQuery(Query_Message message)
        {
            return Task.Run(() =>
            {
                using (var socket = new RequestSocket($"{_address}:{_port_range_start}"))
                {
                    _log.Debug($"QUERY: '{message.Query.GetType()}'");
                    socket.SendFrame(message.Encoded());
                    var response = Encoding.Response_Decoded(socket.ReceiveFrameBytes());
                    _log.Debug($"RESPONSE: '{response.GetType()}'");
                    return response;
                }
            });
        }

        public Task<Token?> Authenticate(string user, string password)
        {
            return Task.Run<Token?>(() =>
            {
                using (var socket = new RequestSocket($"{_address}:{Adapter.Authentication_Port(_port_range_start) }"))
                {
                    _log.Debug("Authenticate");
                    socket.SendFrame(new Authentication_Request(user, password).Encoded());
                    var result= Bare.Msg.Authentication_Result.Decoded(socket.ReceiveFrameBytes());

                    if (!result.Token.HasValue) return null;
                    _log.Debug("Authenticated");
                    return new Token(result.Token.Value);
                }
            });
        }
    }
}