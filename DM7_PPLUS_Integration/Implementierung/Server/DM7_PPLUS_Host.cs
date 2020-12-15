using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using DM7_PPLUS_Integration.Implementierung.Protokoll;
using DM7_PPLUS_Integration.Implementierung.Shared;
using DM7_PPLUS_Integration.Implementierung.Testing;

namespace DM7_PPLUS_Integration.Implementierung.Server
{
    public class DM7_PPLUS_Host : IDisposable
    {
        public Version_0_Test_API Schicht_1_API_Version_0 { get; }
        public DM7_PPLUS_API Schicht_1_API_Version_1 { get; }
        public DM7_PPLUS_API Schicht_1_API_Version_3 { get; }

        public Schicht_2_Protokoll__Verbindungsaufbau Schicht_2_Service { get; }
        public Schicht_2_Protokoll__API_Version_unabhaengige_Uebertragung Schicht_2_Data { get; }
        public Schicht_3_Protokoll__Service Schicht_3_Service { get; }
        public Schicht_3_Protokoll__Data Schicht_3_Data { get; }

        private readonly DisposeGroup _disposegroup;

        private static readonly List<int> API_VERSIONS = new List<int> {1,3};

        private DM7_PPLUS_Host(PPLUS_Backend backend, PPLUS_Authentifizierung authentifizierung, Action<Exception> onError, Log log, string hostaddress, int port, string privateKey, List<int> apiVersions)
        {
            log.Info("DM7/P-PLUS Integrationsschnittstelle - " + Version.VersionString);

            _disposegroup = new DisposeGroup();

            if (apiVersions.Contains(0)) Schicht_1_API_Version_0 = new TestBackend_Version_0();
            if (apiVersions.Contains(1)) Schicht_1_API_Version_1 = new API_Version_1_Adapter(backend, onError, log, _disposegroup);
            if (apiVersions.Contains(3)) Schicht_1_API_Version_3 = new API_Version_3_Adapter(backend, onError, log, _disposegroup);

            var router = new API_Router(log, backend.AuswahllistenVersion, authentifizierung, Schicht_1_API_Version_0, Schicht_1_API_Version_1, Schicht_1_API_Version_3, _disposegroup);
            Schicht_2_Service = router;
            Schicht_2_Data = router;

            Schicht_3_Service = new Service_Adapter(Schicht_2_Service, _disposegroup);
            Schicht_3_Data = new Data_Adapter(Schicht_2_Data, log, _disposegroup);

            if (hostaddress.StartsWith("tcp://"))
            {
                NetMQ_Server.Start(Schicht_3_Service, Schicht_3_Data, $"{hostaddress}:{port}", privateKey, log, _disposegroup);
            }

            new Thread(() =>
            {
                Thread.Sleep(500);
                (Schicht_1_API_Version_1 as API_Version_3_Adapter)?.Announce();
            }).Start();
        }

        public static DM7_PPLUS_Host Starten(PPLUS_Backend backend, PPLUS_Authentifizierung authentifizierung, string hostname, int port, string privateKey, Log log, Action<Exception> onError)
        {
            return new DM7_PPLUS_Host(backend, authentifizierung, onError, log, hostname, port, privateKey, API_VERSIONS);
        }

        public static DM7_PPLUS_Host Starten(PPLUS_Backend backend, PPLUS_Authentifizierung authentifizierung, Log log, Action<Exception> onError, IEnumerable<int> versions = null)
        {
            return new DM7_PPLUS_Host(backend, authentifizierung, onError, log, "", 0, "", (versions ?? API_VERSIONS).ToList());
        }

        public void Dispose()
        {
            _disposegroup.Dispose();
        }
    }
}