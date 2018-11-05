using System;
using System.Threading;
using System.Threading.Tasks;
using DM7_PPLUS_Integration.Implementierung.Protokoll;
using DM7_PPLUS_Integration.Implementierung.Server;
using DM7_PPLUS_Integration.Implementierung.Shared;

namespace DM7_PPLUS_Integration.Implementierung.Client
{
    public class LoopbackFactory : Ebene_2_Proxy_Factory
    {
        private readonly DM7_PPLUS_Host _host;
        private readonly int _ebene;
        private readonly DisposeGroup _disposegroup;

        public LoopbackFactory(DM7_PPLUS_Host host, int ebene)
        {
            _host = host;
            _ebene = ebene;
            _disposegroup = new DisposeGroup();
        }

        internal LoopbackFactory(DM7_PPLUS_Host host, int ebene, DisposeGroup disposegroup)
        {
            _host = host;
            _ebene = ebene;
            _disposegroup = disposegroup;
        }

        public Task<Tuple<Ebene_2_Protokoll__Verbindungsaufbau, Ebene_2_Protokoll__API_Level_unabhaengige_Uebertragung>> Connect_Ebene_2(string networkAddress, string credentials, Log log, CancellationToken cancellationToken_Verbindung)
        {
            var task =
                new Task<Tuple<Ebene_2_Protokoll__Verbindungsaufbau,
                    Ebene_2_Protokoll__API_Level_unabhaengige_Uebertragung>>(() =>
                    {

                        Ebene_2_Protokoll__Verbindungsaufbau service;
                        Ebene_2_Protokoll__API_Level_unabhaengige_Uebertragung data;

                        if (_ebene == 2)
                        {
                            service = _host.Ebene_2_Service;
                            data = _host.Ebene_2_Data;
                        }
                        else
                        {
                            service = new Service_Proxy(_host.Ebene_3_Service, _disposegroup);
                            data = new Data_Proxy(_host.Ebene_3_Data, _disposegroup);
                        }

                        return new Tuple<Ebene_2_Protokoll__Verbindungsaufbau, Ebene_2_Protokoll__API_Level_unabhaengige_Uebertragung>(service, data);
                    });

            task.RunSynchronously();
            return task;
        }
    }
}