using System;
using System.Threading.Tasks;
using DM7_PPLUS_Integration.Implementierung.Protokoll;

namespace DM7_PPLUS_Integration.Implementierung.Client
{
    public class NetMqfactory : ProxyFactory
    {
        public Task<Tuple<Ebene_2_Protokoll__Verbindungsaufbau, Ebene_2_Protokoll__API_Level_unabhaengige_Uebertragung>> Connect(string networkAddress, Log log)
        {
            throw new NotImplementedException();
        }
    }
}