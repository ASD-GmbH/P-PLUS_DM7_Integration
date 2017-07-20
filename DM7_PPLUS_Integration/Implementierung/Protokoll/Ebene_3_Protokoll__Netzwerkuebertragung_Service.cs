using System;
using System.Threading.Tasks;

namespace DM7_PPLUS_Integration.Implementierung.Protokoll
{
    public interface Ebene_3_Protokoll__Netzwerkuebertragung_Service : IDisposable
    {
        Task<byte[]> ServiceRequest(byte[] request);
    }
}