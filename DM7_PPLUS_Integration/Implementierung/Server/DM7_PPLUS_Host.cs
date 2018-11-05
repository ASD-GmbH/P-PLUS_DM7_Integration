﻿using System;
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
        public Level_0_Test_API Ebene_1_API_Level_0 { get; }
        public DM7_PPLUS_API Ebene_1_API_Level_1 { get; }
        public DM7_PPLUS_API Ebene_1_API_Level_3 { get; }

        public Ebene_2_Protokoll__Verbindungsaufbau Ebene_2_Service { get; }
        public Ebene_2_Protokoll__API_Level_unabhaengige_Uebertragung Ebene_2_Data { get; }
        public Ebene_3_Protokoll__Service Ebene_3_Service { get; }
        public Ebene_3_Protokoll__Data Ebene_3_Data { get; }

        private readonly DisposeGroup _disposegroup;

        private static readonly List<int> API_LEVELS = new List<int> {1,3};

        private DM7_PPLUS_Host(PPLUS_Backend backend, PPLUS_Authentifizierung authentifizierung, Action<Exception> onError, Log log, string hostaddress, int port, string privateKey, List<int> apiLevels)
        {
            log.Info("DM7/P-PLUS Integrationsschnittstelle - " + Version.VersionString);

            _disposegroup = new DisposeGroup();

            if (apiLevels.Contains(0)) Ebene_1_API_Level_0 = new TestBackend_Level_0();
            if (apiLevels.Contains(1)) Ebene_1_API_Level_1 = new API_Level_1_Adapter(backend, onError, log, _disposegroup);
            if (apiLevels.Contains(3)) Ebene_1_API_Level_3 = new API_Level_3_Adapter(backend, onError, log, _disposegroup);

            var router = new API_Router(log, backend.AuswahllistenVersion, authentifizierung, Ebene_1_API_Level_0, Ebene_1_API_Level_1, Ebene_1_API_Level_3, _disposegroup);
            Ebene_2_Service = router;
            Ebene_2_Data = router;

            Ebene_3_Service = new Service_Adapter(Ebene_2_Service, _disposegroup);
            Ebene_3_Data = new Data_Adapter(Ebene_2_Data, log, _disposegroup);

            if (hostaddress.StartsWith("tcp://"))
            {
                NetMQ_Server.Start(Ebene_3_Service, Ebene_3_Data, $"{hostaddress}:{port}", privateKey, log, _disposegroup);
            }

            new Thread(() =>
            {
                Thread.Sleep(500);
                (Ebene_1_API_Level_1 as API_Level_3_Adapter)?.Announce();
            }).Start();
        }

        public static DM7_PPLUS_Host Starten(PPLUS_Backend backend, PPLUS_Authentifizierung authentifizierung, string hostname, int port, string privateKey, Log log, Action<Exception> onError)
        {
            return new DM7_PPLUS_Host(backend, authentifizierung, onError, log, hostname, port, privateKey, API_LEVELS);
        }

        public static DM7_PPLUS_Host Starten(PPLUS_Backend backend, PPLUS_Authentifizierung authentifizierung, Log log, Action<Exception> onError, IEnumerable<int> levels = null)
        {
            return new DM7_PPLUS_Host(backend, authentifizierung, onError, log, "", 0, "", (levels ?? API_LEVELS).ToList());
        }

        public void Dispose()
        {
            _disposegroup.Dispose();
        }
    }
}