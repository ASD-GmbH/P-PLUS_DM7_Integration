using System;
using System.Threading;
using System.Threading.Tasks;
using DM7_PPLUS_Integration.Implementierung.Protokoll;

namespace DM7_PPLUS_Integration.Implementierung.Client
{
    public interface Ebene_2_Proxy_Factory
    {
        Task<Tuple<Ebene_2_Protokoll__Verbindungsaufbau, Ebene_2_Protokoll__API_Level_unabhaengige_Uebertragung>> Connect_Ebene_2(string networkAddress, Log log, CancellationToken cancellationToken_Verbindung);
    }
}