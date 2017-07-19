using System;
using System.Threading.Tasks;

namespace DM7_PPLUS_Integration.Implementierung.Protokoll
{
    public interface Ebene_2_Protokoll__Verbindungsaufbau : IDisposable
    {
        Task<ConnectionResult> Connect(string login, int maxApiLevel, int minApiLevel);
    }
}