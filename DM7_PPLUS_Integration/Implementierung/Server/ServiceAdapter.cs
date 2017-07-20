using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DM7_PPLUS_Integration.Implementierung.Protokoll;
using DM7_PPLUS_Integration.Implementierung.Shared;

namespace DM7_PPLUS_Integration.Implementierung.Server
{
    public class ServiceAdapter : Ebene_3_Protokoll__Netzwerkuebertragung_Service
    {
        private readonly Ebene_2_Protokoll__Verbindungsaufbau _backend;

        public ServiceAdapter(Ebene_2_Protokoll__Verbindungsaufbau backend)
        {
            _backend = backend;
        }

        public void Dispose()
        {
        }

        public Task<byte[]> ServiceRequest(byte[] request)
        {
            var protocol = (int)request[0];
            var message = (int)request[1];
            if (protocol == Constants.SERVICE_PROTOCOL_1)
            {
                if (message == Constants.SERVICE_CONNECT)
                {
                    return Connect(request, 2);
                }
            }
            var task = new Task<byte[]>(() => new byte[]
                { Constants.CONNECTION_RESPONSE_FAILURE,  });
            task.RunSynchronously();
            return task;
        }

        private Task<byte[]> Connect(byte[] request, int offset)
        {
            var maxApiLevel = BitConverter.ToInt32(request, offset);
            var minApiLevel = BitConverter.ToInt32(request, offset + 4);
            var loginlength = BitConverter.ToInt32(request, offset + 4 + 4);
            var login = System.Text.Encoding.UTF8.GetString(request, offset + 4 + 4 + 4, loginlength);
            return _backend.Connect(login, maxApiLevel, minApiLevel).ContinueWith(task => EncodeConnectionResult(task.Result));
        }

        private byte[] EncodeConnectionResult(ConnectionResult result)
        {
            if (result is ConnectionSucceeded)
            {
                var success = (ConnectionSucceeded) result;
                return new List<byte[]>
                {
                    new byte[] {Constants.CONNECTION_RESPONSE_OK},
                    BitConverter.GetBytes(success.Api_Level),
                    BitConverter.GetBytes(success.Auswahllistenversion),
                    success.Session.ToByteArray()
                }.Concat();
            }
            else
            {
                var failure = (ConnectionFailed) result;
                var info = System.Text.Encoding.UTF8.GetBytes(failure.Info);
                return new List<byte[]>
                {
                    new byte[] {Constants.CONNECTION_RESPONSE_FAILURE},
                    BitConverter.GetBytes((int) failure.Reason),
                    BitConverter.GetBytes(info.Length),
                    info
                }.Concat();
            }
        }
    }
}