using System;
using System.Threading.Tasks;
using DM7_PPLUS_Integration.Implementierung.Protokoll;
using DM7_PPLUS_Integration.Implementierung.Server;

namespace DM7_PPLUS_Integration.Implementierung.Client
{
    public class NetMqfactory : Ebene_2_Proxy_Factory
    {
        public Task<Tuple<Ebene_2_Protokoll__Verbindungsaufbau, Ebene_2_Protokoll__API_Level_unabhaengige_Uebertragung>> Connect_Ebene_2(string networkAddress, Log log)
        {
            var task =
                new Task<Tuple<Ebene_2_Protokoll__Verbindungsaufbau,
                    Ebene_2_Protokoll__API_Level_unabhaengige_Uebertragung>>(() =>
                {
                    var client = new NetMQ_Client(networkAddress, log);
                    var serializer = new Data_Proxy(client);
                    var connector = new Service_Proxy(client);

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