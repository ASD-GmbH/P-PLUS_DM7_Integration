using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DM7_PPLUS_Integration.Implementierung.Protokoll;
using DM7_PPLUS_Integration.Implementierung.Shared;

namespace DM7_PPLUS_Integration.Implementierung.Server
{
    internal class Service_Adapter : DisposeGroupMember, Ebene_3_Protokoll__Service
    {
        private readonly Ebene_2_Protokoll__Verbindungsaufbau _backend;

        public Service_Adapter(Ebene_2_Protokoll__Verbindungsaufbau backend, DisposeGroup disposegroup) : base(disposegroup)
        {
            _backend = backend;
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
            var credentials = System.Text.Encoding.UTF8.GetString(request, offset + 4 + 4 + 4, loginlength);
            return _backend.Connect_Ebene_1(credentials, maxApiLevel, minApiLevel).ContinueWith(task => EncodeConnectionResult(task.Result));
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
                    BitConverter.GetBytes(success.Auswahllistenversion)
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