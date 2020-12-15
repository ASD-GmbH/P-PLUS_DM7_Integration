using System;
using System.Threading;
using System.Threading.Tasks;
using DM7_PPLUS_Integration.Implementierung.Protokoll;
using DM7_PPLUS_Integration.Implementierung.Server;
using DM7_PPLUS_Integration.Implementierung.Shared;

namespace DM7_PPLUS_Integration.Implementierung.Client
{
    public class LoopbackFactory : Schicht_2_Proxy_Factory
    {
        private readonly DM7_PPLUS_Host _host;
        private readonly int _schicht;
        private readonly DisposeGroup _disposegroup;

        public LoopbackFactory(DM7_PPLUS_Host host, int schicht)
        {
            _host = host;
            _schicht = schicht;
            _disposegroup = new DisposeGroup();
        }

        internal LoopbackFactory(DM7_PPLUS_Host host, int schicht, DisposeGroup disposegroup)
        {
            _host = host;
            _schicht = schicht;
            _disposegroup = disposegroup;
        }

        public Task<Tuple<Schicht_2_Protokoll__Verbindungsaufbau, Schicht_2_Protokoll__API_Version_unabhaengige_Uebertragung>> Connect_Schicht_2(string networkAddress, string credentials, Log log, CancellationToken cancellationToken_Verbindung)
        {
            var task =
                new Task<Tuple<Schicht_2_Protokoll__Verbindungsaufbau,
                    Schicht_2_Protokoll__API_Version_unabhaengige_Uebertragung>>(() =>
                    {

                        Schicht_2_Protokoll__Verbindungsaufbau service;
                        Schicht_2_Protokoll__API_Version_unabhaengige_Uebertragung data;

                        if (_schicht == 2)
                        {
                            service = _host.Schicht_2_Service;
                            data = _host.Schicht_2_Data;
                        }
                        else
                        {
                            service = new Service_Proxy(_host.Schicht_3_Service, _disposegroup);
                            data = new Data_Proxy(_host.Schicht_3_Data, _disposegroup);
                        }

                        return new Tuple<Schicht_2_Protokoll__Verbindungsaufbau, Schicht_2_Protokoll__API_Version_unabhaengige_Uebertragung>(service, data);
                    });

            task.RunSynchronously();
            return task;
        }
    }
}