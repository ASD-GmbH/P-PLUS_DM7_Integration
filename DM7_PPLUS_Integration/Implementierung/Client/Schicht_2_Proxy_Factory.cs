using System;
using System.Threading;
using System.Threading.Tasks;
using DM7_PPLUS_Integration.Implementierung.Protokoll;

namespace DM7_PPLUS_Integration.Implementierung.Client
{
    public interface Schicht_2_Proxy_Factory
    {
        Task<Tuple<Schicht_2_Protokoll__Verbindungsaufbau, Schicht_2_Protokoll__API_Version_unabhaengige_Uebertragung>> Connect_Schicht_2(string networkAddress, string credentials, Log log, CancellationToken cancellationToken_Verbindung);
    }
}