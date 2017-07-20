using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DM7_PPLUS_Integration.Implementierung.Protokoll;
using DM7_PPLUS_Integration.Implementierung.Server;
using DM7_PPLUS_Integration.Implementierung.Shared;

namespace DM7_PPLUS_Integration.Implementierung.Client
{
    public class ServiceClient : Ebene_2_Protokoll__Verbindungsaufbau
    {
        private readonly Ebene_3_Protokoll__Netzwerkuebertragung_Service _client;

        public ServiceClient(Ebene_3_Protokoll__Netzwerkuebertragung_Service client)
        {
            _client = client;
        }

        public void Dispose()
        {
            _client.Dispose();
        }

        public Task<ConnectionResult> Connect(string login, int maxApiLevel, int minApiLevel)
        {
            var loginbuffer = System.Text.Encoding.UTF8.GetBytes(login);
            return
                _client.ServiceRequest(new List<byte[]>
                {
                    new byte[] {1, 1},
                    BitConverter.GetBytes(maxApiLevel),
                    BitConverter.GetBytes(minApiLevel),
                    BitConverter.GetBytes(loginbuffer.Length),
                    loginbuffer
                }.Concat()).ContinueWith(task => Deserialize(task.Result));
        }
        private ConnectionResult Deserialize(byte[] response)
        {
            if (response[0] == Constants.CONNECTION_RESPONSE_OK)
            {
                var apilevel = BitConverter.ToInt32(response, 1);
                var auswahllistenversion = BitConverter.ToInt32(response, 1 + 4);
                var guidbuffer = new byte[16];
                Array.Copy(response, 1 + 4 + 4, guidbuffer, 0, 16);
                var session = new Guid(guidbuffer);
                return new ConnectionSucceeded(apilevel, auswahllistenversion, session);
            }
            if (response[0] == Constants.CONNECTION_RESPONSE_FAILURE)
            {
                var reason = (ConnectionFailure)BitConverter.ToInt32(response, 1);
                var infolength = BitConverter.ToInt32(response, 1 + 4);
                var info = System.Text.Encoding.UTF8.GetString(response, 1 + 4 + 4, infolength);
                return new ConnectionFailed(reason, info);
            }
            return new ConnectionFailed(ConnectionFailure.Unsupported_Connection_Protocol, $"Unknown response code ({response[0]})");
        }
    }
}