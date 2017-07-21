using System;
using System.Threading.Tasks;
using DM7_PPLUS_Integration.Implementierung.Protokoll;
using DM7_PPLUS_Integration.Implementierung.Shared;

namespace DM7_PPLUS_Integration.Implementierung.Client
{
    internal class NetMQ_Factory :  Ebene_2_Proxy_Factory
    {
        private readonly DisposeGroup _disposegroup;

        public NetMQ_Factory(DisposeGroup disposegroup)
        {
            _disposegroup = disposegroup;
        }

        public Task<Tuple<Ebene_2_Protokoll__Verbindungsaufbau, Ebene_2_Protokoll__API_Level_unabhaengige_Uebertragung>> Connect_Ebene_2(string networkAddress, Log log)
        {
            var task =
                new Task<Tuple<Ebene_2_Protokoll__Verbindungsaufbau,
                    Ebene_2_Protokoll__API_Level_unabhaengige_Uebertragung>>(() =>
                {
                    var client = new NetMQ_Client(networkAddress, log, _disposegroup);
                    var serializer = new Data_Proxy(client, _disposegroup);
                    var connector = new Service_Proxy(client, _disposegroup);

                    return new Tuple<Ebene_2_Protokoll__Verbindungsaufbau,
                        Ebene_2_Protokoll__API_Level_unabhaengige_Uebertragung>(
                        connector, serializer
                    );
                });

            task.RunSynchronously();
            return task;
        }
    }
}