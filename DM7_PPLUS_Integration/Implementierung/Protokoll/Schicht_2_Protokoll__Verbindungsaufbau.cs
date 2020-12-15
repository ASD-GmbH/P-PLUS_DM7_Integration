using System;
using System.Threading.Tasks;

namespace DM7_PPLUS_Integration.Implementierung.Protokoll
{
    public interface Schicht_2_Protokoll__Verbindungsaufbau : IDisposable
    {
        Task<ConnectionResult> Connect_Schicht_1(string credentials, int maxApiVersion, int minApiVersion);
    }
}