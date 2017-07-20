using System;
using DM7_PPLUS_Integration.Implementierung.Protokoll;
using DM7_PPLUS_Integration.Implementierung.Testing;

namespace DM7_PPLUS_Integration.Implementierung.Server
{
    public class DM7_PPLUS_Host : IDisposable
    {
        private readonly Action<Exception> _onError;

        public DM7_PPLUS_API Ebene_1_API_Level_1 { get; }
        public Ebene_2_Protokoll__Verbindungsaufbau Ebene_2_Service { get; }
        public Ebene_2_Protokoll__API_Level_unabhaengige_Uebertragung Ebene_2_Data { get; }
        public Ebene_3_Protokoll__Service Ebene_3_Service { get; }
        public Ebene_3_Protokoll__Data Ebene_3_Data { get; }
        public NetMQ_Server NetMQServer { get; }


        public DM7_PPLUS_Host(PPLUS_Backend backend, Action<Exception> onError, Log log, string hostaddress, int port)
        {
            _onError = onError;

            var session = Guid.NewGuid();

            Ebene_1_API_Level_1 = new API_Level_1_Adapter(backend, _onError, session, log);

            var router = new API_Router(log, session, backend.AuswahllistenVersion, null, Ebene_1_API_Level_1);
            Ebene_2_Service = router;
            Ebene_2_Data = router;

            Ebene_3_Service = new Service_Adapter(Ebene_2_Service);
            Ebene_3_Data = new Data_Adapter(Ebene_2_Data);

            if (hostaddress.StartsWith("tcp://"))
            {
                NetMQServer = new NetMQ_Server(Ebene_3_Service, Ebene_3_Data, $"{hostaddress}:{port}", log);
                Url = $"{hostaddress}:{port}";
            }
            else
            {
                Url = "demo://Internes Loopback";
            }
        }

        public static DM7_PPLUS_Host Starten(PPLUS_Backend backend, string hostname, int port, Log log, Action<Exception> onError)
        {
            return new DM7_PPLUS_Host(backend, onError, log, hostname, port);
        }

        public static DM7_PPLUS_Host Starten(PPLUS_Backend backend, Log log, Action<Exception> onError)
        {
            return new DM7_PPLUS_Host(backend, onError, log, "", 0);
        }

        public static DM7_PPLUS_Host Starten(Log log, Action<Exception> onError)
        {
            return new DM7_PPLUS_Host(new Demo_Datenserver(), onError, log, "", 0);
        }

        public string Url { get; }

        public void Dispose()
        {
            try
            {
                NetMQServer?.Dispose();
            }
            catch (Exception ex)
            {
                if (_onError != null)
                {
                    _onError(ex);
                }
                else
                {
                    throw;
                }
            }
        }
    }
}