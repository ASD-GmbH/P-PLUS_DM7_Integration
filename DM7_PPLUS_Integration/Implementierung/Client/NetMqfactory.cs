using System;
using System.Threading.Tasks;
using DM7_PPLUS_Integration.Implementierung.Protokoll;
using DM7_PPLUS_Integration.Implementierung.Server;

namespace DM7_PPLUS_Integration.Implementierung.Client
{
    public class NetMqfactory : ProxyFactory
    {
        public Task<Tuple<Ebene_2_Protokoll__Verbindungsaufbau, Ebene_2_Protokoll__API_Level_unabhaengige_Uebertragung>> Connect(string networkAddress, Log log)
        {
            var task =
                new Task<Tuple<Ebene_2_Protokoll__Verbindungsaufbau,
                    Ebene_2_Protokoll__API_Level_unabhaengige_Uebertragung>>(() =>
                {
                    var client = new NetMQ_Client(networkAddress);
                    var serializer = new Serialization_Proxy(client);
                    var connector = new ServiceClient(client);

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